using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NamedPipeWrapper;

namespace TestServer
{
    public partial class Form1 : Form
    {
        private NamedPipeServer<string> server; 

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            server = new NamedPipeServer<string>("serverPipe");
            server.ClientConnected += connection => Log($"{connection.Id} {connection.Name} connected}}");
            server.ClientDisconnected += connection => Log($"{connection.Id} {connection.Name} disconnected}}");
            server.ClientMessage += (connection, message) =>
            {
                Log($"{connection.Id} {connection.Name} says: {message}");
                server.PushMessage(message);
            };
            server.Start();
        }

        private void Log(string text)
        {
            Invoke(new Action(() => textBox1.Text += text + Environment.NewLine));
        }
    }
}
