using System;
using System.Linq;
using System.Xml.Schema;

namespace UblSharp.Generator.XsdFixers
{
    public static class UblCommonBasicComponentFixer
    {
        public static void FlattenCommonBasicComponents(XmlSchemaSet schemaSet)
        {
            var qdtSchema = schemaSet.Schemas(Namespaces.Qdt).OfType<XmlSchema>().Single();
            var qdtTypes = qdtSchema.Items.OfType<XmlSchemaComplexType>()
                .Select(
                    x => new
                    {
                        ComplexType = x,
                        BaseType = (x.ContentModel?.Content as XmlSchemaSimpleContentRestriction)?.BaseTypeName
                    })
                .ToList();

            foreach (var xmlSchema in schemaSet.Schemas().OfType<XmlSchema>())
            {
                var qdtBaseTypes = xmlSchema.Items.OfType<XmlSchemaComplexType>()
                    .Where(
                        x => (x.ContentModel?.Content as XmlSchemaSimpleContentExtension)?.BaseTypeName.Namespace == Namespaces.Qdt ||
                             (x.ContentModel?.Content as XmlSchemaSimpleContentRestriction)?.BaseTypeName.Namespace == Namespaces.Qdt)
                    .ToList();

                var qdtBaseTypesMap = qdtBaseTypes
                    .ToDictionary(x => x.QualifiedName);

                var qdtItemsToReplace = xmlSchema.Items.OfType<XmlSchemaElement>()
                    .Where(x => qdtBaseTypesMap.ContainsKey(x.SchemaTypeName))
                    .ToList();

                qdtItemsToReplace.ForEach(
                    x =>
                    {
                        var ct = qdtBaseTypesMap[x.SchemaTypeName];
                        var baseName = (ct.ContentModel.Content as XmlSchemaSimpleContentExtension)?.BaseTypeName ??
                                       (ct.ContentModel.Content as XmlSchemaSimpleContentRestriction)?.BaseTypeName;

                        var udtType = qdtTypes.FirstOrDefault(t => t.ComplexType.Name == baseName.Name);
                        if (udtType == null)
                        {
                            throw new InvalidOperationException();
                        }

                        xmlSchema.Items.Remove(ct);
                        x.SchemaTypeName = udtType.BaseType;
                    });

                schemaSet.Reprocess(xmlSchema);

                // Fix Common basic components 'inheritance'
                var baseTypes = xmlSchema.Items.OfType<XmlSchemaComplexType>()
                    .Where(
                        x => (x.ContentModel?.Content as XmlSchemaSimpleContentExtension)?.BaseTypeName.Namespace == Namespaces.Udt ||
                             (x.ContentModel?.Content as XmlSchemaSimpleContentRestriction)?.BaseTypeName.Namespace == Namespaces.Udt)
                    .ToList();

                var baseTypesMap = baseTypes
                    .ToDictionary(x => x.QualifiedName);

                var itemsToReplace = xmlSchema.Items.OfType<XmlSchemaElement>()
                    .Where(x => baseTypesMap.ContainsKey(x.SchemaTypeName))
                    .ToList();
                itemsToReplace.ForEach(
                    x =>
                    {
                        xmlSchema.Items.Remove(baseTypesMap[x.SchemaTypeName]);

                        x.SchemaTypeName = (baseTypesMap[x.SchemaTypeName].ContentModel.Content as XmlSchemaSimpleContentExtension)?.BaseTypeName ??
                                           (baseTypesMap[x.SchemaTypeName].ContentModel.Content as XmlSchemaSimpleContentRestriction)?.BaseTypeName;
                    });

                schemaSet.Reprocess(xmlSchema);
            }

            qdtSchema.Items.OfType<XmlSchemaComplexType>().ToList().ForEach(x => qdtSchema.Items.Remove(x));
            schemaSet.Reprocess(qdtSchema);
        }
    }
}
