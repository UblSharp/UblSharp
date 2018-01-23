using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace UblSharp.Generator.CodeFixers
{

    public class RenameXadesIntPropertyFixer : CodeNamespaceVisitor
    {
        protected override void VisitClass(CodeTypeDeclaration type)
        {
            if (type.Name == "IntegerListType")
            {
                var member = type.Members.OfType<CodeTypeMember>().Single(x => x.Name == "int");
                member.Name = "Int";
            }
        }
    }
}
