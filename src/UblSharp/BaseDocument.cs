using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace UblSharp
{
    public partial class BaseDocument : IBaseDocument
    {
        private static readonly XmlSerializerNamespaces _xmlns = new XmlSerializerNamespaces(new[]
        {
            new XmlQualifiedName("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"),
            new XmlQualifiedName("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")
        });

        public BaseDocument()
        {
            UBLVersionID = "2.1";
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns { get; set; } = _xmlns;
    }
}