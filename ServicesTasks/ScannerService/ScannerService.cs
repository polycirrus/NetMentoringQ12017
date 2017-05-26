using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MoreLinq;
using Image = System.Windows.Controls.Image;
using System.Diagnostics;
using System.Messaging;
using Shared;
using Timer = System.Timers.Timer;

namespace ScannerService
{
    class ScannerService : ServiceBase
    {
        private static readonly TimeSpan DefaultScanTimeout = TimeSpan.FromMinutes(2);
        private static readonly double StateUpdateTimerInterval = 60000;
        private static readonly string MasterQueueName = @".\Private$\MasterServiceQueue";
        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(1);
        private static readonly int DocumentChunkSize = 1048000;

        private ManualResetEventSlim changesDetected;
        private CancellationTokenSource tokenSource;
        private Task workerTask;
        private FileSystemWatcher watcher;

        private TimeSpan scanTimeout;
        private List<string> processedFiles;
        private ScannerStatus status = ScannerStatus.Busy;

        private string serviceName;
        private MessageQueue masterServiceQueue;
        private object masterQueueLock;
        private MessageQueue queue;
        private Task listenerTask;
        private Timer stateUpdateTimer;

        protected override void OnStart(string[] args)
        {
            Thread.Sleep(20000);
            Log("Initializing service...");

            try
            {
                InitializeWorker();
                InitializeWatcher();
                InitializeMessaging();
            }
            catch (Exception exception)
            {
                Log($"An exception has occured during initialization: {exception}");
                throw;
            }

            Log("Initialization complete.");
            base.OnStart(args);
        }

        #region Init

        private void InitializeMessaging()
        {
            serviceName = $"ScannerQueue{Guid.NewGuid().ToString("N")}";
            Log($"Service name is {serviceName}");

            var queueName = $".\\Private$\\{serviceName}";
            if (MessageQueue.Exists(queueName))
                queue = new MessageQueue($".\\Private$\\{serviceName}");
            else
                queue = MessageQueue.Create(queueName);

            masterServiceQueue = new MessageQueue(MasterQueueName);
            masterServiceQueue.DefaultPropertiesToSend.ResponseQueue = queue;

            masterQueueLock = new object();

            listenerTask = Task.Run(new Action(ListenerRoutine), tokenSource.Token);

            stateUpdateTimer = new Timer(StateUpdateTimerInterval);
            stateUpdateTimer.Elapsed += (sender, eventArgs) => SendStatusUpdate();
            stateUpdateTimer.AutoReset = true;
            stateUpdateTimer.Start();
        }

