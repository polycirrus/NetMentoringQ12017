using System;

namespace Infrastructure
{
    public class ConnectionEventArgs : EventArgs
    {
        public string UserId { get; set; }
    }
}