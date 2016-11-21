using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using UblSharp.Generator.Extensions;

namespace UblSharp.Generator.CodeFixers
{
    public class XmlAttributesFixer : CodeNamespaceVisitor
    {
        private readonly string[] _attributesToRemove = { "System.ComponentModel.DesignerCategoryAttribute", "System.CodeDom.Compiler.GeneratedCodeAttribute" };

        protected CodeTypeDeclaration CurrentType { get; set; }

        protected override void VisitType(CodeTypeDeclaration type)
        {
            CurrentType = type;
            var attributes = type.CustomAttributes.Cast<CodeAttributeDeclaration>().ToList();

            foreach (var attributeName in _attributesToRemove)
            {
                var attr = attributes.FirstOrDefault(x => x.Name == attributeName);
                if (attr != null)
                {
                    type.CustomAttributes.Remove(attr);
                }
            }

            var xmlElAttr = attributes.FirstOrDefault(x => x.Name == "System.Xml.Serialization.XmlTypeAttribute");
            if (xmlElAttr != null)
            {
                var arg = xmlElAttr.Arguments.Cast<CodeAttributeArgument>().FirstOrDefault(x => x.Name == "" || x.Name == "Name");
                if (arg == null)
                {
                    var rootName = ((CodePrimitiveExpression)attributes.FirstOrDefault(x => x.Name == "System.Xml.Serialization.XmlRootAttribute")
                                       ?.Arguments.Cast<CodeAttributeArgument>().FirstOrDefault(x => x.Name == "" || x.Name == "Name")?.Value)?.Value as string
                                   ?? type.GetQualifiedName().Name;
                    xmlElAttr.Arguments.Insert(0, new CodeAttributeArgument(new CodePrimitiveExpression(rootName)));
                }
            }

            //var xmlRootAttr = attributes.FirstOrDefault(x => x.Name == "System.Xml.Serialization.XmlRootAttribute");

            //var rootnamespaces = new[] { "maindoc", "xmldsig" };
            //if (!rootnamespaces.Any(x=> type.GetSchema().TargetNamespace.Contains(x)) && xmlRootAttr != null)
            //{
            //    type.CustomAttributes.Remove(xmlRootAttr);
            //}

            // FixXmlSignatureType(type, attributes);

            base.VisitType(type);
        }

        private void FixXmlSignatureType(CodeTypeDeclaration type, List<CodeAttributeDeclaration> attributes)
        {
            if (type.Name == "XmlSignatureType")
            {
                var xmlElAttr = attributes.FirstOrDefault(x => x.Name == "System.Xml.Serialization.XmlRootAttribute");
                if (xmlElAttr == null)
                {
                    var ns = ((CodePrimitiveExpression)attributes.FirstOrDefault(x => x.Name == "System.Xml.Serialization.XmlTypeAttribute")
                                       ?.Arguments.Cast<CodeAttributeArgument>().FirstOrDefault(x => x.Name == "Namespace")?.Value)?.Value as string
                                   ?? type.GetQualifiedName().Namespace;
                    var rootName = ((CodePrimitiveExpression)attributes.FirstOrDefault(x => x.Name == "System.Xml.Serialization.XmlTypeAttribute")
                                       ?.Arguments.Cast<CodeAttributeArgument>().FirstOrDefault(x => x.Name == "" || x.Name == "Name")?.Value)?.Value as string
                                   ?? type.GetQualifiedName().Name;
                    type.CustomAttributes.Add(new CodeAttributeDeclaration("System.Xml.Serialization.XmlRootAttribute", new CodeAttributeArgument(new CodePrimitiveExpression(rootName)), new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(ns))));
                }
            }
        }

        protected override void VisitField(CodeMemberField member)
        {
            var attributes = member.CustomAttributes.Cast<CodeAttributeDeclaration>().ToList();
            if (attributes.Count == 0)
            {
                member.CustomAttributes.Add(new CodeAttributeDeclaration()
                {
                    Name = "System.Xml.Serialization.XmlElementAttribute",
                    Arguments =
                    {
                        new CodeAttributeArgument(new CodePrimitiveExpression(member.Name)),
                        // new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(member.GetSchema().TargetNamespace))
                    }
                });
            }
            else
            {
                var xmlElAttr = attributes.FirstOrDefault(x => x.Name == "System.Xml.Serialization.XmlElementAttribute");
                if (xmlElAttr != null)
                {
                    var arg = xmlElAttr.Arguments.Cast<CodeAttributeArgument>().FirstOrDefault(x => x.Name == "" || x.Name == "Name");
                    if (arg == null)
                    {
                        xmlElAttr.Arguments.Insert(0, new CodeAttributeArgument(new CodePrimitiveExpression(member.Name)));
                    }
                }

                xmlElAttr = attributes.FirstOrDefault(x => x.Name == "System.Xml.Serialization.XmlAttributeAttribute");
                if (xmlElAttr != null)
                {
                    var arg = xmlElAttr.Arguments.Cast<CodeAttributeArgument>().FirstOrDefault(x => x.Name == "" || x.Name == "Name");
                    if (arg == null)
                    {
                        xmlElAttr.Arguments.Insert(0, new CodeAttributeArgument(new CodePrimitiveExpression(member.Name)));
                    }
                }

                xmlElAttr = attributes.FirstOrDefault(x => x.Name == "System.Xml.Serialization.XmlArrayAttribute");
                if (xmlElAttr != null)
                {
                    var arg = xmlElAttr.Arguments.Cast<CodeAttributeArgument>().FirstOrDefault(x => x.Name == "" || x.Name == "Name");
                    if (arg == null)
                    {
                        xmlElAttr.Arguments.Insert(0, new CodeAttributeArgument(new CodePrimitiveExpression(member.Name)));
                    }
                }
            }

            // Ignore Value of TimeType
            if ((CurrentType.Name == "TimeType" || CurrentType.Name == "DateType")
                && member.Name == "Value")
            {
                var attr = attributes.Single(x => x.Name == "System.Xml.Serialization.XmlTextAttribute");
                member.CustomAttributes.Remove(attr);
                member.CustomAttributes.Add(new CodeAttributeDeclaration("System.Xml.Serialization.XmlIgnoreAttribute"));
                member.Type = new CodeTypeReference(typeof(System.DateTimeOffset));
            }

            base.VisitField(member);
        }
    }
}