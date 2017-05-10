using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;

namespace SynchronousClient
{
    public class NamedPipeClientConnector : IClientConnector
    {
        private Action<string> logAction;
        private NamedPipeClientStream pipeClient;
        private CancellationTokenSource tokenSource;

        public event EventHandler<ConnectionEventArgs> Connected;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<ConnectionEventArgs> Disconnected;

        public string UserId { get; private set; }

        public NamedPipeClientConnector(Action<string> logAction)
        {
            this.logAction = logAction;
        }

        public void Connect(string userId)
        {
            pipeClient = new NamedPipeClientStream(".", "serverPipe", PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            pipeClient.Connect();

            var stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, new Authentication() {UserId = userId});
            var data = stream.ToArray();

            pipeClient.Write(data, 0, data.Length);
            pipeClient.WaitForPipeDrain();

            UserId = userId;
            if (Connected != null)
                Connected.Invoke(this, new ConnectionEventArgs() {UserId = userId});
            else
                logAction($"Connected as {userId}");

            tokenSource = new CancellationTokenSource();
            Task.Run(() => Listen(), tokenSource.Token);
        }

        private void Listen()
        {
            while (!tokenSource.Token.IsCancellationRequested)
            {
                while (pipeClient.IsConnected)
                {
                    try
                    {
                        var receivedObject = new BinaryFormatter().Deserialize(pipeClient);
                        HandleReceivedObject(receivedObject);
                    }
                    catch (SerializationException)
                    {
                        continue;
                    }
                }
            }
            tokenSource.Token.ThrowIfCancellationRequested();
        }

        private void HandleReceivedObject(object receivedObject)
        {
            var message = receivedObject as Message;
            if (message != null)
            {
                if (MessageReceived != null)
                    MessageReceived.Invoke(this, new MessageReceivedEventArgs() {Message = message});
                else
                    logAction($"{message.UserId} says: {message.Text}");
            }

            var notification = receivedObject as Notification;
            if (notification != null)
                logAction($"Server says: {notification.Text}");
        }

        public void Disconnect()
        {
            if (!tokenSource.IsCancellationRequested)
                tokenSource.Cancel();

            var connected = pipeClient.IsConnected;
            pipeClient.Dispose();

            if (connected)
                Disconnected?.Invoke(this, new ConnectionEventArgs() {UserId = UserId});
        }

        public void Send(string message)
        {
            if (pipeClient == null || !pipeClient.IsConnected)
            {
                logAction("You must be connected to send a message");
                return;
            }

            var stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, new Message() {UserId = UserId, Text = message});
            var data = stream.ToArray();

            pipeClient.Write(data, 0, data.Length);
            pipeClient.WaitForPipeDrain();
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}