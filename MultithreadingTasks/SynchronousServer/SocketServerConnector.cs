using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace SynchronousServer
{
    public class SocketServerConnector : IServerConnector
    {
        private TcpListener listener;
        private CancellationTokenSource tokenSource;
        private ConcurrentDictionary<TcpClient, string> clients; 

        public event EventHandler<ConnectionEventArgs> Connected;
        public event EventHandler<ConnectionEventArgs> Disconnected;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Broadcast(Message message)
        {
            Broadcast((object)message);
        }

        private void Broadcast(object data)
        {
            var connections = clients.Keys.ToArray();
            var tasks = connections.Select(connection => Task.Run(() => Send(connection, data)));
            Task.WaitAll(tasks.ToArray());
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 17643);
            listener = new TcpListener(endpoint);
            listener.Start();

            clients = new ConcurrentDictionary<TcpClient, string>();

            tokenSource = new CancellationTokenSource();
            Task.Run(() => Listen(), tokenSource.Token);
        }

        private void Listen()
        {
            while (!tokenSource.IsCancellationRequested)
            {
                var connection = listener.AcceptTcpClient();
                Task.Run(() => ReadFromClient(connection), tokenSource.Token);
            }
            tokenSource.Token.ThrowIfCancellationRequested();
        }

        private async void ReadFromClient(TcpClient connection)
        {
            try
            {
                while (!tokenSource.IsCancellationRequested && connection.Connected)
                {
                    var data = new byte[1024];
                    var bytesReceived = 0;
                    bytesReceived += await connection.GetStream().ReadAsync(data, 0, data.Length, tokenSource.Token);
                    while (connection.Available > 0)
                        bytesReceived += await connection.GetStream().ReadAsync(data, bytesReceived, data.Length - bytesReceived, tokenSource.Token);

                    if (bytesReceived <= 0)
                        break;

                    var receivedObject = new BinaryFormatter().Deserialize(new MemoryStream(data.Take(bytesReceived).ToArray()));
                    HandleReceivedObject(connection, receivedObject);
                }
                tokenSource.Token.ThrowIfCancellationRequested();
            }
            finally
            {
                connection.Close();

                string userId;
                if (clients.TryRemove(connection, out userId))
                    Disconnected?.Invoke(this, new ConnectionEventArgs() { UserId = userId });
            }
        }

        private void HandleReceivedObject(TcpClient connection, object receivedObject)
        {
            var message = receivedObject as Message;
            if (message != null)
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs() { Message = message });

            var authentication = receivedObject as Authentication;
            if (authentication != null)
            {
                clients[connection] = authentication.UserId;
                Connected?.Invoke(this, new ConnectionEventArgs() { UserId = authentication.UserId });
            }
        }

        public void Send(string userId, Message message)
        {
            var connection = clients.Where(kvp => kvp.Value == userId).Select(kvp => kvp.Key).FirstOrDefault();
            Send(connection, message);
        }

        public void Send(string userId, IEnumerable<Message> messages)
        {
            var connection = clients.Where(kvp => kvp.Value == userId).Select(kvp => kvp.Key).FirstOrDefault();
            Send(connection, messages.ToArray());
        }

        private void Send(TcpClient connection, object data)
        {
            var memstr = new MemoryStream();
            new BinaryFormatter().Serialize(memstr, data);
            connection.GetStream().Write(memstr.ToArray(), 0, (int)memstr.Length);
        }

        public void Stop()
        {
            Broadcast(new Notification() {Text = "The server is shutting down."});
            tokenSource.Cancel();
        }
    }
}
