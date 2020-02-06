using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using UblSharp.Generator.Extensions;

namespace UblSharp.Generator.XsdFixers
{
    public static class UblCommonBasicComponentFixer
    {
        public static void FlattenCommonBasicComponents(XmlSchemaSet schemaSet)
        {
            var replaced = new List<Replacement>();
            foreach (var xmlSchema in schemaSet.Schemas().OfType<XmlSchema>())
            {
                // Fix Common basic components 'inheritance'
                var baseTypes = xmlSchema.Items.OfType<XmlSchemaComplexType>()
                    .Where(x => (x.ContentModel?.Content as XmlSchemaSimpleContentExtension)?.BaseTypeName.Namespace == Namespaces.Udt)
                    .ToList();

                var baseTypesMap = baseTypes
                    .ToDictionary(x => x.QualifiedName);

                var itemsToReplace = xmlSchema.Items.OfType<XmlSchemaElement>()
                    .Where(x => baseTypesMap.ContainsKey(x.SchemaTypeName))
                    .ToList();
                itemsToReplace.ForEach(x =>
                {
                    var removedType = baseTypesMap[x.SchemaTypeName];
                    var newTypeName = ((XmlSchemaSimpleContentExtension)baseTypesMap[x.SchemaTypeName].ContentModel.Content).BaseTypeName;
                    
                    replaced.Add(new Replacement(xmlSchema, removedType, x, x.SchemaTypeName, newTypeName));
                });
            }

            // Replace usages of "type='cbc:xxxType'" to their new name (udt:xxxType)
            //
            // Note:
            // In the core UBL documents this situation does not happen, since each Cbc type is defined as an xsd:element
            // and used in main documents using xsd:element 'ref'.

            // In SCSN_OrderStatus-v1.0.xsd an xsd:element is used which references a cbc type using 'type=csb:...' instead of defining
            // their own xsd:element and use 'ref'.

            // Because of this, we need to walk the xsd recursively, replacing all usages of 'cbc:xxxType' with 'udt:xxxType'
            // Note that many cases are probably missing (I don't know the entire xsd schema spec and how to properly walk it)
            // For now, this has been tested with the SCSN_OrderStatus-v1.0.xsd
            foreach (XmlSchema xmlSchema in schemaSet.Schemas())
            {
                if (!xmlSchema.IsMaindocSchema())
                {
                    continue;
                }

                var walker = new ReplaceElementTypes(replaced);
                walker.Visit(xmlSchema);
                if (walker.IsChanged)
                {
                    schemaSet.Reprocess(xmlSchema);
                }
            }

            foreach (Replacement replacement in replaced)
            {
                replacement.XmlSchema.Items.Remove(replacement.RemovedType);
                replacement.ElementToReplace.SchemaTypeName = replacement.NewTypeName;
            }

            foreach (XmlSchema xmlSchema in schemaSet.Schemas())
            {
                schemaSet.Reprocess(xmlSchema);
            }
        }

        private class Replacement
        {
            public XmlSchema XmlSchema { get; }
            public XmlSchemaComplexType RemovedType { get; }
            public XmlSchemaElement ElementToReplace { get; }
            public XmlQualifiedName OldTypeName { get; }
            public XmlQualifiedName NewTypeName { get; }

            public Replacement(XmlSchema xmlSchema, XmlSchemaComplexType removedType, XmlSchemaElement elementToReplace, XmlQualifiedName oldTypeName, XmlQualifiedName newTypeName)
            {
                XmlSchema = xmlSchema;
                RemovedType = removedType;
                ElementToReplace = elementToReplace;
                OldTypeName = oldTypeName;
                NewTypeName = newTypeName;
            }
        }

        private class ReplaceElementTypes : XmlSchemaWalker
        {
            private readonly IDictionary<XmlQualifiedName, XmlQualifiedName> _replacements;

            public ReplaceElementTypes(List<Replacement> replacements)
            {
                _replacements = replacements.ToDictionary(x => x.OldTypeName, x => x.NewTypeName);
            }

            protected override void VisitElement(XmlSchemaElement element)
            {
                if (_replacements.TryGetValue(element.SchemaTypeName, out var newTypeName))
                {
                    if (element.SchemaTypeName != newTypeName)
                    {
                        element.SchemaTypeName = newTypeName;
                        IsChanged = true;
                    }
                }

                base.VisitElement(element);
            }

            public bool IsChanged { get; private set; }
        }
    }
}
