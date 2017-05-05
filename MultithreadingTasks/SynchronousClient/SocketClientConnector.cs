using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Infrastructure;

namespace SynchronousClient
{
    public class SocketClientConnector : IClientConnector
    {
        private Action<string> logAction;
        private Socket socket;
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
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            //var endpoint = new IPEndPoint(IPAddress.Any, 0);
            //socket.Bind(endpoint);
            //socket.Listen(10);

            var serverEndpoint = new IPEndPoint(IPAddress.Loopback, 17643);
            socket.Connect(serverEndpoint);

            var stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, new Authentication() { UserId = userId });
            var data = stream.ToArray();

            var sentBytes = socket.Send(data);
            if (sentBytes < data.Length)
                throw new SocketException();

            Connected?.Invoke(this, new ConnectionEventArgs() {UserId = userId});
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Send(string message)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}