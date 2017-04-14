using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Resolvers;
using System.Xml.Xsl;

namespace XsltTransformer
{
    public static class XsltTransformer
    {
        public static void Transform(string inputPath, string stylesheetPath, string outputPath)
        {
            var transform = new XslCompiledTransform();
            transform.Load(stylesheetPath, XsltSettings.TrustedXslt, null);
            transform.Transform(inputPath, outputPath);
        }
    }
}
