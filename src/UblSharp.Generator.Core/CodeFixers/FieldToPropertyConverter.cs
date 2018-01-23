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

        private readonly string[] _baseDocumentProperties =
            {
                "UBLExtensions",
                "UBLVersionID",
                "CustomizationID",
                "ProfileID",
                "ProfileExecutionID",
                "ID",
                "UUID",
                "Signature",
            };

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

        // 0 = fieldName, 1 = propertyName, 2 = propertyType, 3 = baseInterfaceType, 4 = propertyVisibility
        private string _emptyPropertyTemplate = @"        [System.Xml.Serialization.XmlIgnoreAttribute]
        {4}{2} {3}{1}
        {{
            get
            {{
                return {0};
            }}
            set
            {{
                {0} = value;
            }}
        }}" + Environment.NewLine;

        private string _defaultPropertyTemplate = @"        [System.Xml.Serialization.XmlIgnoreAttribute]
        {4}{2} {3}{1}
        {{
            get
            {{
                if ({0} == null) {{ {0} = new {2}(); }}
                return {0};
            }}
            set
            {{
                {0} = value;
            }}
        }}" + Environment.NewLine;

        private string _enumPropertyTemplate = @"        [System.Xml.Serialization.XmlIgnoreAttribute]
        {4}{2} {3}{1}
        {{
            get
            {{                
                return {0};
            }}
            set
            {{
                {0} = value;
            }}
        }}" + Environment.NewLine;

        private CodeNamespace _currentNamespace;

        protected override void VisitNamespace(CodeNamespace codeNamespace)
        {
            _currentNamespace = codeNamespace;
            base.VisitNamespace(codeNamespace);
        }

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
            foreach (var field in type.Members.OfType<CodeMemberField>().Where(f => !f.Attributes.HasFlag(MemberAttributes.Private)).ToList())
            {
                var propertyName = field.Name;
                var fieldName = field.Name.MakePrivatePropertyName();
                var snippet = new CodeSnippetTypeMember();
                var typeName = _csharp.GetTypeOutput(field.Type);

                var propertyVisibility = "public ";
                var baseInterfaceType = "";
                if (type.IsMaindocSchema() && _baseDocumentProperties.Contains(propertyName))
                {
                    baseInterfaceType = "IBaseDocument.";
                    propertyVisibility = "";
                }

                string propertyTemplate = _defaultPropertyTemplate;

                if (field.Type.ArrayElementType != null
                    || _systemTypes.Any(x => x == field.Type.BaseType || field.Type.BaseType == _csharp.GetTypeOutput(new CodeTypeReference(x))))
                {
                    propertyTemplate = _emptyPropertyTemplate;
                }

                // try best to detect enums
                var propertyType = _currentNamespace.Types.Cast<CodeTypeDeclaration>().FirstOrDefault(x => x.Name == field.Type.BaseType);
                if (propertyType != null && propertyType.IsEnum)
                {
                    propertyTemplate = _enumPropertyTemplate;
                }

                snippet.Text = string.Format(propertyTemplate, fieldName, propertyName, typeName, baseInterfaceType, propertyVisibility);

                snippet.Comments.AddRange(field.Comments);
                snippet.UserData["PropertyName"] = propertyName;
                properties.Add(snippet);

                field.Comments.Clear();
                field.CustomAttributes.Insert(0, new CodeAttributeDeclaration("System.ComponentModel.EditorBrowsableAttribute", new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(EditorBrowsableState)), "Never"))));
                field.Name = fieldName;

                //if (type.Name == "BaseDocument" || !_baseDocumentProperties.Contains(propertyName))
                //{
                //    if (field.Type.ArrayElementType != null
                //        || _systemTypes.Any(x => x == field.Type.BaseType || field.Type.BaseType == _csharp.GetTypeOutput(new CodeTypeReference(x))))
                //    {
                //        snippet.Text = string.Format(_emptyPropertyTemplate, fieldName, propertyName, typeName);
                //    }
                //    else
                //    {
                //        snippet.Text = string.Format(_defaultPropertyTemplate, fieldName, propertyName, typeName);
                //    }

                //    snippet.Comments.AddRange(field.Comments);
                //    snippet.UserData["PropertyName"] = propertyName;
                //    properties.Add(snippet);
                //}

                //if (type.Name == "BaseDocument")
                //{
                //    type.Members.Remove(field);
                //}
                //else
                //{
                //    field.Comments.Clear();
                //    field.CustomAttributes.Insert(0, new CodeAttributeDeclaration("System.ComponentModel.EditorBrowsableAttribute", new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(EditorBrowsableState)), "Never"))));
                //    field.Name = fieldName;
                //}
            }

            type.Members.AddRange(properties.ToArray());
        }
    }
}
