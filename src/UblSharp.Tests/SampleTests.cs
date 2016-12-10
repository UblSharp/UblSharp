using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using FluentAssertions;
using UblSharp.Tests.Util;
using UblSharp.Validation;
using Xunit.Abstractions;

namespace UblSharp.Tests
{
    public partial class SampleTests
    {
        private static UblDocumentValidator _validater = new UblDocumentValidator();
        private readonly ITestOutputHelper _output;

        public static readonly Dictionary<string, string> SkippedTests = new Dictionary<string, string>
        {
            { "UBL-Invoice-2.0-Enveloped.xml", "xml serializer removed some unnecessary namespaces" },
            { "UBL-ForecastRevision-2.1-Example.xml", "The element 'ForecastRevisionLine' has invalid child element 'SourceForecastIssueDate'. List of possible elements expected: 'ID'." },
            { "UBL-OrderResponse-2.1-Example.xml", "The element 'LineItem' has incomplete content." }
        };

        public SampleTests(ITestOutputHelper output)
        {
            _output = output;
        }

        protected bool TestDocument<T>(string documentFilename, Func<T> factory)
            where T : BaseDocument
        {
            // serialize
            var doc = factory();
            string rawSample;
            using (var res = ResourceHelper.GetResource("Samples." + documentFilename))
            using (var rdr = new StreamReader(res))
            {
                rawSample = rdr.ReadToEnd();
            }

            var sampledocument = XDocument.Parse(rawSample, LoadOptions.SetBaseUri);

            var document = ToXDocument(doc);

            var areEqual = UblXmlComparer.IsCopyEqual(sampledocument, document, _output);
            if (!areEqual)
            {
                var sb = new StringBuilder();
                using (var stream = new StringWriter(sb))
                    doc.GetSerializer().Serialize(stream, doc);
                var rawDocumentText = sb.ToString();
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.ChangeExtension(documentFilename, ".orig.xml")), rawSample);
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.ChangeExtension(documentFilename, ".sample.xml")), sampledocument.ToString());
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.ChangeExtension(documentFilename, ".test.xml")), rawDocumentText);
            }

            areEqual.Should().BeTrue();

            // deserialize
            var sampleDoc = UblDocument.FromXDocument<T>(sampledocument);

            //var p1 = (sampleDoc as TransportationStatusType)?.__UBLVersionID;
            //var p2 = (doc as TransportationStatusType)?.__UBLVersionID;

            // TODO compare doc models
            doc.ShouldBeEquivalentTo(sampleDoc, options => options
                    .ExcludingProperties()
                    .ExcludingFields()
                    .Including(x => x.SelectedMemberInfo.Name.StartsWith("__", StringComparison.Ordinal))
            );

            var errors = _validater.Validate(doc).ToList();
            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }

            _validater.IsValid(doc).Should().BeTrue();
            errors.Should().NotContain(x => x.Severity == XmlSeverityType.Error);

            return areEqual;
        }

        private static XDocument ToXDocument(BaseDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            using (var ms = new MemoryStream())
            {
                document.Serialize(ms);

                ms.Position = 0;
                return XDocument.Load(ms);
            }
        }

        private static XDocument ToFormattedXDocument(BaseDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            using (var ms = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(ms, UblDocumentManager.Default.XmlWriterSettings))
                {
                    document.Serialize(writer);
                }

                ms.Position = 0;
                return XDocument.Load(ms);
            }
        }
    }
}