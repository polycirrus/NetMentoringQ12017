using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            CombineExistingFiles();
        }

        private static void CombineExistingFiles()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var files = Directory.GetFiles(path);

            if (File.Exists("abc.xps"))
                File.Delete("abc.xps");
            
            using (var doc = new XpsDocument("abc.xps", FileAccess.ReadWrite))
            {
                var writer = XpsDocument.CreateXpsDocumentWriter(doc);

                var collator = writer.CreateVisualsCollator();
                collator.BeginBatchWrite();
                foreach (var file in files)
                {
                    BitmapImage bitmap;
                    try
                    {
                        bitmap = new BitmapImage(new Uri(file));
                    }
                    catch (NotSupportedException)
                    {
                        continue;
                    }

                    var image = new Image();
                    image.Source = bitmap;
                    image.Arrange(new Rect(new Size(bitmap.Width, bitmap.Height)));

                    collator.Write(image);
                }
                collator.EndBatchWrite();
            }
        }
    }
}
