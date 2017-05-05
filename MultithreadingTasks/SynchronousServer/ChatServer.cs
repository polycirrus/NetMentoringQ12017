using Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynchronousServer
{
    class ChatServer
    {
        private static readonly int MessageHistorySize = 10;

        private IServerConnector[] connectors;
        private BlockingCollection<Message> pendingMessages;
        private ConcurrentQueue<Message> messageHistory;

        private CancellationToken token;
        private Task messageSenderTask;

        private Action<string> Log;

        public ChatServer(Action<string> log)
        {
            connectors = new IServerConnector[] {new NamedPipeServerConnector("serverPipe")/*, new SocketServerConnector()*/};
            Log = log;
        }

        public void Start()
        {
            foreach (var connector in connectors)
            {
                connector.Init();

                connector.Connected += OnConnected;
                connector.MessageReceived += OnMessageReceived;
                connector.Disconnected += OnDisconnected;
            }

            pendingMessages = new BlockingCollection<Message>();
            messageHistory = new ConcurrentQueue<Message>();

            token = new CancellationTokenSource().Token;
            messageSenderTask = Task.Run(() => MessageSender(), token);
        }

        private void MessageSender()
        {
            while (!token.IsCancellationRequested)
            {
                var message = pendingMessages.Take(token);
                foreach (var connector in connectors)
                    connector.Broadcast(message);
            }
            token.ThrowIfCancellationRequested();
        }

        private void OnConnected(object sender, ConnectionEventArgs e)
        {
            Log($"{e.UserId} connected via {sender.GetType().Name}.");

            Message[] messages;
            lock (messageHistory)
            {
                messages = messageHistory.ToArray();
            }

            foreach (var message in messages)
            {
                ((IServerConnector)sender).Send(e.UserId, message);
            }
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Log($"{e.Message.UserId} says: {e.Message.Text}");

            pendingMessages.Add(e.Message);

            lock (messageHistory)
            {
                if (messageHistory.Count >= 10)
                {
                    Message oldMessage;
                    messageHistory.TryDequeue(out oldMessage);
                }
                messageHistory.Enqueue(e.Message);
            }
        }

        private void OnDisconnected(object sender, ConnectionEventArgs e)
        {
            Log($"{e.UserId} disconnected.");
        }

        public void Stop()
        {
            foreach (var connector in connectors)
            {
                connector.Stop();
            }
        }
    }
}
