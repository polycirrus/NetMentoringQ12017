using System;
using Infrastructure;

namespace SynchronousServer
{
    public interface IConnector : IDisposable
    {
        void Init();
        void Stop();

        void Broadcast(Message message);
        void Send(string userId, Message message);

        event EventHandler<ConnectionEventArgs> Connected; 
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        event EventHandler<ConnectionEventArgs> Disconnected;
    }
}