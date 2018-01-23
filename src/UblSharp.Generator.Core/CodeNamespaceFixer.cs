using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace UblSharp.Generator
{
    public class CodeNamespaceFixer
    {
        public IEnumerable<CodeNamespaceVisitor> Visitors { get; }

        public CodeNamespaceFixer(params CodeNamespaceVisitor[] visitors)
        {
            Visitors = visitors ?? Enumerable.Empty<CodeNamespaceVisitor>();
        }

        public void Fix(CodeNamespace codeNamespace)
        {
            foreach (var walker in Visitors)
            {
                walker.Visit(codeNamespace);
            }
        }
    }
}