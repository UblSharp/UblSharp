using System.Xml.Schema;

namespace UblSharp.Generator
{
    public class UblGeneratorOptions
    {
        public string XsdBasePath { get; set; }

        public string Namespace { get; set; }

        public string OutputPath { get; set; }

        public ValidationEventHandler ValidationHandler { get; set; }

        public void Validate()
        {
            // TODO: validate properties
        }
    }
}