using System;
using Infrastructure;

namespace SynchronousServer
{
    public interface IServerConnector : IConnector
    {
        void Init();
        void Stop();

        void Broadcast(Message message);
        void Send(string userId, Message message);
    }
}