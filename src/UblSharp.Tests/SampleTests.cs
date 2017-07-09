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
#if FEATURE_VALIDATION
using UblSharp.Validation;
#endif
using Xunit.Abstractions;

namespace UblSharp.Tests
{
    public partial class SampleTests
    {
#if FEATURE_VALIDATION
        private static UblDocumentValidator _validater = new UblDocumentValidator();
#endif

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
            bool areEqual = true;

#if FEATURE_XMLDIFFPATCH
            areEqual = UblXmlComparer.IsCopyEqual(sampledocument, document, _output);

#if DEBUG
            if (!areEqual)
            {
                var sb = new StringBuilder();
                using (var stream = new StringWriter(sb))
                    doc.Save(stream);
                var rawDocumentText = sb.ToString();
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.ChangeExtension(documentFilename, ".orig.xml")), rawSample);
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.ChangeExtension(documentFilename, ".sample.xml")), sampledocument.ToString());
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.ChangeExtension(documentFilename, ".test.xml")), rawDocumentText);
            }
#endif

            areEqual.Should().BeTrue();
#endif

            // deserialize
            var sampleDoc = UblDocument.Load<T>(sampledocument);

            //var p1 = (sampleDoc as TransportationStatusType)?.__UBLVersionID;
            //var p2 = (doc as TransportationStatusType)?.__UBLVersionID;

            // TODO compare doc models
            doc.ShouldBeEquivalentTo(sampleDoc, options => options
                    .ExcludingProperties()
                    .ExcludingFields()
                    .Including(x => x.SelectedMemberInfo.Name.StartsWith("__", StringComparison.Ordinal))
            );

#if FEATURE_VALIDATION
            var errors = _validater.Validate(doc).ToList();
            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }

            _validater.IsValid(doc).Should().BeTrue();
            errors.Should().NotContain(x => x.Severity == XmlSeverityType.Error);
#endif

            return areEqual;
        }

        private static XDocument ToXDocument<T>(T document)
            where T : IBaseDocument
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            using (var ms = new MemoryStream())
            {
                document.Save(ms);

                ms.Position = 0;
                return XDocument.Load(ms);
            }
        }
    }
}
