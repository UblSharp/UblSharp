using System.Xml;
using System.Xml.Linq;

namespace UblSharp.Tests.Util
{
    public static class XElementExtensions
    {
        public static XmlElement ToXmlElement(this string str)
        {
            var doc = new XmlDocument();
            doc.LoadXml(str);
            return doc.DocumentElement;
        }

        public static XmlElement ToXmlElement(this XElement el)
        {
            var doc = new XmlDocument();
            using (var rdr = el.CreateReader())
            {
                doc.Load(rdr);
            }
            return doc.DocumentElement;
        }
    }
}