        private void InitializeWatcher()
        {
            watcher = new FileSystemWatcher(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            watcher.Changed += WatcherOnChanged;
            watcher.Created += WatcherOnCreated;
            watcher.Renamed += WatcherOnRenamed;

            watcher.EnableRaisingEvents = true;
        }

        private void InitializeWorker()
        {
            changesDetected = new ManualResetEventSlim(true);
            processedFiles = new List<string>();
            scanTimeout = DefaultScanTimeout;
            tokenSource = new CancellationTokenSource();
            workerTask = Task.Run(() => WorkerRoutine(), tokenSource.Token);
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            changesDetected.Set();
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            changesDetected.Set();
        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs renamedEventArgs)
        {
            changesDetected.Set();
        }

        #endregion

        protected override void OnStop()
        {
            Log("Stopping service...");

            try
            {
                stateUpdateTimer.Stop();
                watcher.EnableRaisingEvents = false;

                tokenSource.Cancel();
                Task.WaitAll(new[] {workerTask, listenerTask}.Where(task => !task.IsCompleted).ToArray());

                Log("Service stopped.");
                base.OnStop();
            }
            finally
            {
                changesDetected.Dispose();
                watcher.Dispose();
                stateUpdateTimer.Dispose();
                queue.Dispose();
                masterServiceQueue.Dispose();
            }
        }

        #region Scanned files processing

        private void WorkerRoutine()
        {
            Thread.Sleep(20000);
            while (!tokenSource.IsCancellationRequested)
            {
                Log("Waiting for files...");
                status = ScannerStatus.Idle;
                changesDetected.Wait(scanTimeout, tokenSource.Token);
                status = ScannerStatus.Busy;

                tokenSource.Token.ThrowIfCancellationRequested();
                changesDetected.Reset();

                try
                {
                    CombineFiles();
                }
                catch (Exception e)
                {
                    //EventLog.WriteEntry(e.ToString(), EventLogEntryType.Error);
                    Log($"An exception has occured during file processing: {e}");
                }
            }
            tokenSource.Token.ThrowIfCancellationRequested();
        }

        public void CombineFiles()
        {
            var indexedFiles = GetFilesWithIndexes();
            if (!indexedFiles.Any())
            {
                Log("No files to combine were found.");
                return;
            }
            Log($"Combining files: {string.Join(", ", indexedFiles.Select(tuple => tuple.Item2.Name))}");

            var batches = SplitFilesIntoBatches(indexedFiles);

            foreach (var batch in batches)
            {
                var fileName = CreateFileNameForBatch(batch);
                Log($"Combining files {string.Join(", ", batch.Select(tuple => tuple.Item2.Name))} into {fileName}");
                CreateDocument(fileName, batch.Select(x => x.Item2.FullName).ToList());
                Log($"Document {fileName} created");
                SendFile(fileName);
            }
        }

        private List<IEnumerable<Tuple<int, FileInfo>>> SplitFilesIntoBatches(Tuple<int, FileInfo>[] indexedFiles)
        {
            var batches = new List<IEnumerable<Tuple<int, FileInfo>>>();

            int batchStart = 0;
            for (int i = 1; i < indexedFiles.Length; i++)
            {
                var previousFileIndex = indexedFiles[i - 1].Item1;
                var currentFileIndex = indexedFiles[i].Item1;

                if (currentFileIndex - previousFileIndex > 1)
                {
                    batches.Add(indexedFiles.Slice(batchStart, i - batchStart));
                    batchStart = i;
                }
            }

            if (DateTime.Now - indexedFiles.Last().Item2.LastWriteTime > scanTimeout)
                batches.Add(indexedFiles.Slice(batchStart, indexedFiles.Length - batchStart));

            return batches;
        }

        private Tuple<int, FileInfo>[] GetFilesWithIndexes()
        {
            var directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            var files = directory.GetFiles();
            var regex = new Regex(@"^img_(\d+)\..+$");

            return files
                .Select(file => new Tuple<Match, FileInfo>(regex.Match(file.Name), file))
                .Where(tuple => tuple.Item1.Success && !processedFiles.Contains(tuple.Item2.FullName))
                .Select(tuple => new Tuple<int, FileInfo>(Convert.ToInt32(tuple.Item1.Groups[1].Value), tuple.Item2))
                .OrderBy(x => x.Item1)
                .ToArray();
        }

        private static string CreateFileNameForBatch(IEnumerable<Tuple<int, FileInfo>> batch)
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fileName = Path.Combine(directory, $"{batch.First().Item1}-{batch.Last().Item1}.pdf");
            if (File.Exists(fileName))
            {
                int count = 1;
                do
                {
                    fileName = Path.Combine(directory, $"{batch.First().Item1}-{batch.Last().Item1}_({count++}).pdf");
                } while (File.Exists(fileName));
            }
            return fileName;
        }

        private void CreateDocument(string documentPath, IEnumerable<string> imagePaths)
        {
            var bitmaps = GetBitmaps(imagePaths);
            if (bitmaps.Count < 1)
                return;

            //WriteBitmapsToXps(documentPath, bitmaps);
            WriteBitmapsToPdf(documentPath, bitmaps);

            processedFiles.AddRange(imagePaths);
        }

        #region Pdf

        private static void WriteBitmapsToPdf(string documentPath, List<BitmapImage> bitmaps)
        {
            var images = bitmaps.Select(bitmapImage => iTextSharp.text.Image.GetInstance(bitmapImage.UriSource));

            using (var fileStream = new FileStream(documentPath, FileMode.Create))
            {
                using (var document = new iTextSharp.text.Document(new Rectangle(images.Max(image => image.Width), images.Max(image => image.Height))))
                {
                    using (var writer = PdfWriter.GetInstance(document, fileStream))
                    {
                        document.Open();

                        foreach (var image in images)
                        {
                            image.SetAbsolutePosition(0, 0);
                            writer.DirectContent.AddImage(image);
                            document.NewPage();
                        }

                        writer.Flush();
                        document.Close();
                    }
                }
            }
        }

        private static List<BitmapImage> GetBitmaps(IEnumerable<string> files)
        {
            var bitmaps = new List<BitmapImage>();
            foreach (var file in files)
            {
                try
                {
                    bitmaps.Add(new BitmapImage(new Uri(file)));
                }
                catch (NotSupportedException)
                {
                }
            }

            return bitmaps;
        }

        #endregion

        #region Xps

