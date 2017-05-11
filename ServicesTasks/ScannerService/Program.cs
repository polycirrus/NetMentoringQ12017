using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;

namespace ScannerService
{
    class Program
    {
        [STAThread()]
        static void Main(string[] args)
        {
            ServiceBase.Run(new ScannerService());
        }
    }
}
