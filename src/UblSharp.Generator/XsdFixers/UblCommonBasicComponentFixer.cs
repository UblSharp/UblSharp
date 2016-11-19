using System.Linq;
using System.Xml.Schema;

namespace UblSharp.Generator.XsdFixers
{
    public static class UblCommonBasicComponentFixer
    {
        public static void FlattenCommonBasicComponents(XmlSchemaSet schemaSet)
        {
            // Fix Common basic components 'inheritance'
            var cbcSchema = schemaSet.Schemas(Namespaces.Cbc).Cast<XmlSchema>().First();
            var baseTypes = cbcSchema.Items.OfType<XmlSchemaComplexType>()
                .Where(x => x.ContentModel?.Content is XmlSchemaSimpleContentExtension)
                .ToList();
            var baseTypesMap = baseTypes
                .ToDictionary(x => x.QualifiedName, x => ((XmlSchemaSimpleContentExtension)x.ContentModel.Content).BaseTypeName);

            baseTypes.ForEach(x => cbcSchema.Items.Remove(x));

            var itemsToReplace = cbcSchema.Items.OfType<XmlSchemaElement>()
                .Where(x => baseTypesMap.ContainsKey(x.SchemaTypeName))
                .ToList();
            itemsToReplace.ForEach(x =>
                {
                    x.SchemaTypeName = baseTypesMap[x.SchemaTypeName];
                    //cbcSchema.Items.Remove(x);
                    //cbcSchema.Items.Add(new XmlSchemaElement
                    //{
                    //    Name = x.Name,
                    //    SchemaTypeName = baseTypesMap[x.SchemaTypeName]
                    //});
                });

            schemaSet.Reprocess(cbcSchema);

            //// Fix Common extension components
            //var cecSchema = schemaSet.Schemas(Namespaces.Cec).Cast<XmlSchema>().First();
            //baseTypes = cecSchema.Items.OfType<XmlSchemaComplexType>()
            //    .Where(x => x.ContentModel?.Content is XmlSchemaSimpleContentExtension)
            //    .ToList();
            //baseTypesMap = baseTypes
            //    .ToDictionary(x => x.QualifiedName, x => ((XmlSchemaSimpleContentExtension)x.ContentModel.Content).BaseTypeName);

            //baseTypes.ForEach(x => cecSchema.Items.Remove(x));

            //itemsToReplace = cecSchema.Items.OfType<XmlSchemaElement>()
            //    .Where(x => baseTypesMap.ContainsKey(x.SchemaTypeName))
            //    .ToList();
            //itemsToReplace.ForEach(x =>
            //{
            //    x.SchemaTypeName = baseTypesMap[x.SchemaTypeName];
            //    //cbcSchema.Items.Remove(x);
            //    //cbcSchema.Items.Add(new XmlSchemaElement
            //    //{
            //    //    Name = x.Name,
            //    //    SchemaTypeName = baseTypesMap[x.SchemaTypeName]
            //    //});
            //});

            //schemaSet.Reprocess(cecSchema);
        }
    }
}