        private static void WriteBitmapsToXps(string filePath, List<BitmapImage> bitmaps)
        {
            var pageWidth = bitmaps.Max(bitmap => bitmap.Width);
            var pageHeight = bitmaps.Max(bitmap => bitmap.Height);
            var pageSize = new Size(pageWidth, pageHeight);

            XpsDocument document = null;
            FileHelper.Retry(() => document = new XpsDocument(filePath, FileAccess.ReadWrite));

            using (document)
            {
                var writer = XpsDocument.CreateXpsDocumentWriter(document);

                var collator = writer.CreateVisualsCollator();
                collator.BeginBatchWrite();
                foreach (var bitmap in bitmaps)
                {
                    var image = CreateImage(bitmap, pageSize);
                    collator.Write(image);
                }
                collator.EndBatchWrite();
            }
        }

        private static Image CreateImage(BitmapImage bitmap, Size pageSize)
        {
            Image image = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                image = new Image();
                image.Source = bitmap;
                image.Arrange(new Rect(pageSize));
            });

            return image;
        }

        #endregion

        #endregion

        #region Messaging

        private void ListenerRoutine()
        {
            Thread.Sleep(20000);

            queue.Formatter = new XmlMessageFormatter(new[] { typeof(RequestStateUpdateCommand), typeof(UpdateConfigurationCommand) });

            using (queue)
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    Message message;
                    try
                    {
                        message = queue.Receive(ReceiveTimeout);
                    }
                    catch (MessageQueueException exception)
                    {
                        if (exception.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                            continue;

                        Log(exception.ToString());
                        throw;
                    }

                    try
                    {
                        HandleMessage(message);
                    }
                    catch (Exception exception)
                    {
                        Log($"An exception has occured during message handling: {exception}");
                        continue;
                    }
                }
            }

        }

        private void HandleMessage(Message message)
        {
            Log($"Recieved '{message}'");

            var messageBody = message.Body;
            var requestStateUpdateCommand = messageBody as RequestStateUpdateCommand;
            if (requestStateUpdateCommand != null)
            {
                SendStatusUpdate();
                return;
            }

            var updateConfigurationCommand = messageBody as UpdateConfigurationCommand;
            if (updateConfigurationCommand != null)
            {
                UpdateConfiguration(updateConfigurationCommand.Configuration);
            }

        }

        private void UpdateConfiguration(ScannerConfiguration configuration)
        {
            scanTimeout = configuration.ScanTimeout;
            Log($"Updated scan timeout to '{configuration.ScanTimeout}.'");
        }

        private void SendStatusUpdate()
        {
            Log("Sending status update to the master service.");
            Send(new ScannerState()
            {
                Configuration = new ScannerConfiguration() { ScanTimeout = scanTimeout },
                Status = status
            });
        }

        private void SendFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            Log($"Sending file '{fileName}' to the master service...");

            var fileStream = new FileStream(filePath, FileMode.Open);
            var chunkCount = fileStream.Length / DocumentChunkSize;
            if (chunkCount * DocumentChunkSize < fileStream.Length)
                chunkCount++;

            var chunkNumber = 0;
            var chunk = new DocumentChunk()
            {
                ChunkNumber = chunkNumber,
                Data = new byte[DocumentChunkSize],
                FileName = fileName,
                TotalChunks = (int)chunkCount
            };
            var bytesRead = 0;
            while ((bytesRead = fileStream.Read(chunk.Data, 0, DocumentChunkSize)) > 0)
            {
                if (bytesRead < DocumentChunkSize)
                    chunk.Data = chunk.Data.Take(bytesRead).ToArray();

                Log($"Sending file '{fileName}' chunk #{chunkNumber}...");
                Send(chunk);
                Log($"Chunk #{chunkNumber} sent.");

                chunkNumber++;
                if (bytesRead < DocumentChunkSize || chunkNumber >= chunkCount)
                    break;

                chunk = new DocumentChunk()
                {
                    ChunkNumber = chunkNumber,
                    Data = new byte[DocumentChunkSize],
                    FileName = fileName,
                    TotalChunks = (int)chunkCount
                };
            }

            Log($"File '{fileName}' sent to the master service.");
        }

        private void Send(object message)
        {
            lock (masterQueueLock)
            {
                masterServiceQueue.Send(message);
            }
        }

        #endregion

        private void Log(string entry)
        {
            using (var writer = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "log.txt"), true))
            {
                writer.WriteLine($"{DateTime.Now}: {entry}{Environment.NewLine}");
            }
        }
    }
}
