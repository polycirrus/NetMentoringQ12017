using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SchemaValidator;

namespace Tests
{
    [TestClass]
    public class SchemaValidatorTests
    {
        [TestMethod]
        public void Validate_ValidXml()
        {
            var path = Path.GetFullPath("books.xml");

            var errors = Validator.Validate(path);
            Console.WriteLine(errors);

            Assert.IsNull(errors);
        }

        [TestMethod]
        public void Validate_InvalidIsbn()
        {
            var path = Path.GetFullPath("books_invalid_isbn.xml");

            var errors = Validator.Validate(path);
            Console.WriteLine(errors);

            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Contains("isbn"));
        }

        [TestMethod]
        public void Validate_InvalidGenre()
        {
            var path = Path.GetFullPath("books_invalid_genre.xml");

            var errors = Validator.Validate(path);
            Console.WriteLine(errors);

            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Contains("genre"));
        }

        [TestMethod]
        public void Validate_InvalidDates()
        {
            var path = Path.GetFullPath("books_invalid_dates.xml");

            var errors = Validator.Validate(path);
            Console.WriteLine(errors);

            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Contains("publish_date"));
            Assert.IsTrue(errors.Contains("registration_date"));
        }

        [TestMethod]
        public void Validate_DuplicateIDs()
        {
            var path = Path.GetFullPath("books_duplicate_ids.xml");

            var errors = Validator.Validate(path);
            Console.WriteLine(errors);

            Assert.IsNotNull(errors);
            Console.WriteLine(errors);
            //Assert.IsTrue(errors.Contains("bookId"));
        }
    }
}
