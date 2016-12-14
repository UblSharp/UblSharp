using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
#if FEATURE_LINQ
using System.Xml.Linq;
#endif

namespace UblSharp
{
    public static class UblDocument
    {
        private static readonly Dictionary<Type, XmlSerializer> _typeCache = new Dictionary<Type, XmlSerializer>();
        private static readonly XmlReaderSettings _xmlReaderSettings = new XmlReaderSettings()
        {
            IgnoreWhitespace = true,
            // DtdProcessing = DtdProcessing.Parse, -- Parse is not defined in .net standard
#if !(NET20 || NET35)
            DtdProcessing = (DtdProcessing)2,
#endif
            MaxCharactersFromEntities = (long)1e7,
        };

        public static XmlWriterSettings XmlWriterSettings { get; private set; } = new XmlWriterSettings()
        {
            CloseOutput = false,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
            IndentChars = "\t",
#if !(NET20 || NET35)
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
#endif
        };

        public static T Load<T>(Stream stream) where T : IBaseDocument
        {
            using (var rdr = XmlReader.Create(stream, _xmlReaderSettings))
            {
                return Load<T>(rdr);
            }
        }

        public static T Load<T>(string uri) where T : IBaseDocument
        {
            using (var rdr = XmlReader.Create(uri, _xmlReaderSettings))
            {
                return Load<T>(rdr);
            }
        }

        public static T Load<T>(TextReader reader) where T : IBaseDocument
        {
            using (var rdr = XmlReader.Create(reader, _xmlReaderSettings))
            {
                return Load<T>(rdr);
            }
        }

        public static T Load<T>(XmlReader reader) where T : IBaseDocument
        {
            return (T)GetSerializer<T>().Deserialize(reader);
        }

        public static T Parse<T>(string text) where T : IBaseDocument
        {
            using (var rdr = new StringReader(text))
            using (var xmlRdr = XmlReader.Create(rdr, _xmlReaderSettings))
            {
                return Load<T>(xmlRdr);
            }
        }

        public static XmlSerializer GetSerializer(Type type)
        {
            XmlSerializer serializer;
            if (!_typeCache.TryGetValue(type, out serializer))
            {
                serializer = new XmlSerializer(type);
                _typeCache[type] = serializer;
            }

            return serializer;
        }

        public static XmlSerializer GetSerializer<T>() => GetSerializer(typeof(T));

#if FEATURE_XMLDOCUMENT
        public static T Load<T>(XmlDocument document) where T : BaseDocument
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            // TODO: check if there's a better performing method reading directly from XmlDocument instead of copying to MemoryStream.
            using (var ms = new MemoryStream())
            {
                document.Save(ms);
                ms.Position = 0;
                using (var reader = XmlReader.Create(ms))
                {
                    return Load<T>(reader);
                }
            }
        }
#endif

#if FEATURE_LINQ
        public static T Load<T>(XDocument document) where T : BaseDocument
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            // TODO: check, there was something wrong with the XmlReader created by XDocument, can't remember what it was
            // disable for now until verified.
            //using (var reader = new StringReader(document.ToString()))
            //{
            //    return (T)UblDocumentManager.Default.GetSerializer(typeof(T)).Deserialize(reader);
            //}
#if NET20 || NET35
            using (var reader = document.CreateReader())
#else
            using (var reader = document.CreateReader(ReaderOptions.OmitDuplicateNamespaces))
#endif
            {
                return Load<T>(reader);
            }
        }
#endif
        }
}