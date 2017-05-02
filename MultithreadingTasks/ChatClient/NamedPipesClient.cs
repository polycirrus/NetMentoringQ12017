using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    public class NamedPipesClient : IClient
    {
        private NamedPipeClientStream stream;
        private StreamWriter writer;

        public string ClientId { get; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public NamedPipesClient(string serverPipeName, string clientId)
        {
            ClientId = clientId;
            stream = new NamedPipeClientStream(serverPipeName, clientId, PipeDirection.InOut);
            writer = new StreamWriter(stream);
        }

        public void Connect()
        {
            stream.Connect();
        }

        public void SendMessage(string messageText)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}
