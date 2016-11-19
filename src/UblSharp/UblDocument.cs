using System;
using System.IO;
using System.Text;

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
                return (T)UblDocumentManager.Default.GetSerializer<T>().Deserialize(rdr);
            }
        }
    }
}