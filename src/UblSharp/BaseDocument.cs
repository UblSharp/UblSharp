using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using UblSharp.CommonAggregateComponents;
using UblSharp.CommonExtensionComponents;
using UblSharp.UnqualifiedDataTypes;

namespace UblSharp
{
    public partial class BaseDocument
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static XmlSerializerNamespaces DefaultXmlns { get; set; } = new XmlSerializerNamespaces(new[]
        {
            new XmlQualifiedName("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"),
            new XmlQualifiedName("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")
        });

        [EditorBrowsable(EditorBrowsableState.Advanced)]
#if !(NETSTANDARD1_3 || NETSTANDARD1_0)
        [XmlNamespaceDeclarations]
#else
        [XmlIgnore]
#endif
        public XmlSerializerNamespaces Xmlns { get; set; } = DefaultXmlns;

        [XmlIgnore]
        public List<UBLExtensionType> UBLExtensions
        {
            get { return ((IBaseDocument)this).UBLExtensions; }
            set { ((IBaseDocument)this).UBLExtensions = value; }
        }

        [XmlIgnore]
        public IdentifierType UBLVersionID
        {
            get { return ((IBaseDocument)this).UBLVersionID; }
            set { ((IBaseDocument)this).UBLVersionID = value; }
        }

        [XmlIgnore]
        public IdentifierType CustomizationID
        {
            get { return ((IBaseDocument)this).CustomizationID; }
            set { ((IBaseDocument)this).CustomizationID = value; }
        }

        [XmlIgnore]
        public IdentifierType ProfileID
        {
            get { return ((IBaseDocument)this).ProfileID; }
            set { ((IBaseDocument)this).ProfileID = value; }
        }

        [XmlIgnore]
        public IdentifierType ProfileExecutionID
        {
            get { return ((IBaseDocument)this).ProfileExecutionID; }
            set { ((IBaseDocument)this).ProfileExecutionID = value; }
        }

        [XmlIgnore]
        public IdentifierType ID
        {
            get { return ((IBaseDocument)this).ID; }
            set { ((IBaseDocument)this).ID = value; }
        }

        [XmlIgnore]
        public IdentifierType UUID
        {
            get { return ((IBaseDocument)this).UUID; }
            set { ((IBaseDocument)this).UUID = value; }
        }

        [XmlIgnore]
        public List<SignatureType> Signature
        {
            get { return ((IBaseDocument)this).Signature; }
            set { ((IBaseDocument)this).Signature = value; }
        }
    }
}