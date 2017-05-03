using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Message = Infrastructure.Message;

namespace SynchronousServer
{
    public partial class MainForm : Form
    {
        private ChatServer server;

        public MainForm()
        {
            InitializeComponent();
            server = new ChatServer(WriteToLog);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            server.Start();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            server.Stop();
        }

        private void WriteToLog(string message)
        {
            Invoke(new Action(() => logTextBox.Text += message + Environment.NewLine));
        }
    }

    //public partial class MainForm : Form
    //{
    //    private static readonly int pipeServersCount = 2;

    //    private Thread[] pipeServerThreads;
    //    private ConcurrentBag<NamedPipeServerStream> connections;

    //    public MainForm()
    //    {
    //        InitializeComponent();
    //    }

    //    private void startButton_Click(object sender, EventArgs e)
    //    {
    //        connections = new ConcurrentBag<NamedPipeServerStream>();

    //        pipeServerThreads = new Thread[pipeServersCount];
    //        for (int i = 0; i < pipeServersCount; i++)
    //        {
    //            pipeServerThreads[i] = new Thread(PipeServerThread);
    //            pipeServerThreads[i].Start();
    //        }
    //    }

    //    private void stopButton_Click(object sender, EventArgs e)
    //    {
    //        foreach (var pipeServerThread in pipeServerThreads)
    //            if (pipeServerThread.IsAlive)
    //                pipeServerThread.Abort();
    //    }

    //    private void PipeServerThread()
    //    {
    //        var pipeServer = new NamedPipeServerStream("serverPipe", PipeDirection.InOut, pipeServersCount);
    //        connections.Add(pipeServer);

    //        try
    //        {
    //            while (true)
    //            {
    //                try
    //                {
    //                    pipeServer.WaitForConnection();

    //                    while (pipeServer.IsConnected)
    //                    {
    //                        var receivedObject = new BinaryFormatter().Deserialize(pipeServer);
    //                        HandleReceivedObject(receivedObject);
    //                    }
    //                }
    //                catch (IOException)
    //                {
    //                    continue;
    //                }
    //            }
    //        }
    //        finally
    //        {
    //            if (pipeServer.IsConnected)
    //                pipeServer.Disconnect();

    //            pipeServer.Dispose();
    //        }
    //    }

    //    private void HandleReceivedObject(object receivedObject)
    //    {
    //        var message = receivedObject as Message;
    //        if (message != null)
    //            HandleMessage(message);
    //    }

    //    private void HandleMessage(Message message)
    //    {
    //        WriteToLog($"{message.UserId} says: {message.Text}");

    //        foreach (var connection in connections.ToList())
    //        {
    //            if (connection.IsConnected)
    //            {
    //                var stream = new MemoryStream();
    //                new BinaryFormatter().Serialize(stream, message);
    //                var data = stream.ToArray();

    //                Task.Run(() =>
    //                {
    //                    connection.Wr
    //                });
    //            }
    //        }
    //    }

    //    private void WriteToLog(string message)
    //    {
    //        Invoke(new Action(() => logTextBox.Text += message + Environment.NewLine));
    //    }
    //}
}
