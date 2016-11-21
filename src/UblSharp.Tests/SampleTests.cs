using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using FluentAssertions;
using Microsoft.XmlDiffPatch;
using UblSharp.Tests.Samples;
using UblSharp.Tests.Util;
using UblSharp.UnqualifiedDataTypes;
using Xunit;
using Xunit.Abstractions;

namespace UblSharp.Tests
{
    public partial class SampleTests
    {
        private readonly ITestOutputHelper _output;

        public static readonly Dictionary<string, string> SkippedTests = new Dictionary<string, string>
        {
            { "UBL-Invoice-2.0-Enveloped.xml", "xml serializer removed some unnecessary namespaces" }
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

    public static class UblXmlComparer
    {
        private static readonly XNamespace cbcNamespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        private static readonly XName schemaLocationAttrName = XName.Get("schemaLocation", XmlSchema.InstanceNamespace);
        private static readonly XName extensionsElementName = XName.Get("Extensions", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
        private static XmlReaderSettings closeInputSettings = new XmlReaderSettings { CloseInput = true };

        /// <summary>
        /// Compare a ubl document class instance with a xml file on disk.
        /// </summary>
        public static bool IsCopyEqual(XDocument sampleDocument, XDocument ublDocument, ITestOutputHelper output)
        {
            bool areEqual = false;

            using (var fileReader = CreateReaderForSampleDocument2(sampleDocument))
            using (var docReader = CreateReaderForDoc2(ublDocument))
            {
                var options = XmlDiffOptions.IgnoreComments | XmlDiffOptions.IgnoreWhitespace | XmlDiffOptions.IgnoreNamespaces | XmlDiffOptions.IgnorePI | XmlDiffOptions.IgnoreXmlDecl;
                var xmlDiff = new XmlDiff(options);

                using (var diffgramStream = new MemoryStream())
                {
                    using (var diffgramWriter = new XmlTextWriter(new StreamWriter(diffgramStream)))
                    {

                        areEqual = xmlDiff.Compare(fileReader, docReader, diffgramWriter);
                        if (!areEqual)
                        {
                            diffgramStream.Position = 0;
                            using (var tr = new XmlTextReader(diffgramStream))
                            {
                                var xdoc = XDocument.Load(tr);
                                var diff = xdoc.ToString();
                                output.WriteLine(diff);
                            }
                        }
                    }
                }
            }

            return areEqual;
        }

        private static Stream CreateReaderForDoc(XDocument document)
        {
            var stream = new MemoryStream();
            document.Save(stream);
            stream.Position = 0;
            return stream;
        }

        private static XmlReader CreateReaderForDoc2(XDocument document)
        {
            return document.CreateReader();
        }

        private static Stream CreateReaderForSampleDocument(XDocument document)
        {
            RemoveSchemaLocationAndFormatTime(document);

            var moddedOrgMs = new MemoryStream();
            using (var moddedOrgXw = XmlWriter.Create(moddedOrgMs))
            {
                document.WriteTo(moddedOrgXw);
            }
            moddedOrgMs.Position = 0;
            return moddedOrgMs;
            // return XmlReader.Create(moddedOrgMs, closeInputSettings);
        }

        private static XmlReader CreateReaderForSampleDocument2(XDocument document)
        {
            RemoveSchemaLocationAndFormatTime(document);

            var moddedOrgMs = new MemoryStream();
            using (var moddedOrgXw = XmlWriter.Create(moddedOrgMs))
            {
                document.WriteTo(moddedOrgXw);
            }
            moddedOrgMs.Position = 0;
            return XmlReader.Create(moddedOrgMs, closeInputSettings);
        }

        private static void RemoveSchemaLocationAndFormatTime(XDocument xDoc)
        {
            var schemaLocationAttr = xDoc.Root.Attribute(schemaLocationAttrName);
            if (schemaLocationAttr != null)
            {
                schemaLocationAttr.Remove();
            }

            //var id = xDoc.Root.Elements().Where(x => x.Name.LocalName == "ID").Select((element, index) => new
            //{
            //    element,
            //    index
            //}).FirstOrDefault();
            //var uuid = xDoc.Root.Elements().Where(x => x.Name.LocalName == "UUID").Select((element, index) => new
            //{
            //    element,
            //    index
            //}).FirstOrDefault();
            //if (id != null && uuid != null)
            //{
            //    uuid.element.Remove();
            //    id.element.AddAfterSelf(uuid.element);
            //}

            // Format the time string in the inputfile to make XmlComparer happy
            foreach (var node in xDoc.Root.Descendants().Where(n => n.Name.Namespace == cbcNamespace && n.Name.LocalName.EndsWith("Time")))
            {
                TimeType ublTimeType = node.Value;
                node.Value = XmlConvert.ToString(ublTimeType.Value).Split('T').Last();
                // node.Value = XmlConvert.ToString(ublTimeType.Value, XmlDateTimeSerializationMode.RoundtripKind).Split('T').Last();
                // node.Value = XmlConvert.ToString(DateTime.MinValue + ublTimeType.Value.TimeOfDay, "HH:mm:ss.fffffffzzzzzz");
            }

            // remove empty elements.
            // http://docs.oasis-open.org/ubl/os-UBL-2.1/UBL-2.1.html#S-EMPTY-ELEMENTS
            foreach (var node in xDoc.Root.Elements().Where(e => e.Name != extensionsElementName).Descendants().Where(n => n.IsEmpty).ToList())
            {
                node.Remove();
            }
        }
    }
}