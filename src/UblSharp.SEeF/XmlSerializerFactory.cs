using System;
#if NETSTANDARD1_0 || NETSTANDARD1_3
using System.Linq;
#endif
using System.Reflection;
using System.Xml.Serialization;

namespace UblSharp.SEeF
{
    public class XmlSerializerFactory
    {
        // private const string XmlnsV1 = "urn:www.energie-efactuur.nl:profile:invoice:ver1.0";
        private const string XmlnsV2 = "urn:www.energie-efactuur.nl:profile:invoice:ver2.0";

        private static readonly XmlSerializer s_serializerV1;
        private static readonly XmlSerializer s_serializerV2;

        public static XmlSerializerFactory Default { get; } = new XmlSerializerFactory();

        static XmlSerializerFactory()
        {
            s_serializerV1 = new XmlSerializer(typeof(SEEFExtensionWrapperType));

#if NETSTANDARD1_0 || NETSTANDARD1_3
            var assembly = typeof(SEEFExtensionWrapperType).GetTypeInfo().Assembly;
#else
            var assembly = typeof(SEEFExtensionWrapperType).Assembly;
#endif

            var overrides = CreateXmlAttributeOverrides(assembly, XmlnsV2);

            s_serializerV2 = new XmlSerializer(typeof(SEEFExtensionWrapperType), overrides);
        }

        public virtual XmlSerializer GetSerializer(SEeFVersion version = SEeFVersion.V1)
        {
            if (version == SEeFVersion.V1)
            {
                return s_serializerV1;
            }

            if (version == SEeFVersion.V2)
            {
                return s_serializerV2;
            }

            throw new ArgumentException("Invalid SEeFVersion", nameof(version));
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
