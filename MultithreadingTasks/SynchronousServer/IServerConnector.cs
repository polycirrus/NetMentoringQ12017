using System;
using System.Collections.Generic;
using Infrastructure;

namespace SynchronousServer
{
    public interface IServerConnector : IConnector
    {
        void Init();
        void Stop();

        void Broadcast(Message message);
        void Send(string userId, Message message);
        void Send(string userId, IEnumerable<Message> messages);
    }
}