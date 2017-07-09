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
        private static readonly Dictionary<Type, XmlSerializer> s_typeCache = new Dictionary<Type, XmlSerializer>();

        private static readonly XmlReaderSettings s_xmlReaderSettings = new XmlReaderSettings()
        {
            CheckCharacters = false,
            IgnoreWhitespace = true,
#if !(NET20 || NET35) // DtdProcessing is not in these frameworks
            // DtdProcessing = DtdProcessing.Parse, // Parse is not defined in .net standard, so use it's integer value
            DtdProcessing = (DtdProcessing)2,
#endif
        };

        private static readonly XmlWriterSettings s_xmlWriterSettings = new XmlWriterSettings()
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
            using (var rdr = XmlReader.Create(stream, s_xmlReaderSettings))
            {
                return Load<T>(rdr);
            }
        }

        public static T Load<T>(string uri) where T : IBaseDocument
        {
            using (var rdr = XmlReader.Create(uri, s_xmlReaderSettings))
            {
                return Load<T>(rdr);
            }
        }

        public static T Load<T>(TextReader reader) where T : IBaseDocument
        {
            using (var rdr = XmlReader.Create(reader, s_xmlReaderSettings))
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
            using (var xmlRdr = XmlReader.Create(rdr, s_xmlReaderSettings))
            {
                return Load<T>(xmlRdr);
            }
        }

        public static XmlSerializer GetSerializer(Type type)
        {
            XmlSerializer serializer;
            if (!s_typeCache.TryGetValue(type, out serializer))
            {
                serializer = new XmlSerializer(type);
                s_typeCache[type] = serializer;
            }

            return serializer;
        }

        public static XmlSerializer GetSerializer<T>()
            where T : IBaseDocument
            => GetSerializer(typeof(T));

#if FEATURE_XMLDOCUMENT
        public static T Load<T>(XmlDocument document) 
            where T : IBaseDocument
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

#if NETSTANDARD1_0 || NETSTANDARD1_3
            using (var memStream = new MemoryStream())
            using (var xmlWriter = XmlWriter.Create(memStream, s_xmlWriterSettings))
            {
                document.Save(xmlWriter);

                memStream.Seek(0, SeekOrigin.Begin);
                
                using (var reader = XmlReader.Create(memStream, s_xmlReaderSettings))
                {
                    return Load<T>(reader);
                }
            }
#else
            using (var reader = new XmlNodeReader(document))
            {
                return Load<T>(reader);
            }
#endif
        }
#endif

#if FEATURE_LINQ
        public static T Load<T>(XDocument document) 
            where T : IBaseDocument
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

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

        public static void Save<T>(T document, Stream stream)
            where T : IBaseDocument
        {
            using (var writer = XmlWriter.Create(stream, s_xmlWriterSettings))
            {
                Save(document, writer);
            }
        }

#if !NETSTANDARD1_0
        public static void Save<T>(T document, string fileName)
            where T : IBaseDocument
        {
            using (var stream = File.CreateText(fileName))
            using (var writer = XmlWriter.Create(stream, s_xmlWriterSettings))
            {
                Save(document, writer);
            }
        }
#endif

        public static void Save<T>(T document, TextWriter writer)
            where T : IBaseDocument
        {
            using (var xmlWriter = XmlWriter.Create(writer, s_xmlWriterSettings))
            {
                Save(document, xmlWriter);
            }
        }

        public static void Save<T>(T document, XmlWriter writer)
            where T : IBaseDocument
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            var serializer = GetSerializer(document.GetType());
#if DISABLE_XMLNSDECL
            var baseDoc = document as BaseDocument;
            if (baseDoc != null)
            {
                serializer.Serialize(writer, document, baseDoc.Xmlns);
            }
            else
            {
                serializer.Serialize(writer, document);
            }
#else
            serializer.Serialize(writer, document);
#endif
        }

#if FEATURE_LINQ
        public static XDocument ToXDocument<T>(T document)
            where T : IBaseDocument
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var writer = XmlWriter.Create(sw))
            {
                document.Save(writer);
            }

            return XDocument.Parse(sb.ToString());
        }
#endif
    }
}
