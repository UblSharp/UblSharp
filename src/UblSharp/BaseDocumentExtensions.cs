using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace UblSharp
{
    public static class BaseDocumentExtensions
    {
        public static XmlSerializer GetSerializer<T>(this T document)
            where T : IBaseDocument
        {
            return UblDocument.GetSerializer(document.GetType());
        }

        public static void Save<T>(this T document, Stream stream)
            where T : IBaseDocument
        {
            UblDocument.Save(document, stream);
        }

#if !NETSTANDARD1_0
        public static void Save<T>(this T document, string fileName)
            where T : IBaseDocument
        {
            UblDocument.Save(document, fileName);
        }
#endif

        public static void Save<T>(this T document, TextWriter writer)
            where T : IBaseDocument
        {
            UblDocument.Save(document, writer);
        }

        public static void Save<T>(this T document, XmlWriter writer)
            where T : IBaseDocument
        {
            UblDocument.Save(document, writer);
        }
    }
}