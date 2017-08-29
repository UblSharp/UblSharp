using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace UblSharp.CommonExtensionComponents
{
    public partial class UBLExtensionType
    {
        // xmlns:sig="urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2"
        // xmlns:sac="urn:oasis:names:specification:ubl:schema:xsd:SignatureAggregateComponents-2"
        // xmlns:sbc="urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2"
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static XmlSerializerNamespaces DefaultXmlns { get; set; } = new XmlSerializerNamespaces(new[]
        {
            new XmlQualifiedName("sig", "urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2"),
            new XmlQualifiedName("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"),
            new XmlQualifiedName("sbc", "urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2")
        });

        [EditorBrowsable(EditorBrowsableState.Advanced)]
#if !DISABLE_XMLNSDECL
        [XmlNamespaceDeclarations]
#else
        [XmlIgnore]
#endif
        public XmlSerializerNamespaces Xmlns { get; set; } = DefaultXmlns;
    }
}
