using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
#if FEATURE_LINQ
using System.Xml.Linq;
#endif
using System.Xml.Schema;
using System.Xml.Serialization;
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
        private readonly UblXsdResolver _xsdResolver = new UblXsdResolver();

        public UblDocumentValidator()
            : this(UblDocumentManager.Default)
        {
        }

        public UblDocumentValidator(UblDocumentManager documentManager)
        {
            _documentManager = documentManager;
            _cachedSchema = new XmlSchemaSet()
            {
                XmlResolver = _xsdResolver
            };

            var xsdName = $"{BaseNamespace}.maindoc.UBL-";
            foreach (var manifestStreamName in _executingAssembly.GetManifestResourceNames())
            {
                if (manifestStreamName.StartsWith(xsdName)
                    && manifestStreamName.EndsWith(".xsd"))
                {
                    using (var manifestStream = _executingAssembly.GetManifestResourceStream(manifestStreamName))
                    {
                        if (manifestStream == null)
                        {
                            throw new Exception("Could not find xsd: " + xsdName);
                        }

                        _cachedSchema.Add(XmlSchema.Read(manifestStream, _schemaValHandler));
                    }
                }
            }

            _cachedSchema.Add(XmlSchema.Read(_executingAssembly.GetManifestResourceStream($"{BaseNamespace}.common.UBL-xmldsig-core-schema-2.1.xsd"), _schemaValHandler));
            _cachedSchema.Compile();
        }

        public static UblDocumentValidator Default { get; set; } = new UblDocumentValidator();

        public bool IsValid(BaseDocument document, bool warningsAsErrors = false)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            foreach (var x in Validate(document))
            {
                if (x.Severity == XmlSeverityType.Error
                    || (warningsAsErrors && x.Severity == XmlSeverityType.Warning))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsValid(XmlDocument xmlDocument, bool warningsAsErrors = false)
        {
            if (xmlDocument == null) throw new ArgumentNullException(nameof(xmlDocument));

            foreach (var x in Validate(xmlDocument))
            {
                if (x.Severity == XmlSeverityType.Error
                    || (warningsAsErrors && x.Severity == XmlSeverityType.Warning))
                {
                    return false;
                }
            }

            return true;
        }

#if FEATURE_LINQ
        public bool IsValid(XDocument xmlDocument, bool warningsAsErrors = false)
        {
            if (xmlDocument == null) throw new ArgumentNullException(nameof(xmlDocument));

            foreach (var x in Validate(xmlDocument))
            {
                if (x.Severity == XmlSeverityType.Error
                    || (warningsAsErrors && x.Severity == XmlSeverityType.Warning))
                {
                    return false;
                }
            }

            return true;
        }
#endif

        public IEnumerable<UblDocumentValidationError> Validate(BaseDocument document, bool suppressWarnings = false)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var serializer = _documentManager.GetSerializer(document.GetType());

            using (var memStream = new MemoryStream())
            {
                serializer.Serialize(memStream, document);
                memStream.Seek(0, SeekOrigin.Begin);
                return Validate(memStream, suppressWarnings);
            }
        }

        private IEnumerable<UblDocumentValidationError> Validate(MemoryStream memStream, bool suppressWarnings)
        {
            var errors = new List<UblDocumentValidationError>();
            var settings = new XmlReaderSettings()
            {
                ValidationType = ValidationType.Schema,
                ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.ProcessSchemaLocation | XmlSchemaValidationFlags.ReportValidationWarnings,
                Schemas = _cachedSchema,
                XmlResolver = _xsdResolver
            };
            settings.ValidationEventHandler += (s, e) => ValidationHandler(errors, suppressWarnings, e);

            using (var xmlReader = XmlReader.Create(memStream, settings))
            {
                while (xmlReader.Read())
                {
                }
            }
            return errors;
        }

        public IEnumerable<UblDocumentValidationError> Validate(XmlDocument xmlDocument, bool suppressWarnings = false)
        {
            if (xmlDocument == null)
                throw new ArgumentNullException(nameof(xmlDocument));
            if (xmlDocument.DocumentElement == null)
                throw new InvalidOperationException("Document must have a root node.");

            using (var memStream = new MemoryStream())
            {
                xmlDocument.Save(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                return Validate(memStream, suppressWarnings);
            }
        }

#if FEATURE_LINQ
        public IEnumerable<UblDocumentValidationError> Validate(XDocument xmlDocument, bool suppressWarnings = false)
        {
            if (xmlDocument == null) throw new ArgumentNullException(nameof(xmlDocument));
            if (xmlDocument.Root == null) throw new InvalidOperationException("Document must have a root node.");

            using (var memStream = new MemoryStream())
            using (var writer = XmlWriter.Create(memStream, _documentManager.XmlWriterSettings))
            {
                xmlDocument.Save(writer);
                writer.Flush();
                memStream.Seek(0, SeekOrigin.Begin);
                return Validate(memStream, suppressWarnings);
            }
        }
#endif

        private void ValidationHandler(List<UblDocumentValidationError> errors, bool suppressWarnings, ValidationEventArgs e)
        {
            if (suppressWarnings && e.Severity == XmlSeverityType.Warning)
                return;

            errors.Add(new UblDocumentValidationError(e.Exception, e.Message, e.Severity));
        }
    }
}
