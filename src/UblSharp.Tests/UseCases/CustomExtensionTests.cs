using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FluentAssertions;
using UblSharp.CommonExtensionComponents;
using UblSharp.CoreComponentTypes;
using Xunit;
using Xunit.Abstractions;

namespace UblSharp.Tests.UseCases
{
    // Define 'ValueType' because they are not defined in UblSharp (they are abstracted away to their 'Core component type')
    [XmlType("Value", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
    [XmlRoot(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2", IsNullable = true)]
    public class ValueType : CctTextType
    {
    }

    public class CustomExtensionTests
    {
        private readonly ITestOutputHelper _output;

        public CustomExtensionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanSerializeCustomCbcType()
        {
            var xmlSerializer = new XmlSerializer(typeof(ValueType));

            var value = new ValueType()
            {
                Value = "SomeValue"
            };

            // Create a temporary XmlDocument
            var doc = new XmlDocument();

            // Create an XmlWriter which writes elements to this document
            using (var writer = doc.CreateNavigator().AppendChild())
            {
                // Serialize our custom type to this XmlWriter
                xmlSerializer.Serialize(writer, value);
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

                sb.ToString()
                    .Replace("\r\n", "\n")
                    .Should().Be(
                        @"<?xml version=""1.0"" encoding=""utf-16""?>
<Invoice xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"" xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"" xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2"">
	<UBLExtensions xmlns=""urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"">
		<UBLExtension xmlns:sig=""urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2"" xmlns:sbc=""urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2"">
			<ExtensionContent>
				<Value xmlns=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"">SomeValue</Value>
			</ExtensionContent>
		</UBLExtension>
	</UBLExtensions>
	<cbc:ID>id</cbc:ID>
</Invoice>".Replace("\r\n", "\n"));
            }
        }

    }
}
