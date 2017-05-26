using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Messaging;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared;

namespace MasterService
{
    public class MasterService : ServiceBase
    {
        private static readonly string QueueName = @".\Private$\MasterServiceQueue";
        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(1);

        private static readonly string LogFileName = "log.txt";

        private static readonly string WorkingDirectoryPathSettingName = "WorkingDirectory";
        private static readonly string ScanTimeoutSettingName = "ScanTimeout";

        private CancellationTokenSource tokenSource;
        private FileSystemWatcher watcher;
        private Task listenerTask;
        private Dictionary<string, ScannerServiceHandler> scannerHandlers;
        private string workingDirectoryPath;
        private TimeSpan? currentScanTimeout;

        private string LogFilePath
            => string.IsNullOrEmpty(workingDirectoryPath)
                ? LogFileName
                : Path.Combine(workingDirectoryPath, LogFileName);

        protected override void OnStart(string[] args)
        {
            Thread.Sleep(20000);
            UpdateWorkingDirectoryPath();
            Log("Initializing service...");

            tokenSource = new CancellationTokenSource();
            listenerTask = Task.Run(new Action(ListenerRoutine));
            scannerHandlers = new Dictionary<string, ScannerServiceHandler>();

            InitializeWatcher();

            Log("Initialization complete.");

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            try
            {
                Log("Stopping service...");

                watcher.EnableRaisingEvents = false;

                tokenSource.Cancel();
                if (!listenerTask.IsCompleted)
                    listenerTask.Wait();

                Log("Service stopped.");
                base.OnStop();
            }
            finally
            {
                watcher.Dispose();
            }
        }

        private void InitializeWatcher()
        {
            var configFilePath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
            Log($"Config file path: {configFilePath}");

            var fileName = Path.GetFileName(configFilePath);
            var directoryPath = Path.GetDirectoryName(configFilePath);
            watcher = new FileSystemWatcher(directoryPath, fileName);

            watcher.Changed += (sender, args) => UpdateConfiguration();

            watcher.EnableRaisingEvents = true;
            Log("Config file path watcher enabled.");
        }

        private void UpdateConfiguration()
        {
            UpdateWorkingDirectoryPath();

            var scanTimeoutSetting = ConfigurationManager.AppSettings[ScanTimeoutSettingName];
            if (string.IsNullOrEmpty(scanTimeoutSetting))
                return;

            TimeSpan scanTimeout;
            if (!TimeSpan.TryParse(scanTimeoutSetting, out scanTimeout))
            {
                Log($"Scan timeout value '{scanTimeout}' is not a valid TimeSpan.");
                return;
            }

            if (currentScanTimeout.HasValue && scanTimeout == currentScanTimeout.Value)
                return;

            currentScanTimeout = scanTimeout;
            Log($"Broadcating new scan timeout '{scanTimeout}' to scanner services.");
            foreach (var scannerHandler in scannerHandlers.Values)
            {
                scannerHandler.UpdateConfiguration(new ScannerConfiguration() {ScanTimeout = scanTimeout});
            }
        }

        private void UpdateWorkingDirectoryPath()
        {
            var workingDirectoryPathSetting = ConfigurationManager.AppSettings[WorkingDirectoryPathSettingName];
            if (!Directory.Exists(workingDirectoryPathSetting))
            {
                Log($"{workingDirectoryPathSetting} is not a valid directory path.");
                return;
            }

            workingDirectoryPath = workingDirectoryPathSetting;
        }

        private void ListenerRoutine()
        {
            Thread.Sleep(20000);
            MessageQueue queue;
            if (MessageQueue.Exists(QueueName))
                queue = new MessageQueue(QueueName);
            else
                queue = MessageQueue.Create(QueueName);

            queue.Formatter = new XmlMessageFormatter(new[] {typeof (string), typeof(DocumentChunk), typeof(ScannerState), typeof(ScannerConfiguration), typeof(TimeSpan)});

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

                    HandleMessage(message);
                }
            }
        }

        private void HandleMessage(Message message)
        {
            if (message.ResponseQueue == null)
            {
                Log($"Recieved message '{message.Body}' with no response queue.");
                return;
            }

            ScannerServiceHandler handler;
            if (!scannerHandlers.TryGetValue(message.ResponseQueue.QueueName, out handler))
            {
                handler = new ScannerServiceHandler(message.ResponseQueue, workingDirectoryPath, Log);
                scannerHandlers[message.ResponseQueue.QueueName] = handler;
            }

            handler.Handle(message);
        }

        private void Log(string entry)
        {
            using (var writer = new StreamWriter(LogFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {entry}{Environment.NewLine}");
            }
        }
    }
}
