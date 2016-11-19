using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace UblSharp
{
    public class UblDocumentManager
    {
        private readonly Dictionary<Type, XmlSerializer> _typeCache = new Dictionary<Type, XmlSerializer>();

        public static UblDocumentManager Default { get; set; } = new UblDocumentManager();

        public XmlWriterSettings XmlWriterSettings { get; set; } = new XmlWriterSettings()
        {
            CloseOutput = false,
            Indent = true,
            IndentChars = "\t",
#if !(NET20 || NET35)
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
#endif
            Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };

        public virtual XmlSerializer GetSerializer(Type type)
        {
            XmlSerializer serializer;
            if (!_typeCache.TryGetValue(type, out serializer))
            {
                _typeCache[type] = serializer = new XmlSerializer(type);
            }

            return serializer;
        }
    }

    public static class UblDocumentManagerExtensions
    {
        public static XmlSerializer GetSerializer<T>(this UblDocumentManager manager) => manager.GetSerializer(typeof(T));
    }
}