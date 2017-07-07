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
        
        public static XmlReaderSettings XmlReaderSettings { get; } = new XmlReaderSettings()
        {
            CheckCharacters = false,
            IgnoreWhitespace = true,
#if !(NET20 || NET35) // DtdProcessing is not in these frameworks
            // DtdProcessing = DtdProcessing.Parse, // Parse is not defined in .net standard, so use it's integer value
            DtdProcessing = (DtdProcessing)2,
#endif
        };

        public static XmlWriterSettings XmlWriterSettings { get; } = new XmlWriterSettings()
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
            using (var rdr = XmlReader.Create(stream, XmlReaderSettings))
            {
                return Load<T>(rdr);
            }
        }

        public static T Load<T>(string uri) where T : IBaseDocument
        {
            using (var rdr = XmlReader.Create(uri, XmlReaderSettings))
            {
                return Load<T>(rdr);
            }
        }

        public static T Load<T>(TextReader reader) where T : IBaseDocument
        {
            using (var rdr = XmlReader.Create(reader, XmlReaderSettings))
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
            using (var xmlRdr = XmlReader.Create(rdr, XmlReaderSettings))
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
            if (document == null)
                throw new ArgumentNullException(nameof(document));

#if NETSTANDARD1_0 || NETSTANDARD1_3
            using (var memStream = new MemoryStream())
            using (var xmlWriter = XmlWriter.Create(memStream, XmlWriterSettings))
            {
                document.Save(xmlWriter);

                memStream.Seek(0, SeekOrigin.Begin);
                
                using (var reader = XmlReader.Create(memStream, XmlReaderSettings))
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
        public static T Load<T>(XDocument document) where T : BaseDocument
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
    }
}