using System.CodeDom;
using System.Linq;

namespace UblSharp.Generator.CodeFixers
{
    public class RenameTypesWithSuffix : CodeNamespaceVisitor
    {
        private CodeNamespace _codeNamespace;

        protected override void VisitNamespace(CodeNamespace codeNamespace)
        {
            _codeNamespace = codeNamespace;
            base.VisitNamespace(codeNamespace);
        }

        protected override void VisitType(CodeTypeDeclaration type)
        {
            if (!type.Name.StartsWith("ItemsChoiceType") && (type.Name.EndsWith("1") || type.Name.EndsWith("2") || type.Name.EndsWith("3")))
            {
                var newName = type.Name.Substring(0, type.Name.Length - 1);
                var visitor = new RenameTypeWithSuffixOfProperties(type.Name, newName);
                type.Name = newName;

                visitor.Visit(_codeNamespace);
            }
        }

        public class RenameTypeWithSuffixOfProperties : CodeNamespaceVisitor
        {
            private readonly string _typeName;
            private readonly string _newName;

            public RenameTypeWithSuffixOfProperties(string typeName, string newName)
            {
                _typeName = typeName;
                _newName = newName;
            }

            protected override void VisitField(CodeMemberField member)
            {
                if (member.Type.BaseType == _typeName)
                {
                    member.Type.BaseType = _newName;
                }
            }

            protected override void VisitProperty(CodeMemberProperty member)
            {
                if (member.Type.BaseType == _typeName)
                {
                    member.Type.BaseType = _newName;
                }
            }
        }
    }
}
