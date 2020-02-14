using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions;
using UblSharp.CommonExtensionComponents;
using UblSharp.SEeF;
using UblSharp.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace UblSharp.Tests.SEeF
{
    public class InvoiceExtensionTests
    {
        private readonly ITestOutputHelper _output;

        public InvoiceExtensionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanDeserializeSEeF_V1()
        {
            var xmlSerializer = UblSharp.SEeF.XmlSerializerFactory.Default.GetSerializer(SEeFVersion.V1);

            var seefV2Doc = ResourceHelper.GetResource("SEeF.Samples.20160704_SEeF - Voorbeeldfactuur 001 - levering.xml");
            var invoice = UblDocument.Load<InvoiceType>(seefV2Doc);

            var extContent = invoice.UBLExtensions[0].ExtensionContent.OuterXml;
            using (var sr = new StringReader(extContent))
            {
                var seef = (UblSharp.SEeF.V1.SEEFExtensionWrapperType)xmlSerializer.Deserialize(sr);

                seef.UtilityConsumptionPoint.Should().NotBeEmpty();
                seef.UtilityConsumptionPoint[0].ID.Value.Should().Be("871687400001234567");
            }
        }

        [Fact]
        public void CanDeserializeSEeF_V2()
        {
            var xmlSerializer = UblSharp.SEeF.XmlSerializerFactory.Default.GetSerializer(SEeFVersion.V2);

            var seefV2Doc = ResourceHelper.GetResource("SEeF.Samples.20170713_SEeF - Voorbeeldfactuur 001 - levering.xml");
            var invoice = UblDocument.Load<InvoiceType>(seefV2Doc);

            var extContent = invoice.UBLExtensions[0].ExtensionContent.OuterXml;
            using (var sr = new StringReader(extContent))
            {
                var seef = (UblSharp.SEeF.V1.SEEFExtensionWrapperType)xmlSerializer.Deserialize(sr);

                seef.UtilityConsumptionPoint.Should().NotBeEmpty();
                seef.UtilityConsumptionPoint[0].ID.Value.Should().Be("871687400001234567");
            }
        }

        [Fact]
        public void CanDeserializeSEeF_V3()
        {
            var xmlSerializer = UblSharp.SEeF.XmlSerializerFactory.Default.GetSerializer(SEeFVersion.V3);

            var seefV3Doc = ResourceHelper.GetResource("SEeF.Samples.20190326_SEeF 3.0  - Voorbeeldfactuur 001 - levering.xml");
            var invoice = UblDocument.Load<InvoiceType>(seefV3Doc);

            var extContent = invoice.UBLExtensions[0].ExtensionContent.OuterXml;
            using (var sr = new StringReader(extContent))
            {
                var seef = (UblSharp.SEeF.V3.SEEFExtensionWrapperType)xmlSerializer.Deserialize(sr);

                seef.UtilityConsumptionPoint.Should().NotBeEmpty();
                seef.UtilityConsumptionPoint[0].ID.Value.Should().Be("871687400001234567");
            }
        }

        [Fact]
        public void CanSerializeSEeF_V1()
        {
            var xmlSerializer = UblSharp.SEeF.XmlSerializerFactory.Default.GetSerializer(SEeFVersion.V1);

            var seef = new UblSharp.SEeF.V1.SEEFExtensionWrapperType()
            {
                UtilityConsumptionPoint =
                {
                    new UblSharp.SEeF.V1.ConsumptionPointType()
                    {
                        ID = "ConsumptionPointType_ID"
                    }
                }
            };

            // Create a temporary XmlDocument
            var doc = new XmlDocument();

            // Create an XmlWriter which writes elements to this document
            using (var writer = doc.CreateNavigator().AppendChild())
            {
                // Serialize our custom type to this XmlWriter
                xmlSerializer.Serialize(writer, seef);
            }

            // Get the root XmlElement of our custom document and use it as our ExtensionContent
            var ext = new UBLExtensionType()
            {
                ExtensionContent = doc.DocumentElement
            };

            var invoice = new InvoiceType()
            {
                ID = "id",
                UBLExtensions = new List<UBLExtensionType>()
                {
                    ext
                }
            };

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                UblDocument.Save(invoice, sw);
            }

            var expected = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Invoice xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"" xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"" xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2"">
	<UBLExtensions xmlns=""urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"">
		<UBLExtension xmlns:sig=""urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2"" xmlns:sbc=""urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2"">
			<ExtensionContent>
				<SEEFExtensionWrapper xmlns=""urn:www.energie-efactuur.nl:profile:invoice:ver1.0"">
					<UtilityConsumptionPoint>
						<ID xmlns=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"">ConsumptionPointType_ID</ID>
					</UtilityConsumptionPoint>
				</SEEFExtensionWrapper>
			</ExtensionContent>
		</UBLExtension>
	</UBLExtensions>
	<cbc:ID>id</cbc:ID>
</Invoice>";

            UblXmlComparer.IsCopyEqual(XDocument.Parse(expected), XDocument.Parse(sb.ToString()), _output).Should().BeTrue();
        }
    }
}
