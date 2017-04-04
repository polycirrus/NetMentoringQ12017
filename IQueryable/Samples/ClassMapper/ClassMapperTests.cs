using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassMapper
{
    public class Foo { }
    public class Bar { }

    [TestClass]
    public class ClassMapperTests
    {
        [TestMethod]
        public void TestMethod3()
        {
            var mapGenerator = new MappingGenerator();
            var mapper = mapGenerator.Generate<Foo, Bar>();

            var res = mapper.Map(new Foo());
        }
    }
}
