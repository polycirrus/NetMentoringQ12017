using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class RssTransformerTests
    {
        [TestMethod]
        public void Transform()
        {
            var inputPath = Path.GetFullPath("books.xml");
            var rss = RssTransformer.RssTransformer.Transform(inputPath);
            Console.WriteLine(rss);
        }
    }
}
