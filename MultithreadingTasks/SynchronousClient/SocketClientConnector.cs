using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;

namespace SynchronousClient
{
    public class SocketClientConnector : IClientConnector
    {
        private Action<string> logAction;
        private TcpClient socket;
        private CancellationTokenSource tokenSource;

        public event EventHandler<ConnectionEventArgs> Connected;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<ConnectionEventArgs> Disconnected;

        public string UserId { get; private set; }

        public SocketClientConnector(Action<string> logAction)
        {
            this.logAction = logAction;
        }

        public void Connect(string userId)
        {
            socket = new TcpClient();
            var serverEndpoint = new IPEndPoint(IPAddress.Loopback, 17643);
            socket.Connect(serverEndpoint);

            Send(new Authentication() {UserId = userId});

            UserId = userId;
            Connected?.Invoke(this, new ConnectionEventArgs() {UserId = userId});

            tokenSource = new CancellationTokenSource();
            Task.Run(ReadFromServer, tokenSource.Token);
        }

        private async Task ReadFromServer()
        {
            try
            {
                while (!tokenSource.Token.IsCancellationRequested)
                {
                    var data = new byte[1024];
                    var bytesReceived = 0;
                    bytesReceived += await socket.GetStream().ReadAsync(data, 0, data.Length, tokenSource.Token);
                    while (socket.Available > 0)
                        bytesReceived += await socket.GetStream().ReadAsync(data, bytesReceived, data.Length - bytesReceived, tokenSource.Token);

                    var obj = new BinaryFormatter().Deserialize(new MemoryStream(data.Take(bytesReceived).ToArray()));
                    HandleReceivedObject(obj);
                }
                tokenSource.Token.ThrowIfCancellationRequested();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void HandleReceivedObject(object receivedObject)
        {
            var message = receivedObject as Message;
            if (message != null)
                HandleMessage(message);

            var messages = receivedObject as IEnumerable<Message>;
            if (messages != null)
                foreach (var msg in messages)
                    HandleMessage(msg);

            var notification = receivedObject as Notification;
            if (notification != null)
                logAction($"Server says: {notification.Text}");
        }

        private void HandleMessage(Message message)
        {
            if (MessageReceived != null)
                MessageReceived.Invoke(this, new MessageReceivedEventArgs() {Message = message});
            else
                logAction($"{message.UserId} says: {message.Text}");
        }

        public void Disconnect()
        {
            if (socket.Connected)
            {
                socket.Close();
                Disconnected?.Invoke(this, new ConnectionEventArgs() {UserId = UserId});
            }
        }

        public void Send(string message)
        {
            Send(new Message() {UserId = UserId, Text = message});
        }

        private void Send(object data)
        {
            var memstr = new MemoryStream();
            new BinaryFormatter().Serialize(memstr, data);
            socket.GetStream().Write(memstr.ToArray(), 0, (int)memstr.Length);
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}