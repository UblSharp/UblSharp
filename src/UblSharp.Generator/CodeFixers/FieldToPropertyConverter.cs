using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.CSharp;
using UblSharp.Generator.Extensions;

namespace UblSharp.Generator.CodeFixers
{
    public class FieldToPropertyConverter : CodeNamespaceVisitor
    {
        private readonly CSharpCodeProvider _csharp = new CSharpCodeProvider();

        private readonly string[] _systemTypes =
        {
            "System.Boolean",
            "System.Byte",
            "System.SByte",
            "System.Char",
            "System.Decimal",
            "System.Double",
            "System.Single",
            "System.Int32",
            "System.UInt32",
            "System.Int64",
            "System.UInt64",
            "System.Object",
            "System.Int16",
            "System.UInt16",
            "System.String",
            "System.DateTime",
            "System.Xml.XmlNode",
            "System.Xml.XmlElement",
            "System.Xml.XmlAttribute",
            "System.Xml.Linq.XNode",
            "System.Xml.Linq.XElement",
            "System.Xml.Linq.XAttribute"
        };

        protected override void VisitClass(CodeTypeDeclaration type)
        {
            FixType(type);
        }

        protected override void VisitInterface(CodeTypeDeclaration type)
        {
            FixType(type);
        }

        private void FixType(CodeTypeDeclaration type)
        {
            var properties = new List<CodeTypeMember>();
            foreach (var field in type.Members.OfType<CodeMemberField>())
            {
                var propertyName = field.Name;
                var fieldName = field.Name.MakePrivateFieldName();
                var snippet = new CodeSnippetTypeMember();
                var typeName = _csharp.GetTypeOutput(field.Type);

                if (field.Type.ArrayElementType != null
                    || _systemTypes.Any(x => x == field.Type.BaseType || field.Type.BaseType == _csharp.GetTypeOutput(new CodeTypeReference(x))))
                {
                    snippet.Text = $@"        [System.Xml.Serialization.XmlIgnoreAttribute]
        public {typeName} {propertyName}
        {{
            get
            {{
                return {fieldName};
            }}
            set
            {{
                {fieldName} = value;
            }}
        }}" + Environment.NewLine;
                }
                else
                {
                    snippet.Text = $@"        [System.Xml.Serialization.XmlIgnoreAttribute]
        public {typeName} {propertyName}
        {{
            get
            {{
                if ({fieldName} == null) {{ {fieldName} = new {typeName}(); }}
                return {fieldName};
            }}
            set
            {{
                {fieldName} = value;
            }}
        }}" + Environment.NewLine;
                }

                snippet.Comments.AddRange(field.Comments);
                field.Comments.Clear();

                field.CustomAttributes.Insert(0, new CodeAttributeDeclaration("System.ComponentModel.EditorBrowsableAttribute", new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(EditorBrowsableState)), "Never"))));

                snippet.UserData["PropertyName"] = propertyName;
                properties.Add(snippet);

                field.Name = fieldName;
            }

            type.Members.AddRange(properties.ToArray());
        }
    }
}