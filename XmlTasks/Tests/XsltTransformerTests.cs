using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class XsltTransformerTests
    {
        [TestMethod]
        public void Transform_BooksToHtmlReport()
        {
            var input = Path.GetFullPath("books.xml");
            var xslt = Path.GetFullPath("report.xslt");
            var output = Path.GetFullPath("output.html");

            XsltTransformer.XsltTransformer.Transform(input, xslt, output);

            Console.WriteLine(File.ReadAllText(output));
        }
    }
}
