using System;
using System.CodeDom;

namespace UblSharp.Generator
{
    public abstract class CodeNamespaceVisitor
    {
        public void Visit(CodeNamespace codeNamespace)
        {
            if (codeNamespace == null)
            {
                throw new ArgumentNullException(nameof(codeNamespace));
            }

            VisitNamespace(codeNamespace);
        }

        protected virtual void VisitNamespace(CodeNamespace codeNamespace)
        {
            foreach (CodeTypeDeclaration type in codeNamespace.Types)
            {
                VisitType(type);
            }
        }

        protected virtual void VisitType(CodeTypeDeclaration type)
        {
            if (type.IsClass)
            {
                VisitClass(type);
            }
            else if (type.IsEnum)
            {
                VisitEnum(type);
            }
            else if (type.IsInterface)
            {
                VisitInterface(type);
            }
        }

        protected virtual void VisitClass(CodeTypeDeclaration type)
        {
            foreach (CodeTypeMember member in type.Members)
            {
                VisitMember(member);
            }
        }

        protected virtual void VisitEnum(CodeTypeDeclaration type)
        {
            foreach (CodeTypeMember member in type.Members)
            {
                VisitMember(member);
            }
        }

        protected virtual void VisitInterface(CodeTypeDeclaration type)
        {
            foreach (CodeTypeMember member in type.Members)
            {
                VisitMember(member);
            }
        }

        protected virtual void VisitMember(CodeTypeMember member)
        {
            if (member is CodeMemberField)
            {
                VisitField((CodeMemberField)member);
            }
            else if (member is CodeMemberProperty)
            {
                VisitProperty((CodeMemberProperty)member);
            }
            else if (member is CodeMemberEvent)
            {
                VisitEvent((CodeMemberEvent)member);
            }
            else if (member is CodeMemberMethod)
            {
                VisitMethod((CodeMemberMethod)member);
            }
            else if (member is CodeSnippetTypeMember)
            {
                VisitSnippet((CodeSnippetTypeMember)member);
            }
        }

        protected virtual void VisitProperty(CodeMemberProperty member)
        {
        }

        protected virtual void VisitField(CodeMemberField member)
        {

        }

        protected virtual void VisitEvent(CodeMemberEvent member)
        {

        }

        protected virtual void VisitMethod(CodeMemberMethod member)
        {

        }

        protected virtual void VisitSnippet(CodeSnippetTypeMember member)
        {

        }
    }
}