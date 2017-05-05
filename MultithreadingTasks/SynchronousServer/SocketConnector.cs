using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using System.Net.Sockets;
using System.Net;

namespace SynchronousServer
{
    public class SocketConnector : IConnector
    {
        private Socket socket;

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
            //socket.Beg
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
