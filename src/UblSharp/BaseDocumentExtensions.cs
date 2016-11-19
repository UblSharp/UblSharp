using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace UblSharp
{
    public static class BaseDocumentExtensions
    {
        public static XmlSerializer GetSerializer(this BaseDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            return UblDocumentManager.Default.GetSerializer(document.GetType());
        }

        public static void Serialize(this BaseDocument document, Stream stream)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var type = document.GetType();
            var serializer = UblDocumentManager.Default.GetSerializer(type);
            serializer.Serialize(stream, document);
        }

        public static void Serialize(this BaseDocument document, TextWriter writer)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            var type = document.GetType();
            var serializer = UblDocumentManager.Default.GetSerializer(type);
            serializer.Serialize(writer, document);
        }

        public static void Serialize(this BaseDocument document, XmlWriter writer)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            var type = document.GetType();
            var serializer = UblDocumentManager.Default.GetSerializer(type);
            serializer.Serialize(writer, document);
        }
    }
}