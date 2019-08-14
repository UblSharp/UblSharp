using System;
using System.IO;
using System.Xml.Schema;
using UblSharp.Generator;

namespace UblSharp.SEeF.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            var generator = new UblGenerator();

            generator.Generate(
                new UblGeneratorOptions()
                {
                    XsdBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"V3\"),
                    OutputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\UblSharp.SEeF"),
                    Namespace = "UblSharp.SEeF",
                    ValidationHandler = ValidationHandler,
                    GenerateCommonFiles = false,
                    XmlToCsNamespaceMapping =
                    {
                        { "urn:www.energie-efactuur.nl:profile:invoice:ver3.0", "SEeF" }
                    }
                });

            Console.WriteLine("Done.");
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
