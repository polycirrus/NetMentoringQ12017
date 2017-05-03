using System;
using Infrastructure;

namespace SynchronousServer
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }
}