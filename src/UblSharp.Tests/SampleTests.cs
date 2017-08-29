using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using FluentAssertions;
using FluentAssertions.Equivalency;
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
            //{ "UBL-Invoice-2.0-Enveloped.xml", "xml serializer removed some unnecessary namespaces" },
            //{ "UBL-ForecastRevision-2.1-Example.xml", "The element 'ForecastRevisionLine' has invalid child element 'SourceForecastIssueDate'. List of possible elements expected: 'ID'." },
            //{ "UBL-OrderResponse-2.1-Example.xml", "The element 'LineItem' has incomplete content." }
        };

        public SampleTests(ITestOutputHelper output)
        {
            _output = output;
        }

        protected bool TestDocument<T>(string documentFilename, Func<T> factory)
            where T : BaseDocument
        {
            // serialize
            var subject = factory();
            string rawSample;
            using (var res = ResourceHelper.GetResource("Samples." + documentFilename))
            using (var rdr = new StreamReader(res))
            {
                rawSample = rdr.ReadToEnd();
            }

            bool areEqual = true;

            XDocument sampleDocument;
            using (var strRdr = new StringReader(rawSample))
            using (var xmlRdr = XmlReader.Create(
                strRdr, new XmlReaderSettings()
                {
                    CheckCharacters = false
                }))
            {
                sampleDocument = XDocument.Load(xmlRdr, LoadOptions.SetBaseUri);
            }

            var subjectXDoc = subject.ToXDocument();
            areEqual = UblXmlComparer.IsCopyEqual(sampleDocument, subjectXDoc, _output);

#if DEBUG
            if (!areEqual)
            {
                var sb = new StringBuilder();
                using (var stream = new StringWriter(sb))
                    subject.Save(stream);
                var rawDocumentText = sb.ToString();
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, Path.ChangeExtension(documentFilename, ".orig.xml")), rawSample);
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, Path.ChangeExtension(documentFilename, ".sample.xml")), sampleDocument.ToString());
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, Path.ChangeExtension(documentFilename, ".test.xml")), rawDocumentText);
            }
#endif

            areEqual.Should().BeTrue("XML should be equal");

            var sampleDoc = UblDocument.Parse<T>(rawSample);
            var trace = new StringBuilderTraceWriter();
            try
            {
                subject.ShouldBeEquivalentTo(
                    sampleDoc, options => options
                        .ExcludingProperties()
                        .ExcludingFields()
                        .Including(x => x.SelectedMemberInfo.Name.StartsWith("__", StringComparison.Ordinal))
                        .WithTracing(trace));
            }
            catch
            {
#if DEBUG
                var sb = new StringBuilder();
                using (var stream = new StringWriter(sb))
                    subject.Save(stream);
                var rawDocumentText = sb.ToString();
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, Path.ChangeExtension(documentFilename, ".orig.xml")), rawSample);
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, Path.ChangeExtension(documentFilename, ".test.xml")), rawDocumentText);
#endif
                throw;
            }
            finally
            {
                _output.WriteLine(trace.ToString());
            }

#if FEATURE_VALIDATION
            var errors = _validater.Validate(subject).ToList();
            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }

            errors.Should().NotContain(x => x.Severity == XmlSeverityType.Error);
#endif

            return areEqual;
        }

#if !NETCORE
        public static class AppContext
        {
            public static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
        }
#endif
    }
}
