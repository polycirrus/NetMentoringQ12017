using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using System.Runtime.Serialization;

namespace SynchronousServer
{
    public class NamedPipeServerConnector : IServerConnector
    {
        private static readonly int ConnectionCount = 2;

        private CancellationTokenSource tokenSource;
        private Dictionary<NamedPipeServerStream, Task> connectionTasks;
        private Dictionary<NamedPipeServerStream, string> userIdMappings; 

        public string PipeName { get; set; }

        public event EventHandler<ConnectionEventArgs> Connected;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<ConnectionEventArgs> Disconnected;

        public NamedPipeServerConnector(string pipeName)
        {
            PipeName = pipeName;
        }

        public void Init()
        {
            userIdMappings = new Dictionary<NamedPipeServerStream, string>();
            connectionTasks = new Dictionary<NamedPipeServerStream, Task>();

            for (int i = 0; i < ConnectionCount; i++)
            {
                var connection = new NamedPipeServerStream(PipeName, PipeDirection.InOut, ConnectionCount, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough);

                tokenSource = new CancellationTokenSource();
                var task = Task.Run(() => PipeServerThread(connection), tokenSource.Token);
                connectionTasks.Add(connection, task);
            }
        }

        public void Stop()
        {
            var notificationTasks = connectionTasks.Keys.Select(
                    connection => Send(connection, new Notification() {Text = "The server has shut down."}));
            Task.WaitAll(notificationTasks.ToArray());

            tokenSource.Cancel();
        }

        public void Broadcast(Message message)
        {
            Task.WaitAll(connectionTasks.Keys.Select(connection => Send(connection, message)).ToArray());
        }

        public void Send(string userId, Message message)
        {
            var connections = userIdMappings.Where(kvp => kvp.Value == userId).Select(kvp => kvp.Key);
            foreach (var connection in connections)
                Send(connection, message);
        }

        public void Send(string userId, IEnumerable<Message> messages)
        {
            foreach (var message in messages)
                Send(userId, message);
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

        private async Task PipeServerThread(NamedPipeServerStream pipeServer)
        {
            try
            {
                while (!tokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (!pipeServer.IsConnected)
                            pipeServer.WaitForConnection();

                        while (pipeServer.IsConnected)
                        {
                            var data = new byte[1024];
                            var readBytes = await pipeServer.ReadAsync(data, 0, data.Length, tokenSource.Token);

                            var stream = new MemoryStream(data.Take(readBytes).ToArray());
                            var receivedObject = new BinaryFormatter().Deserialize(stream);
                            HandleReceivedObject(pipeServer, receivedObject);
                        }

                        OnDisconnect(pipeServer);
                    }
                    catch (IOException)
                    {
                        if (pipeServer.IsConnected)
                            pipeServer.Disconnect();

                        OnDisconnect(pipeServer);
                    }
                    catch (SerializationException)
                    {
                        if (pipeServer.IsConnected)
                            pipeServer.Disconnect();

                        OnDisconnect(pipeServer);
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
            tokenSource.Cancel();
        }
    }
}