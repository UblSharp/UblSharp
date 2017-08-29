using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace UblSharp.Tests
{
    public class NamespacedTypeSerializationTests
    {
        private readonly ITestOutputHelper _output;

        public NamespacedTypeSerializationTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        public void CanSerializeTypeWithXmlNamespaceDeclarations()
        {
            var serializer = new XmlSerializer(typeof(TypeWithXmlNamespaceDeclarations));
            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb))
            {
                var obj = new TypeWithXmlNamespaceDeclarations();
                serializer.Serialize(xmlWriter, obj);
            }

            var xml = sb.ToString();
        }

        public class TypeWithXmlNamespaceDeclarations
        {
            [XmlNamespaceDeclarations]
            public XmlSerializerNamespaces Xmlns { get; set; } = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("somens", "urn:somenamespace")
            });
        }
    }
}
