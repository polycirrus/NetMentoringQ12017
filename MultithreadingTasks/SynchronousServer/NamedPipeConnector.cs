using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;

namespace SynchronousServer
{
    public class NamedPipeConnector : IConnector
    {
        private static readonly int ConnectionCount = 2;

        private Dictionary<NamedPipeServerStream, Thread> connectionThreads;
        private Dictionary<NamedPipeServerStream, string> userIdMappings; 

        public string PipeName { get; set; }

        public event EventHandler<ConnectionEventArgs> Connected;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<ConnectionEventArgs> Disconnected;

        public NamedPipeConnector(string pipeName)
        {
            PipeName = pipeName;
        }

        public void Init()
        {
            userIdMappings = new Dictionary<NamedPipeServerStream, string>();
            connectionThreads = new Dictionary<NamedPipeServerStream, Thread>();

            for (int i = 0; i < ConnectionCount; i++)
            {
                var connection = new NamedPipeServerStream(PipeName, PipeDirection.InOut, ConnectionCount);
                var listenerThread = new Thread(PipeServerThread);
                listenerThread.Start(connection);
                connectionThreads.Add(connection, listenerThread);
            }
        }

        public void Stop()
        {
            foreach (var connection in connectionThreads.Keys)
                Send(connection, new Notification() {Text = "The server has shut down."});

            foreach (var thread in connectionThreads.Values)
                thread.Abort();
        }

        public void Broadcast(Message message)
        {
            Task.WaitAll(connectionThreads.Keys.Select(connection => Send(connection, message)).ToArray());
        }

        public void Send(string userId, Message message)
        {
            var connections = userIdMappings.Where(kvp => kvp.Value == userId).Select(kvp => kvp.Key);
            foreach (var connection in connections)
                Send(connection, message);
        }

        private Task Send(NamedPipeServerStream connection, object data)
        {
            return Task.Run(() =>
            {
                if (!connection.IsConnected)
                    return;

                var stream = new MemoryStream();
                new BinaryFormatter().Serialize(stream, data);
                var dataBytes = stream.ToArray();

                connection.Write(dataBytes, 0, dataBytes.Length);
                connection.WaitForPipeDrain();
            });
        }

        private void PipeServerThread(object state)
        {
            var pipeServer = state as NamedPipeServerStream;
            if (pipeServer == null)
                throw new ArgumentException();

            try
            {
                while (true)
                {
                    try
                    {
                        pipeServer.WaitForConnection();

                        while (pipeServer.IsConnected)
                        {
                            var receivedObject = new BinaryFormatter().Deserialize(pipeServer);
                            HandleReceivedObject(pipeServer, receivedObject);
                        }

                        OnDisconnect(pipeServer);
                    }
                    catch (IOException)
                    {
                        OnDisconnect(pipeServer);
                        continue;
                    }
                }
            }
            finally
            {
                if (pipeServer.IsConnected)
                    pipeServer.Disconnect();
                OnDisconnect(pipeServer);

                pipeServer.Dispose();
            }
        }

        private void OnDisconnect(NamedPipeServerStream pipeServer)
        {
            string userId;
            if (userIdMappings.TryGetValue(pipeServer, out userId))
                Disconnected?.Invoke(this, new ConnectionEventArgs() {UserId = userId});
        }

        private void HandleReceivedObject(NamedPipeServerStream connection, object receivedObject)
        {
            var message = receivedObject as Message;
            if (message != null)
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs() {Message = message});

            var authentication = receivedObject as Authentication;
            if (authentication != null)
            {
                userIdMappings[connection] = authentication.UserId;
                Connected?.Invoke(this, new ConnectionEventArgs() { UserId = authentication.UserId });
            }
        }

        public void Dispose()
        {
            foreach (var thread in connectionThreads.Values)
                thread.Abort();
        }
    }
}