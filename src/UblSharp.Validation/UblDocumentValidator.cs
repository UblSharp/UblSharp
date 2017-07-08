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
using UblSharp.Validation.Internal;

namespace UblSharp.Validation
{
    public class UblDocumentValidator
    {
        private const string BaseNamespace = "UblSharp.Validation.Resources";
        private readonly XmlSchemaSet _cachedSchema;
        private readonly Assembly _executingAssembly = Assembly.GetExecutingAssembly();
        private static readonly ValidationEventHandler _schemaValHandler = (s, e) => { throw new Exception(e.Message); };
        private readonly UblXsdResolver _xsdResolver = new UblXsdResolver();
        private static readonly XmlWriterSettings _xmlWriterSettings = new XmlWriterSettings()
        {
            CloseOutput = false,
            Indent = false,
            CheckCharacters = false,
            NewLineHandling = NewLineHandling.None,            
#if !(NET20 || NET35)
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
#endif
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };

        public UblDocumentValidator()
        {
            _cachedSchema = new XmlSchemaSet()
            {
                XmlResolver = _xsdResolver
            };

            var xsdName = $"{BaseNamespace}.maindoc.UBL-";
            foreach (var manifestStreamName in _executingAssembly.GetManifestResourceNames())
            {
                if (manifestStreamName.StartsWith(xsdName, StringComparison.Ordinal)
                    && manifestStreamName.EndsWith(".xsd", StringComparison.Ordinal))
                {
                    using (var manifestStream = _executingAssembly.GetManifestResourceStream(manifestStreamName))
                    {
                        if (manifestStream == null)
                        {
                            throw new InvalidOperationException($"Error while getting manifest resource '{manifestStreamName}'");
                        }

                        _cachedSchema.Add(XmlSchema.Read(manifestStream, _schemaValHandler));
                    }
                }
            }

            var resourceName = $"{BaseNamespace}.common.UBL-xmldsig-core-schema-2.1.xsd";
            using (var stream = _executingAssembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException($"Error while getting manifest resource '{resourceName}'");
                }

                _cachedSchema.Add(XmlSchema.Read(stream, _schemaValHandler));
            }

            _cachedSchema.Compile();
        }

        public static UblDocumentValidator Default { get; set; } = new UblDocumentValidator();

        public bool IsValid<T>(T document, bool warningsAsErrors = false)
            where T : IBaseDocument
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

        public IEnumerable<UblDocumentValidationError> Validate<T>(T document, bool suppressWarnings = false)
            where T : IBaseDocument
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var serializer = document.GetSerializer();

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
                    // Read all XML content
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
            using (var writer = XmlWriter.Create(memStream, _xmlWriterSettings))
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
