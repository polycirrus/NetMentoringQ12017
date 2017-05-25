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

namespace MasterService
{
    public class MasterService : ServiceBase
    {
        private static readonly string QueueName = @".\Private$\MasterServiceQueue";

        private CancellationTokenSource tokenSource;
        private FileSystemWatcher watcher;
        private Task workerTask;

        protected override void OnStart(string[] args)
        {
            tokenSource = new CancellationTokenSource();

            var configFilePath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
            var fileName = Path.GetFileName(configFilePath);
            var directoryPath = Path.GetDirectoryName(configFilePath);
            watcher = new FileSystemWatcher(directoryPath, fileName);

            base.OnStart(args);
        }

        private void WorkerRoutine()
        {
            MessageQueue queue;
            if (MessageQueue.Exists(QueueName))
                queue = new MessageQueue(QueueName);
            else
                queue = MessageQueue.Create(QueueName);

            queue.Formatter = new XmlMessageFormatter(new[] {typeof (string)});

            using (queue)
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    queue.Rec
                }
            }
        }
    }
}
