using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace SchemaValidator
{
    public class Validator
    {
        private static XmlReaderSettings CreateSettings(StringBuilder errors)
        {
            var settings = new XmlReaderSettings();
            settings.Schemas.Add("http://library.by/catalog", "books.xsd");

            settings.ValidationEventHandler +=
                delegate (object sender, ValidationEventArgs e)
                {
                    errors.AppendFormat("[{0}:{1}] {2}", e.Exception.LineNumber, e.Exception.LinePosition, e.Message);
                };

            settings.ValidationFlags = settings.ValidationFlags | XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationType = ValidationType.Schema;

            return settings;
        }

        public static string Validate(string path)
        {
            var errors = new StringBuilder();

            using (var reader = XmlReader.Create(path, CreateSettings(errors)))
            {
                while (reader.Read())
                {
                }
            }

            return errors.Length > 0 ? errors.ToString() : null;
        }
    }
}
