using System;
using System.IO;
using System.Xml.Schema;

namespace UblSharp.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            var generator = new UblGenerator();
            generator.Generate(
                new UblGeneratorOptions()
                {
                    XsdBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\UblSharp.Validation\Resources\maindoc"),
                    OutputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\UblSharp"),
                    Namespace = "UblSharp",
                    ValidationHandler = ValidationHandler,
                    GenerateCommonFiles = false
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
