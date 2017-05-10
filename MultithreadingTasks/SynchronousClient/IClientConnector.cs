using System;
using Infrastructure;

namespace SynchronousClient
{
    public interface IClientConnector : IConnector
    {
        string UserId { get; }

        void Connect(string userId);
        void Disconnect();
        void Send(string message);
    }
}