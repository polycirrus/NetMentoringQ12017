using System;
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

namespace SynchronousClient
{
    using Infrastructure;

    public partial class MainForm : Form
    {
        private NamedPipeClientStream pipeClient;
        private Task listenerTask;
        private CancellationTokenSource tokenSource;

        public MainForm()
        {
            InitializeComponent();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (pipeClient == null || !pipeClient.IsConnected)
            {
                MessageBox.Show("You must be connected to send a message", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, new Message() {UserId = userIdTextBox.Text, Text = messageTextBox.Text});
            var data = stream.ToArray();

            pipeClient.Write(data, 0, data.Length);
            pipeClient.WaitForPipeDrain();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            pipeClient = new NamedPipeClientStream(".", "serverPipe", PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            pipeClient.Connect();

            var stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, new Authentication() {UserId = userIdTextBox.Text});
            var data = stream.ToArray();

            pipeClient.Write(data, 0, data.Length);
            pipeClient.WaitForPipeDrain();

            tokenSource = new CancellationTokenSource();
            listenerTask = Task.Run(() => Listen(), tokenSource.Token);

            LogMessage($"Connected as {userIdTextBox.Text}");
        }

        private void Listen()
        {
            while (!tokenSource.Token.IsCancellationRequested)
            {
                while (pipeClient.IsConnected)
                {
                    var receivedObject = new BinaryFormatter().Deserialize(pipeClient);
                    HandleReceivedObject(receivedObject);
                }
            }
            tokenSource.Token.ThrowIfCancellationRequested();
        }

        private void HandleReceivedObject(object receivedObject)
        {
            var message = receivedObject as Message;
            if (message != null)
                LogMessage($"{message.UserId} says: {message.Text}");

            var notification = receivedObject as Notification;
            if (notification != null)
                LogMessage($"Server says: {notification.Text}");
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();
            pipeClient.Dispose();
        }

        private void LogMessage(string message)
        {
            Invoke(new Action(() => logTextBox.Text += message + Environment.NewLine));
        }
    }
}
