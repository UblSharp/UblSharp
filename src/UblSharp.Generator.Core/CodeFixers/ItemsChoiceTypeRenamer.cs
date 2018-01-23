using System.CodeDom;

namespace UblSharp.Generator.CodeFixers
{
    public class ItemsChoiceTypeRenamer : CodeNamespaceVisitor
    {
        private CodeTypeDeclaration _currentClass;
        private CodeNamespace _currentNs;

        protected override void VisitNamespace(CodeNamespace codeNamespace)
        {
            _currentNs = codeNamespace;

            base.VisitNamespace(codeNamespace);
        }

        protected override void VisitClass(CodeTypeDeclaration type)
        {
            _currentClass = type;

            base.VisitClass(type);
        }

        protected override void VisitField(CodeMemberField member)
        {
            if (member.Name == "ItemsElementName" && member.Type.ArrayRank == 1)
            {
                var oldName = member.Type.BaseType;
                var newName = $"{_currentClass.Name}Item";
                member.Type = new CodeTypeReference(newName, 1);

                new RenameEnum(oldName, newName).Visit(_currentNs);
            }
        }

        private class RenameEnum: CodeNamespaceVisitor
        {
            private readonly string _oldName;
            private readonly string _newName;

            public RenameEnum(string oldName, string newName)
            {
                _oldName = oldName;
                _newName = newName;
            }

            protected override void VisitEnum(CodeTypeDeclaration type)
            {
                if (type.Name == _oldName)
                {
                    type.Name = _newName;
                }
            }
            
            protected override void VisitClass(CodeTypeDeclaration type)
            {
                if (type.Name == _oldName)
                {
                    type.Name = _newName;
                }
            }
        }
    }
}