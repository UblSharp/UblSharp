using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using UblSharp.Validation.Internal;

namespace UblSharp.Validation
{
    public class UblDocumentValidator
    {
        private const string BaseNamespace = "UblSharp.Validation.Resources";
        private readonly XmlSchemaSet _cachedSchema;
        private readonly Assembly _executingAssembly = Assembly.GetExecutingAssembly();
        private readonly UblDocumentManager _documentManager;
        private static readonly ValidationEventHandler _schemaValHandler = (s, e) => { throw new Exception(e.Message); };

        public UblDocumentValidator()
            : this(UblDocumentManager.Default)
        {
        }

        public UblDocumentValidator(UblDocumentManager documentManager)
        {
            _documentManager = documentManager;
            _cachedSchema = new XmlSchemaSet()
            {
                XmlResolver = new XsdResolver()
            };

            _cachedSchema.Add(XmlSchema.Read(_executingAssembly.GetManifestResourceStream($"{BaseNamespace}.common.UBL-xmldsig-core-schema-2.1.xsd"), _schemaValHandler));
        }

        public static UblDocumentValidator Default { get; set; } = new UblDocumentValidator();

        public bool IsValid(BaseDocument document, bool warningsAsErrors = false)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var serializer = _documentManager.GetSerializer(document.GetType());
            var xmlDocument = new XDocument();
            using (var xmlWriter = xmlDocument.CreateWriter())
            {
                serializer.Serialize(xmlWriter, document);
            }

            return IsValid(xmlDocument, warningsAsErrors);
        }

        public bool IsValid(XDocument xmlDocument, bool warningsAsErrors = false)
        {
            if (xmlDocument == null) throw new ArgumentNullException(nameof(xmlDocument));

            return !Validate(xmlDocument).Any(x => x.Severity == XmlSeverityType.Error || (warningsAsErrors && x.Severity == XmlSeverityType.Warning));
        }

        public IEnumerable<UblDocumentValidationError> Validate(BaseDocument document, bool suppressWarnings = false)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var serializer = _documentManager.GetSerializer(document.GetType());
            var xmlDocument = new XDocument();
            using (var xmlWriter = xmlDocument.CreateWriter())
            {
                serializer.Serialize(xmlWriter, document);
            }

            return Validate(xmlDocument, suppressWarnings);
        }

        public IEnumerable<UblDocumentValidationError> Validate(XDocument xmlDocument, bool suppressWarnings = false)
        {
            if (xmlDocument == null) throw new ArgumentNullException(nameof(xmlDocument));

            var ns = xmlDocument.Root.GetDefaultNamespace().NamespaceName;
            var errors = new List<UblDocumentValidationError>();
            ValidationEventHandler valHandler = (s, e) =>
                {
                    if (suppressWarnings && e.Severity == XmlSeverityType.Warning)
                        return;

                    errors.Add(new UblDocumentValidationError(e.Exception, e.Message, e.Severity));
                };

            // Add schema to cached xmlschemaset from assembly resource
            if (!_cachedSchema.Contains(ns))
            {
                var docNsType = ns.Substring(ns.LastIndexOf(":", StringComparison.Ordinal) + 1);
                if (docNsType.EndsWith("-2"))
                {
                    docNsType = docNsType.Substring(0, docNsType.Length - 2);
                }

                var xsdName = $"{BaseNamespace}.maindoc.UBL-{docNsType}-";

                var manifestStreamName = _executingAssembly.GetManifestResourceNames().FirstOrDefault(x => x.StartsWith(xsdName) && x.EndsWith(".xsd"));
                if (string.IsNullOrEmpty(manifestStreamName))
                {
                    throw new Exception("Could not find xsd: " + xsdName);
                }

                using (var manifestStream = _executingAssembly.GetManifestResourceStream(manifestStreamName))
                {
                    if (manifestStream == null)
                    {
                        throw new Exception("Could not find xsd: " + xsdName);
                    }

                    _cachedSchema.Add(XmlSchema.Read(manifestStream, _schemaValHandler));
                }

                _cachedSchema.Compile();
            }

            xmlDocument.Validate(_cachedSchema, valHandler);
            return errors;
        }
    }
}