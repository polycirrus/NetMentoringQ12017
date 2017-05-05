using System;

namespace Infrastructure
{
    public interface IConnector : IDisposable
    {
        event EventHandler<ConnectionEventArgs> Connected;
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        event EventHandler<ConnectionEventArgs> Disconnected;
    }
}