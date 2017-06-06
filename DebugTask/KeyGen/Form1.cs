using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyGen
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            var networkInterface = NetworkInterface.GetAllNetworkInterfaces().First();
            var addressBytes = networkInterface.GetPhysicalAddress().GetAddressBytes();

            var dateBytes = BitConverter.GetBytes(DateTime.Now.Date.ToBinary());

            var keyNumbers = addressBytes
                .Select((@byte, index) => @byte ^ dateBytes[index])
                .Select(number => number <= 999 ? number * 10 : number);

            var key = string.Join("-", keyNumbers.Select(number => number.ToString()));
            keyTextBox.Text = key;
        }
    }
}
