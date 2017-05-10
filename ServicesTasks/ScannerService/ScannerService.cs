using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace ScannerService
{
    class ScannerService : ServiceBase
    {
        protected override void OnStart(string[] args)
        {
            CombineExistingFiles();
        }

        private void CombineExistingFiles()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var files = Directory.GetFiles(path);

            using (var doc = new XpsDocument("abc.xps", FileAccess.ReadWrite))
            {
                var writer = XpsDocument.CreateXpsDocumentWriter(doc);

                var collator = writer.CreateVisualsCollator();
                collator.BeginBatchWrite();
                foreach (var file in files)
                {
                    var bitmap = new BitmapImage(new Uri(file));

                    var image = new Image();
                    image.Source = bitmap;

                    collator.Write(image);
                }
                collator.EndBatchWrite();
            }
        }
    }
}
