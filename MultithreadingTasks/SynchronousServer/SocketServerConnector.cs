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
        private Socket socket;
        private CancellationTokenSource tokenSource;
        private ConcurrentDictionary<Socket, string> clients; 

        public event EventHandler<ConnectionEventArgs> Connected;
        public event EventHandler<ConnectionEventArgs> Disconnected;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Broadcast(Message message)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            var endpoint = new IPEndPoint(IPAddress.Any, 17643);
            socket.Bind(endpoint);
            socket.Listen(10);

            tokenSource = new CancellationTokenSource();
            Task.Run(() => Listen(), tokenSource.Token);
        }

        private void Listen()
        {
            while (!tokenSource.IsCancellationRequested)
            {
                var connection = socket.Accept();
                Task.Run(() => ReadFromClient(connection), tokenSource.Token);
            }
            tokenSource.Token.ThrowIfCancellationRequested();
        }

        private void ReadFromClient(Socket connection)
        {
            using (connection)
            {
                while (!tokenSource.IsCancellationRequested && connection.Connected)
                {
                    var data = new byte[1024];
                    var bytesReceived = socket.Receive(data);

                    if (bytesReceived <= 0)
                        break;

                    var receivedObject = new BinaryFormatter().Deserialize(new MemoryStream(data));
                    HandleReceivedObject(socket, receivedObject);
                }
                tokenSource.Token.ThrowIfCancellationRequested();
            }
        }

        private void HandleReceivedObject(Socket connection, object receivedObject)
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
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
