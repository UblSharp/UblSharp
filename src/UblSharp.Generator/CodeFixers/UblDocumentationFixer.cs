using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using UblSharp.Generator.Extensions;

namespace UblSharp.Generator.CodeFixers
{
    public class UblDocumentationFixer : CodeNamespaceVisitor
    {
        private Dictionary<string, XmlSchemaDocumentation> _fieldDocumentationLookup;

        protected override void VisitType(CodeTypeDeclaration type)
        {
            _fieldDocumentationLookup = new Dictionary<string, XmlSchemaDocumentation>();

            var xsdComplexType = type.GetXmlSchemaType() as XmlSchemaComplexType;
            if (xsdComplexType == null)
            {
                return;
            }

            // Add comment to class
            var xsdDocNode = xsdComplexType.Annotation?.Items.OfType<XmlSchemaDocumentation>().FirstOrDefault();
            if (xsdDocNode != null)
            {
                var textLines = GetDocumentation(xsdDocNode);
                foreach (var line in textLines)
                {
                    type.Comments.Add(new CodeCommentStatement(line, true));
                }
            }

            if (type.Members.OfType<CodeMemberField>().All(x => x.HasAnyXmlTextAttribute()))
            {
                // Types without own properties
                return;
            }

            switch (xsdComplexType.ContentType)
            {
                case XmlSchemaContentType.TextOnly: // simpleContent, DateType
                    var sext = (XmlSchemaSimpleContentExtension)((XmlSchemaSimpleContent)xsdComplexType.ContentModel).Content;
                    _fieldDocumentationLookup = sext.Attributes.OfType<XmlSchemaAttribute>().ToDictionary(k => k.Name, v => v.Annotation?.Items.OfType<XmlSchemaDocumentation>().First());
                    break;
                case XmlSchemaContentType.Empty:
                    break;
                case XmlSchemaContentType.ElementOnly:
                case XmlSchemaContentType.Mixed:
                    if (xsdComplexType.ContentTypeParticle is XmlSchemaSequence)
                    {
                        var schemaSequence = xsdComplexType.ContentTypeParticle as XmlSchemaSequence;
                        _fieldDocumentationLookup = schemaSequence.Items.OfType<XmlSchemaElement>().ToDictionary(k => k.QualifiedName.Name, v => v.Annotation?.Items.OfType<XmlSchemaDocumentation>().FirstOrDefault());
                    }
                    else if (xsdComplexType.ContentTypeParticle is XmlSchemaChoice)
                    {
                        var schemaChoice = xsdComplexType.ContentTypeParticle as XmlSchemaChoice;
                        _fieldDocumentationLookup = schemaChoice.Items.OfType<XmlSchemaElement>().ToDictionary(k => k.QualifiedName.Name, v => v.Annotation?.Items.OfType<XmlSchemaDocumentation>().FirstOrDefault());
                    }
                    break;
            }

            base.VisitType(type);
        }

        protected override void VisitField(CodeMemberField member)
        {
            var memberName = member.Name;
            if (_fieldDocumentationLookup.ContainsKey(memberName))
            {
                var textLines = GetDocumentation(_fieldDocumentationLookup[memberName]);
                foreach (var line in textLines)
                {
                    member.Comments.Add(new CodeCommentStatement(line, true));
                }
            }
        }

        private static string[] GetDocumentation(XmlSchemaDocumentation doc)
        {
            char[] trimChars = new[] { ' ', '\t', '\n', '\r' };
            var summary = new List<string>(); ;
            var remarks = new List<string>();
            var example = "";

            if (doc != null && doc.Markup.Any())
            {
                var node = doc.Markup.First();
                if (node.NodeType == XmlNodeType.Text)
                {
                    // return pure text
                    summary = doc.Markup.OfType<XmlText>()
                        .Select(s => s.Value.Trim(trimChars))
                        .ToList();
                }
                else if (node.NodeType == XmlNodeType.Element)
                {
                    // xml to text
                    string xml = string.Empty;
                    if (node.Name == "ccts:Component")
                    {
                        xml = node.OuterXml;
                    }
                    else if (node.Name.StartsWith("ccts:", StringComparison.Ordinal))
                    {
                        xml = $"<ccts:Component xmlns:ccts=\"urn:un:unece:uncefact:documentation:2\">{node.ParentNode?.InnerXml}</ccts:Component>";
                    }

                    var comp = XElement.Parse(xml);
                    summary = new List<string>() { comp.Elements().Where(e => e.Name.LocalName == "Definition").Select(e => $"{SecurityElement.Escape(e.Value.Trim(trimChars))}").FirstOrDefault() };
                    example = comp.Elements().Where(e => e.Name.LocalName == "Examples").Select(e => $"{e.Value.Trim(trimChars)}").FirstOrDefault();
                    var terms = comp.Elements().Where(e => e.Name.LocalName != "Definition" && e.Name.LocalName != "Examples").Select(e => new
                    {
                        Term = e.Name.LocalName,
                        Description = SecurityElement.Escape(e.Value.Trim(trimChars))
                    }).ToList();
                    if (terms.Any())
                    {
                        foreach (var term in terms)
                        {
                            summary.Add($"<para />{term.Term}: {term.Description}");
                        }
                    }

                    /*
                    if (terms.Any())
                    {
                        remarks.Add("<list type=\"table\">");
                        remarks.Add("  <listheader><term>Term</term><description>Description</description></listheader>");
                        foreach (var term in terms)
                        {
                            remarks.Add("  <item>");
                            remarks.Add($"    <term>{term.Term}</term>");
                            remarks.Add($"    <description>{term.Description}</description>");
                            remarks.Add("  </item>");
                        }
                        remarks.Add("</list>");
                    }
                    */
                }
            }

            var comments = new List<string>();
            if (summary.Any())
            {
                comments.Add("<summary>");
                comments.AddRange(summary);
                comments.Add("</summary>");
            }

            if (remarks.Any())
            {
                comments.Add("<remarks>");
                comments.AddRange(remarks);
                comments.Add("</remarks>");
            }

            if (!string.IsNullOrEmpty(example))
            {
                comments.Add($"<example>{SecurityElement.Escape(example)}</example>");
            }

            return comments.ToArray();
        }
    }
}