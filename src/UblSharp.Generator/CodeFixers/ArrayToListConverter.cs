using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using UblSharp.Generator.Extensions;

namespace UblSharp.Generator.CodeFixers
{
    public class ArrayToListConverter : CodeNamespaceVisitor
    {
        private readonly string[] _typesToIgnore = { "System.Byte", "byte" };

        protected override void VisitType(CodeTypeDeclaration type)
        {
            var arrayMembers = type.Members.OfType<CodeMemberField>().Where(x => !x.Name.StartsWith("__") && x.Type.ArrayElementType != null && !_typesToIgnore.Contains(x.Type.ArrayElementType.BaseType)).ToList();
            foreach (var field in arrayMembers)
            {
                var prop = new CodeMemberProperty()
                {
                    Type = field.Type,
                    Name = "__" + field.Name,
                    Attributes = field.Attributes
                };

                prop.CustomAttributes.AddRange(field.CustomAttributes);

                var fieldName = field.Name.MakePrivateFieldName();
                prop.GetStatements.Add(new CodeSnippetExpression()
                {
                    Value = $@"return {fieldName}?.ToArray()"
                });
                prop.SetStatements.Add(new CodeSnippetExpression()
                {
                    Value = $@"{fieldName} = value == null ? null : new System.Collections.Generic.List<{field.Type.ArrayElementType.BaseType}>(value)"
                });

                var interfaceDecl = "";
                var visibility = "public ";
                if (type.IsMaindocSchema() && new[] { "UBLExtensions", "Signature" }.Contains(field.Name))
                {
                    interfaceDecl = "IBaseDocument.";
                    visibility = "";
                }

                var snip = new CodeSnippetTypeMember();
                snip.Text = $@"        [System.Xml.Serialization.XmlIgnoreAttribute]
        {visibility}System.Collections.Generic.List<{field.Type.ArrayElementType.BaseType}> {interfaceDecl}{field.Name}
        {{
            get {{ return {fieldName} ?? ({fieldName} = new System.Collections.Generic.List<{field.Type.ArrayElementType.BaseType}>()); }}
            set {{ {fieldName} = value; }}
        }}" + Environment.NewLine;

                snip.Comments.AddRange(field.Comments);

                //var propPub = new CodeMemberProperty()
                //{
                //    Type = field.Type,
                //    Name = field.Name,
                //    Attributes = field.Attributes,
                //    CustomAttributes = field.CustomAttributes
                //};
                //propPub.Comments.AddRange(field.Comments);

                //propPub.GetStatements.Add(new CodeSnippetExpression()
                //{
                //    Value = $@"return __{field.Name}"
                //});
                //propPub.SetStatements.Add(new CodeSnippetExpression()
                //{
                //    Value = $@"__{field.Name} = value"
                //});
                field.Name = fieldName;
                field.Attributes = MemberAttributes.Private;
                field.Type = new CodeTypeReference($"System.Collections.Generic.List<{field.Type.ArrayElementType.BaseType}>");
                field.CustomAttributes.Clear();
                field.Comments.Clear();

                // Fix items choice types
                var attr = prop.CustomAttributes.Cast<CodeAttributeDeclaration>().FirstOrDefault(x => x.Name.EndsWith("XmlChoiceIdentifierAttribute"));
                if (attr != null)
                {
                    var expr = (CodePrimitiveExpression)attr.Arguments[0].Value;
                    expr.Value = "__" + expr.Value;
                }

                var index = type.Members.IndexOf(field);
                type.Members.Insert(index + 1, prop);
                type.Members.Add(snip);
            }

            // base.VisitType(type);
        }

        protected override void VisitField(CodeMemberField member)
        {
            //if (member.Type.ArrayElementType == null || _typesToIgnore.Contains(member.Type.ArrayElementType.BaseType))
            //{
            //    base.VisitField(member);
            //    return;
            //}

            //var elementType = member.Type.ArrayElementType;

            //member.Type = new CodeTypeReference("System.Collections.Generic.List", elementType);

            // base.VisitField(member);
        }
    }
}