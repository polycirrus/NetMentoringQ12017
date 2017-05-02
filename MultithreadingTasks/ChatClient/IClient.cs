using System;

using Infrastructure;

namespace ChatClient
{
    public interface IClient : IDisposable
    {
        void Connect();
        void SendMessage(string messageText);
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }
}