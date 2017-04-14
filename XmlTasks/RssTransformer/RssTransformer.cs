using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace RssTransformer
{
    public static class RssTransformer
    {
        public static string Transform(string path)
        {
            var transform = new XslCompiledTransform();
            transform.Load("rss.xslt");

            var builder = new StringBuilder();
            using (var writer = XmlWriter.Create(builder))
            {
                transform.Transform(path, writer);
            }

            return builder.ToString();
        }
    }
}
