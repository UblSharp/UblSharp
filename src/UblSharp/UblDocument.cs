using System;
using System.IO;
using System.Text;
using System.Xml;
#if FEATURE_LINQ
using System.Xml.Linq;
#endif

namespace UblSharp
{
    public static class UblDocument
    {
        public static T FromStream<T>(Stream stream)
            where T : BaseDocument
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            return FromStream<T>(stream, Encoding.UTF8);
        }

        public static T FromStream<T>(Stream stream, Encoding encoding)
            where T : BaseDocument
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            using (var rdr = new StreamReader(stream, encoding))
            {
                return (T)UblDocumentManager.Default.GetSerializer(typeof(T)).Deserialize(rdr);
            }
        }

        public static T FromXmlReader<T>(XmlReader reader)
            where T : BaseDocument
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            return (T)UblDocumentManager.Default.GetSerializer(typeof(T)).Deserialize(reader);
        }

#if FEATURE_LINQ
        public static T FromXDocument<T>(XDocument document)
            where T : BaseDocument
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            using (var reader = new StringReader(document.ToString()))
            {
                return (T)UblDocumentManager.Default.GetSerializer(typeof(T)).Deserialize(reader);
            }
        }
#endif
    }
}