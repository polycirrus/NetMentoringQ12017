using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassMapper
{
    [TestClass]
    public class ClassMapperTests
    {
        private class Source
        {
            public int MappedProperty1 { get; set; }
            public int MappedProperty2 { get; set; }
            public int MappedProperty3 { get; set; }

            public int UnmappedProperty1 { get; set; }
            public int UnmappedProperty2 { get; set; }
        }

        private class Destination
        {
            public int MappedProperty1 { get; set; }
            public int MappedProperty2 { get; set; }
            public int MappedProperty3 { get; set; }

            public int UnmappedProperty3 { get; set; }
            public int UnmappedProperty4 { get; set; }
        }

        [TestMethod]
        public void Map()
        {
            var source = new Source()
            {
                MappedProperty1 = 1,
                MappedProperty2 = 2,
                MappedProperty3 = 3,
                UnmappedProperty1 = 4,
                UnmappedProperty2 = 5
            };

            var mapper = MappingGenerator.Generate<Source, Destination>();
            var destination = mapper.Map(source);

            Assert.AreEqual(source.MappedProperty1, destination.MappedProperty1);
            Assert.AreEqual(source.MappedProperty2, destination.MappedProperty2);
            Assert.AreEqual(source.MappedProperty3, destination.MappedProperty3);
            Assert.AreEqual(default(int), destination.UnmappedProperty3);
            Assert.AreEqual(default(int), destination.UnmappedProperty4);
        }
    }
}
