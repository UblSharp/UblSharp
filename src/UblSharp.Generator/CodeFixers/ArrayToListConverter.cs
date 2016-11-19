using System.CodeDom;
using System.Linq;

namespace UblSharp.Generator.CodeFixers
{
    public class ArrayToListConverter : CodeNamespaceVisitor
    {
        private readonly string[] _typesToIgnore = { "System.Byte", "byte" };

        protected override void VisitField(CodeMemberField member)
        {
            if (member.Type.ArrayElementType == null || _typesToIgnore.Contains(member.Type.ArrayElementType.BaseType))
            {
                base.VisitField(member);
                return;
            }

            var elementType = member.Type.ArrayElementType;

            member.Type = new CodeTypeReference("System.Collections.Generic.List", elementType);

            base.VisitField(member);
        }
    }
}