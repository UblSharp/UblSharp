using System.Collections.Generic;
using System.Xml.Schema;

namespace UblSharp.Generator
{
    public class UblGeneratorOptions
    {
        public string XsdBasePath { get; set; }

        public string UblSharpNamespace { get; set; } = "UblSharp";

        public string Namespace { get; set; } = "UblSharp";

        public string OutputPath { get; set; }

        public ValidationEventHandler ValidationHandler { get; set; }

        public Dictionary<string, string> XmlToCsNamespaceMapping { get; } = new Dictionary<string, string>();

        public bool GenerateCommonFiles { get; set; }

        public void Validate()
        {
            // TODO: validate properties
        }
    }
}
