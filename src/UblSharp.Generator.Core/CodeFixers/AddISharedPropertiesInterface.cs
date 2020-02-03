using System.CodeDom;
using System.Linq;
using System.Xml.Schema;
using UblSharp.Generator.Extensions;

namespace UblSharp.Generator.CodeFixers
{
    public class AddISharedPropertiesInterface : CodeNamespaceVisitor
    {
        protected override void VisitClass(CodeTypeDeclaration type)
        {
            if (IsUblDocument(type) || type.GetSchema().TargetNamespace == "urn:oasis:names:specification:ubl:schema:xsd:BaseDocument-2")
            {
                type.BaseTypes.Add(new CodeTypeReference("IBaseDocument"));
            }
        }

        private bool IsUblDocument(CodeTypeDeclaration type)
        {
            var schema = type.GetSchema();
            if (!schema.IsMaindocSchema())
            {
                return false;
            }

            var hasSchemaElement = schema.Elements.Values.OfType<XmlSchemaElement>()
                .Any(x => x.SchemaTypeName.Name == type.Name);
            return hasSchemaElement;
        }
    }
}
