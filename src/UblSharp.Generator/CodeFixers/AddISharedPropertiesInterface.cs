using System.CodeDom;
using UblSharp.Generator.Extensions;

namespace UblSharp.Generator.CodeFixers
{
    public class AddISharedPropertiesInterface : CodeNamespaceVisitor
    {
        protected override void VisitClass(CodeTypeDeclaration type)
        {
            if (type.GetSchema().SourceUri.Contains("maindoc")
                // && type.Name != "BaseDocument"
                )
            {
                type.BaseTypes.Add(new CodeTypeReference("IBaseDocument"));
            }
        }
    }
}