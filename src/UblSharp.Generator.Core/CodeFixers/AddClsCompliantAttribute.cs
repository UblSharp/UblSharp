using System.CodeDom;

namespace UblSharp.Generator.CodeFixers
{
    public class AddClsCompliantAttribute : CodeNamespaceVisitor
    {
        protected override void VisitType(CodeTypeDeclaration type)
        {
            type.CustomAttributes.Add(new CodeAttributeDeclaration("System.CLSCompliant", new CodeAttributeArgument(new CodePrimitiveExpression() { Value = true })));
        }
    }
}