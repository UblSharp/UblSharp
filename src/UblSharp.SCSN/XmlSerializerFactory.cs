using System;
#if NETSTANDARD1_0 || NETSTANDARD1_3
using System.Linq;
#endif
using System.Reflection;
using System.Xml.Serialization;

namespace UblSharp.SCSN
{
    public class XmlSerializerFactory
    {
        private const string XmlnsV1 = "SmartConnectedSupplierNetwork:schema:xsd:BillOfMaterials";

        private static readonly XmlSerializer s_serializerV1;

        public static XmlSerializerFactory Default { get; } = new XmlSerializerFactory();

        static XmlSerializerFactory()
        {
            s_serializerV1 = new XmlSerializer(typeof(BillOfMaterialsType));

#if NETSTANDARD1_0 || NETSTANDARD1_3
            var assembly = typeof(BillOfMaterialsType).GetTypeInfo().Assembly;
#else
            var assembly = typeof(BillOfMaterialsType).Assembly;
#endif

            var overrides = CreateXmlAttributeOverrides(assembly, XmlnsV1);

            s_serializerV1 = new XmlSerializer(typeof(BillOfMaterialsType), overrides);
        }

        public virtual XmlSerializer GetSerializer()
        {
            return s_serializerV1;
        }

        protected static XmlAttributeOverrides CreateXmlAttributeOverrides(Assembly assemblytoScan, string xmlns)
        {
            var overrides = new XmlAttributeOverrides();
#if NETSTANDARD1_0 || NETSTANDARD1_3
            var types = assemblytoScan.ExportedTypes;
#else
            var types = assemblytoScan.GetExportedTypes();
#endif
            foreach (var type in types)
            {
#if NETSTANDARD1_0 || NETSTANDARD1_3
                var rootAttrs = type.GetTypeInfo().GetCustomAttributes(typeof(XmlRootAttribute), false).ToArray();
                var typeAttrs = type.GetTypeInfo().GetCustomAttributes(typeof(XmlTypeAttribute), false).ToArray();
#else
                var rootAttrs = type.GetCustomAttributes(typeof(XmlRootAttribute), false);
                var typeAttrs = type.GetCustomAttributes(typeof(XmlTypeAttribute), false);
#endif
                if (rootAttrs.Length == 0
                    && typeAttrs.Length == 0)
                {
                    continue;
                }

                var attrs = new XmlAttributes();
                if (rootAttrs.Length > 0)
                {
                    var rootAttr = (XmlRootAttribute)rootAttrs[0];
                    rootAttr.Namespace = xmlns;
                    attrs.XmlRoot = rootAttr;
                }

                if (typeAttrs.Length > 0)
                {
                    var typeAttr = (XmlTypeAttribute)typeAttrs[0];
                    typeAttr.Namespace = xmlns;
                    attrs.XmlType = typeAttr;
                }

                overrides.Add(type, attrs);
            }

            return overrides;
        }
    }
}
