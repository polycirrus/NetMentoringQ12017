using System;

namespace Infrastructure
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }
}