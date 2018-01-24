using System.CodeDom;
using System.Linq;

namespace UblSharp.Generator.CodeFixers
{
    public class RemoveConflictingTypes : CodeNamespaceVisitor
    {
        protected override void VisitNamespace(CodeNamespace codeNamespace)
        {
            var types = codeNamespace.Types.OfType<CodeTypeDeclaration>().Where(
                    x =>
                        x.Name == "UBLExtensionsType"
                        || x.Name == "TransformsType")
                .ToList();

            if (types.Any())
            {
                foreach (var type in types)
                {
                    codeNamespace.Types.Remove(type);
                }
            }
        }
    }
}
