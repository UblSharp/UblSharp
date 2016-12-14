using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace UblSharp
{
    public static class BaseDocumentExtensions
    {
        private static readonly XmlWriterSettings _xmlWriterSettings = new XmlWriterSettings()
        {
            CloseOutput = false,
            Indent = true,
            IndentChars = "\t",
#if !(NET20 || NET35)
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
#endif
            Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };

        public static XmlSerializer GetSerializer<T>(this T document)
            where T : IBaseDocument
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            return UblDocument.GetSerializer(document.GetType());
        }

        public static void WriteTo<T>(this T document, XmlWriter writer)
            where T : IBaseDocument
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            GetSerializer(document).Serialize(writer, document);
        }

        public static void Save<T>(this T document, Stream stream)
            where T : IBaseDocument
        {
            using (var writer = XmlWriter.Create(stream, _xmlWriterSettings))
            {
                Save(document, writer);
            }
        }

#if !NETSTANDARD1_0
        public static void Save<T>(this T document, string fileName)
            where T : IBaseDocument
        {
            using (var stream = File.CreateText(fileName))
            using (var writer = XmlWriter.Create(stream, _xmlWriterSettings))
            {
                Save(document, writer);
            }
        }
#endif

        public static void Save<T>(this T document, TextWriter writer)
            where T : IBaseDocument
        {
            using (var xmlWriter = XmlWriter.Create(writer, _xmlWriterSettings))
            {
                Save(document, xmlWriter);
            }
        }

        public static void Save<T>(this T document, XmlWriter writer)
            where T : IBaseDocument
        {
            WriteTo(document, writer);
        }
    }
}