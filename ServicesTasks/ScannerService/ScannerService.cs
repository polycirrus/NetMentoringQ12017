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

namespace ScannerService
{
    class ScannerService : ServiceBase
    {
        private static readonly TimeSpan ScanTimeout = TimeSpan.FromMinutes(2);

        private CancellationTokenSource tokenSource;
        private ManualResetEventSlim changesDetected;
        private FileSystemWatcher watcher;
        private Task workerTask;

        protected override void OnStart(string[] args)
        {
            tokenSource = new CancellationTokenSource();

            changesDetected = new ManualResetEventSlim(true);

            var watcher = new FileSystemWatcher(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            watcher.Changed += WatcherOnChanged;
            watcher.Created += WatcherOnCreated;
            watcher.Renamed += WatcherOnRenamed;

            workerTask = Task.Run(() => WorkerRoutine(), tokenSource.Token);

            watcher.EnableRaisingEvents = true;

            base.OnStart(args);
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

        protected override void OnStop()
        {
            try
            {
                tokenSource.Cancel();
                if (!workerTask.IsCompleted)
                    workerTask.Wait();

                base.OnStop();
            }
            finally
            {
                changesDetected.Dispose();

                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
        }

        private void WorkerRoutine()
        {
            Thread.Sleep(20000);
            while (!tokenSource.IsCancellationRequested)
            {
                changesDetected.Wait(ScanTimeout, tokenSource.Token);
                tokenSource.Token.ThrowIfCancellationRequested();
                changesDetected.Reset();

                CombineFiles();
            }
            tokenSource.Token.ThrowIfCancellationRequested();
        }

        public static void CombineFiles()
        {
            var indexedFiles = GetFilesWithIndexes();
            var batches = SplitFilesIntoBatches(indexedFiles);

            foreach (var batch in batches)
            {
                var fileName = CreateFileNameForBatch(batch);
                CreateDocument(fileName, batch.Select(x => x.Item2.FullName).ToList());
            }
        }

        private static List<IEnumerable<Tuple<int, FileInfo>>> SplitFilesIntoBatches(Tuple<int, FileInfo>[] indexedFiles)
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

            if (DateTime.Now - indexedFiles.Last().Item2.LastWriteTime > ScanTimeout)
                batches.Add(indexedFiles.Slice(batchStart, indexedFiles.Length - batchStart));

            return batches;
        }

        private static Tuple<int, FileInfo>[] GetFilesWithIndexes()
        {
            var directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            var files = directory.GetFiles();
            var regex = new Regex(@"^img_(\d+)\..+$");

            return files
                .Select(file => new Tuple<Match, FileInfo>(regex.Match(file.Name), file))
                .Where(tuple => tuple.Item1.Success)
                .Select(tuple => new Tuple<int, FileInfo>(Convert.ToInt32(tuple.Item1.Groups[1].Value), tuple.Item2))
                .OrderBy(x => x.Item1)
                .ToArray();
        }

        private static string CreateFileNameForBatch(IEnumerable<Tuple<int, FileInfo>> batch)
        {
            var fileName = $"{batch.First().Item1}-{batch.Last().Item1}.pdf";
            if (File.Exists(fileName))
            {
                int count = 1;
                do
                {
                    fileName = $"{batch.First().Item1}-{batch.Last().Item1}_({count++}).pdf";
                } while (File.Exists(fileName));
            }
            return fileName;
        }

        private static void CreateDocument(string documentPath, IEnumerable<string> imagePaths)
        {
            var bitmaps = GetBitmaps(imagePaths);
            if (bitmaps.Count < 1)
                return;

            //WriteBitmapsToXps(documentPath, bitmaps);
            WriteBitmapsToPdf(documentPath, bitmaps);

            //FileHelper.Delete(imagePaths);
        }

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
                        //foreach (var bitmapImage in bitmaps)
                        //{
                        //    var image = iTextSharp.text.Image.GetInstance(bitmapImage.UriSource);
                        //    image.SetAbsolutePosition(0, 0);
                        //    writer.DirectContent.AddImage(image);
                        //    document.NewPage();
                        //}

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
    }
}
