using System.CodeDom;
using System.Linq;

namespace UblSharp.Generator.CodeFixers
{
    public class AddXmlMemberOrderToFields : CodeNamespaceVisitor
    {
        private CodeNamespace _codeNamespace;

        protected override void VisitNamespace(CodeNamespace codeNamespace)
        {
            _codeNamespace = codeNamespace;
            base.VisitNamespace(codeNamespace);
        }

        protected override void VisitType(CodeTypeDeclaration type)
        {
            var members = type.Members.OfType<CodeMemberField>().ToList();
            var order = 0;
            foreach (var field in members)
            {
                var attributes = field.CustomAttributes.Cast<CodeAttributeDeclaration>()
                    .Where(x =>
                        x.Name.EndsWith("XmlElementAttribute")
                        || x.Name.EndsWith("XmlArrayAttribute")
                        || x.Name.EndsWith("XmlAnyElementAttribute"));

                bool increaseOrder = false;
                foreach (var attr in attributes)
                {
                    if (_codeNamespace.Name.EndsWith("XmlDigitalSignature"))
                    {
                        var found = attr.Arguments.OfType<CodeAttributeArgument>().FirstOrDefault(x => x.Name == "Order");
                        if (found != null)
                        {
                            attr.Arguments.Remove(found);
                        }
                    }
                    else
                    {
                        attr.Arguments.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(order)));
                        increaseOrder = true;
                    }
                }

                if (increaseOrder)
                {
                    order++;
                }
            }
        }
    }
}
