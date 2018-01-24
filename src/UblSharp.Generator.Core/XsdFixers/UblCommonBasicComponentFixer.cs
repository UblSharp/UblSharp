using System.Linq;
using System.Xml.Schema;

namespace UblSharp.Generator.XsdFixers
{
    public static class UblCommonBasicComponentFixer
    {
        public static void FlattenCommonBasicComponents(XmlSchemaSet schemaSet)
        {
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
                    xmlSchema.Items.Remove(baseTypesMap[x.SchemaTypeName]);

                    x.SchemaTypeName = ((XmlSchemaSimpleContentExtension)baseTypesMap[x.SchemaTypeName].ContentModel.Content).BaseTypeName;
                    //cbcSchema.Items.Remove(x);
                    //cbcSchema.Items.Add(new XmlSchemaElement
                    //{
                    //    Name = x.Name,
                    //    SchemaTypeName = baseTypesMap[x.SchemaTypeName]
                    //});
                });

                schemaSet.Reprocess(xmlSchema);
            }
        }
    }
}
