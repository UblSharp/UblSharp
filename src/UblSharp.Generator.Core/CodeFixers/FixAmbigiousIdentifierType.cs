using System.CodeDom;
using System.Linq;
using UblSharp.Generator.Extensions;

namespace UblSharp.Generator.CodeFixers
{
    public class FixAmbigiousIdentifierType : CodeNamespaceVisitor
    {
        protected override void VisitClass(CodeTypeDeclaration type)
        {
            if (type.GetSchema().TargetNamespace == Namespaces.Sac)
            {
                var props = type.Members.OfType<CodeMemberProperty>().Where(x => x.Type.BaseType == "IdentifierType");
                foreach (var prop in props)
                {
                    prop.Type = new CodeTypeReference("UnqualifiedDataTypes.IdentifierType");
                }

                var fields = type.Members.OfType<CodeMemberField>().Where(x => x.Type.BaseType == "IdentifierType");
                foreach (var field in fields)
                {
                    field.Type = new CodeTypeReference("UnqualifiedDataTypes.IdentifierType");
                }
            }
            if (type.GetSchema().TargetNamespace == Namespaces.Xades132)
            {
                var props = type.Members.OfType<CodeMemberProperty>().Where(x => x.Type.BaseType == "IdentifierType");
                foreach (var prop in props)
                {
                    prop.Type = new CodeTypeReference("Xades.IdentifierType");
                }

                var fields = type.Members.OfType<CodeMemberField>().Where(x => x.Type.BaseType == "IdentifierType");
                foreach (var field in fields)
                {
                    field.Type = new CodeTypeReference("Xades.IdentifierType");
                }
            }
        }
    }
}
