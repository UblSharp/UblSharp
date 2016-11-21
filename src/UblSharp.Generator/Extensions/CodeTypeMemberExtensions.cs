using System.CodeDom;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace UblSharp.Generator.Extensions
{
    public static class CodeTypeMemberExtensions
    {
        public const string QualifiedNameKey = "QualifiedName";
        public const string XmlSchemaTypeKey = "XmlSchemaType";
        public const string XmlSchemaKey = "XmlSchema";

        public static CodeAttributeArgument GetNamespaceAttributeArgument(this CodeTypeMember typeMember)
        {
            return typeMember.CustomAttributes.OfType<CodeAttributeDeclaration>().Single(d => d.Name == "System.Xml.Serialization.XmlTypeAttribute")
                .Arguments.Cast<CodeAttributeArgument>().Single(a => a.Name == "Namespace");
        }

        public static XmlQualifiedName GetQualifiedName(this CodeTypeMember typeMember)
        {
            if (typeMember.UserData[QualifiedNameKey] == null)
            {
                var name = typeMember.Name;
                while (char.IsDigit(name.Last()))
                {
                    name = name.Substring(0, name.Length - 1);
                }

                var codeDeclTargetNamespace = ((CodePrimitiveExpression)GetNamespaceAttributeArgument(typeMember).Value).Value as string;
                typeMember.UserData[QualifiedNameKey] = new XmlQualifiedName(name, codeDeclTargetNamespace);
            }

            return typeMember.UserData[QualifiedNameKey] as XmlQualifiedName;
        }

        public static bool HasAnyXmlTextAttribute(this CodeTypeMember typeMember)
        {
            return typeMember.CustomAttributes.Cast<CodeAttributeDeclaration>().Any(a => a.Name == "System.Xml.Serialization.XmlTextAttribute");
        }

        public static XmlSchema GetSchema(this CodeTypeMember typeMember)
        {
            return typeMember.UserData[XmlSchemaKey] as XmlSchema;
        }

        public static XmlSchemaType GetXmlSchemaType(this CodeTypeMember typeMember)
        {
            return typeMember.UserData[XmlSchemaTypeKey] as XmlSchemaType;
        }

        public static bool IsMaindocSchema(this CodeTypeMember typeMember)
        {
            var uri = typeMember.GetSchema().SourceUri;
            return uri.Contains("maindoc") && !uri.Contains("BaseDocument");
        }

        public static bool HasAnyRequiredMembers(this CodeTypeMember typeMember)
        {
            var xmlComplexType = GetXmlSchemaType(typeMember) as XmlSchemaComplexType;
            if (xmlComplexType != null)
            {
                var content = (xmlComplexType.ContentModel as XmlSchemaSimpleContent)?.Content;
                if (content != null)
                {
                    var restriction = content as XmlSchemaSimpleContentRestriction;
                    var extension = content as XmlSchemaSimpleContentExtension;
                    var attr = restriction?.Attributes ?? extension?.Attributes;
                    if (attr == null)
                    {
                        return false;
                    }

                    return attr.Cast<XmlSchemaAttribute>().Any(a => a.Use == XmlSchemaUse.Required);
                }

                return xmlComplexType.Attributes.Cast<XmlSchemaAttribute>().Any(a => a.Use == XmlSchemaUse.Required);
            }

            return false;
        }
    }
}