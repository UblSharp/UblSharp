using System.Collections.Generic;
using System.Xml.Schema;

namespace UblSharp.Generator
{
    /// <summary>
    /// Used to specify options to the UBL class generator
    /// </summary>
    public class UblGeneratorOptions
    {
        /// <summary>
        /// Full path to a directory where the generator will look for xsd files to generate classes from.
        /// </summary>
        public string XsdBasePath { get; set; }

        /// <summary>
        /// Namespace of UblSharp. You don't need to override this when using the 'UblSharp' library
        /// </summary>
        public string UblSharpNamespace { get; set; } = "UblSharp";

        /// <summary>
        /// Namespace the generator should use when generating classes
        /// </summary>
        public string Namespace { get; set; } = "UblSharp";

        /// <summary>
        /// Full path of the output directory (where .cs files are written).
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// Use this handler if you want custom logging for validation errors. By default, it's logging using LibLog.
        /// </summary>
        public ValidationEventHandler ValidationHandler { get; set; }

        /// <summary>
        /// Xml to CSharp namespace mappings. Use this if you want types from an xml namespaces to be in a custom target c# namespace.
        /// </summary>
        public Dictionary<string, string> XmlToCsNamespaceMapping { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Specify true, to generate .cs files for common UBL types like CaC. CsC, CCT. You don't need this if using the 'UblSharp' library
        /// </summary>
        public bool GenerateCommonFiles { get; set; }

        /// <summary>
        /// Currently unused
        /// </summary>
        internal void Validate()
        {
            // TODO: validate properties
        }
    }
}
