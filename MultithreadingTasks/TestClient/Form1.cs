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

namespace TestClient
{
    public partial class Form1 : Form
    {
        private NamedPipeClient<string> client; 

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            client = new NamedPipeClient<string>("serverPipe");
            client.ServerMessage +=
                (connection, message) =>
                    MessageBox.Show(message, "Server message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            client.Start();

            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            client.PushMessage(textBox1.Text);
        }
    }
}
