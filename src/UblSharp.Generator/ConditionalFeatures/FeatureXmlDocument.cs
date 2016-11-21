using System.Collections.Generic;

namespace UblSharp.Generator.ConditionalFeatures
{
    public static class FeatureXmlDocument
    {
        private static readonly Dictionary<string, string> _typesToReplace = new Dictionary<string, string>()
        {
            { "System.Xml.XmlNode", "System.Xml.Linq.XNode" },
            { "System.Xml.XmlElement", "System.Xml.Linq.XElement" },
            { "System.Xml.XmlAttribute", "System.Xml.Linq.XAttribute" },
        };

        public static int Add(List<string> lines, int lineNum)
        {
            foreach (var type in _typesToReplace)
            {
                lineNum = 0;
                while (true)
                {
                    lineNum = lines.FindIndex(lineNum, s => s.Contains(type.Key));
                    if (lineNum < 0)
                        break;

                    var origLine = lines[lineNum];
                    lines.Insert(lineNum, "#if FEATURE_XMLDOCUMENT");
                    lines.Insert(lineNum + 2, "#elif !FEATURE_XMLDOCUMENT && FEATURE_LINQ");
                    lines.Insert(lineNum + 3, origLine.Replace(type.Key, type.Value));
                    lines.Insert(lineNum + 4, "#else");
                    lines.Insert(lineNum + 5, origLine.Replace(type.Key, "object"));
                    lines.Insert(lineNum + 6, "#endif");
                    lineNum += 8;

                    // lines[lineNum] = origLine.Replace(type.Key, "object");
                }
            }

            return -1;
        }
    }
}