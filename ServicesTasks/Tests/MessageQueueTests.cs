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
        public void RecieveTests()
        {
            MessageQueue queue;
            if (MessageQueue.Exists(QueueName))
                queue = new MessageQueue(QueueName);
            else
                queue = MessageQueue.Create(QueueName);

            queue.Formatter = new XmlMessageFormatter(new[] { typeof(string) });

            using (queue)
            {
                var message = queue.Receive(TimeSpan.FromSeconds(5));
                Console.WriteLine((string)message?.Body ?? "No message recieved.");
            }
        }
    }
}
