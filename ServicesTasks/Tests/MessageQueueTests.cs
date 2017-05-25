using System;
using System.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class MessageQueueTests
    {
        private static readonly string QueueName = @".\Private$\TestQueue";

        [TestMethod]
        public void BeginRecieveTests()
        {
            MessageQueue queue;
            if (MessageQueue.Exists(QueueName))
                queue = new MessageQueue(QueueName);
            else
                queue = MessageQueue.Create(QueueName);

            queue.Formatter = new XmlMessageFormatter(new[] { typeof(string) });

            using (queue)
            {
                var result = queue.BeginReceive(TimeSpan.FromSeconds(10), null,
                    asyncResult => Console.WriteLine("the unthinkable has happened"));
            }
        }
    }
}
