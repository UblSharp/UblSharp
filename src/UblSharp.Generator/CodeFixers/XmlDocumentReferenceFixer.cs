using System.CodeDom;
using System.Collections.Generic;

namespace UblSharp.Generator.CodeFixers
{
    public class XmlDocumentReferenceFixer : CodeNamespaceVisitor
    {
        private readonly Dictionary<string, string> _typesToReplace = new Dictionary<string, string>()
        {
            { "System.Xml.XmlNode", "System.Xml.Linq.XNode" },
            { "System.Xml.XmlElement", "System.Xml.Linq.XElement" },
            { "System.Xml.XmlAttribute", "System.Xml.Linq.XAttribute" },
            { "System.Xml.XmlNode[]", "System.Xml.Linq.XNode[]" },
            { "System.Xml.XmlElement[]", "System.Xml.Linq.XElement[]" },
            { "System.Xml.XmlAttribute[]", "System.Xml.Linq.XAttribute[]" }
        };

        protected override void VisitField(CodeMemberField member)
        {
            if (_typesToReplace.ContainsKey(member.Type.BaseType))
            {
                if (member.Type.ArrayElementType != null)
                {
                    member.Type.ArrayElementType = new CodeTypeReference(_typesToReplace[member.Type.BaseType]);
                }
                else
                {
                    member.Type.BaseType = _typesToReplace[member.Type.BaseType];
                }
            }

            base.VisitField(member);
        }

        protected override void VisitProperty(CodeMemberProperty member)
        {
            if (_typesToReplace.ContainsKey(member.Type.BaseType))
            {
                if (member.Type.ArrayElementType != null)
                {
                    member.Type.ArrayElementType = new CodeTypeReference(_typesToReplace[member.Type.BaseType]);
                }
                else
                {
                    member.Type.BaseType = _typesToReplace[member.Type.BaseType];
                }
            }

            base.VisitProperty(member);
        }
    }
}