using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace UblSharp.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            var generator = new UblGenerator();
            generator.Generate(new UblGeneratorOptions()
            {
                XsdBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\UblSharp.Validation\Resources"),
                OutputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\UblSharp"),
                Namespace = "UblSharp",
                ValidationHandler = ValidationHandler
            });

            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        private static void ValidationHandler(object sender, ValidationEventArgs e)
        {
            Console.WriteLine($"{e.Severity}: {e.Message}");
            if (e.Severity == XmlSeverityType.Error)
            {
                throw e.Exception;
            }
        }
    }
}
