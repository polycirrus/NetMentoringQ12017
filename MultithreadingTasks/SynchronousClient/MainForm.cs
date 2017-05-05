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
        private IClientConnector client;

        public MainForm()
        {
            InitializeComponent();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            client?.Send(messageTextBox.Text);
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (namedPipeRadioButton.Checked)
                client = new NamedPipeClientConnector(LogMessage);
            if (socketRadioButton.Checked)
                client = new SocketClientConnector(LogMessage);

            client.Connected += (o, args) => LogMessage($"Connected as {args.UserId}");
            client.Disconnected += (o, args) => LogMessage($"Disconnected as {args.UserId}");
            client.MessageReceived += (o, args) => LogMessage($"{args.Message.UserId} says: {args.Message.Text}");

            client.Connect(userIdTextBox.Text);
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            client?.Disconnect();
        }

        private void LogMessage(string message)
        {
            Invoke(new Action(() => logTextBox.Text += message + Environment.NewLine));
        }
    }
}
