using System;
using System.CodeDom;
using System.Linq;
using UblSharp.Generator.Extensions;

namespace UblSharp.Generator.CodeFixers
{
    public class ArrayToListConverter : CodeNamespaceVisitor
    {
        private readonly string[] _typesToIgnore = { "System.Byte", "byte" };

        protected override void VisitType(CodeTypeDeclaration type)
        {
            var members = type.Members.OfType<CodeMemberField>().ToList();
            var order = 0;
            foreach (var field in members)
            {
                var attr = field.CustomAttributes.Cast<CodeAttributeDeclaration>()
                    .FirstOrDefault(x =>
                        x.Name.EndsWith("XmlElementAttribute")
                        || x.Name.EndsWith("XmlArrayAttribute"));
                if (attr != null)
                {
                    attr.Arguments.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(order)));
                    order++;
                }
            }

            var arrayMembers = type.Members.OfType<CodeMemberField>().Where(x => x.Type.ArrayElementType != null && !_typesToIgnore.Contains(x.Type.ArrayElementType.BaseType)).ToList();
            foreach (var field in arrayMembers)
            {
                var prop = new CodeMemberProperty()
                {
                    Type = field.Type,
                    Name = "__" + field.Name,
                    Attributes = field.Attributes,
                    CustomAttributes = field.CustomAttributes
                };
                prop.GetStatements.Add(new CodeSnippetExpression()
                {
                    Value = $@"return {field.Name}?.ToArray()"
                });
                prop.SetStatements.Add(new CodeSnippetExpression()
                {
                    Value = $@"{field.Name} = value == null ? null : new System.Collections.Generic.List<{field.Type.ArrayElementType.BaseType}>(value)"
                });

                string interfaceDecl = "";
                string visibility = "public ";
                if (type.IsMaindocSchema() && new[] { "UBLExtensions", "Signature" }.Contains(field.Name))
                {
                    interfaceDecl = "IBaseDocument.";
                    visibility = "";
                }

                var snip = new CodeSnippetTypeMember();
                snip.Text = $@"        [System.Xml.Serialization.XmlIgnoreAttribute]
        {visibility}System.Collections.Generic.List<{field.Type.ArrayElementType.BaseType}> {interfaceDecl}{field.Name} {{ get; set; }}" + Environment.NewLine;

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

                var index = type.Members.IndexOf(field);
                type.Members.RemoveAt(index);
                type.Members.Insert(index, prop);
                // type.Members.Add(propPub);
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