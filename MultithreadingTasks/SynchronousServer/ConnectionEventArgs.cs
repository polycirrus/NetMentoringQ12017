using System;

namespace SynchronousServer
{
    public class ConnectionEventArgs : EventArgs
    {
        public string UserId { get; set; }
    }
}