using System;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace UblSharp.Generator.XsdFixers
{
    public static class UblCoreComponentsRenamer
    {
        public static void RenameCoreComponentTypes(XmlSchemaSet schemaSet)
        {
            var coreCompSchema = schemaSet.Schemas(Namespaces.Cct).OfType<XmlSchema>().Single();
            var unqualSchema = schemaSet.Schemas(Namespaces.Udt).OfType<XmlSchema>().Single();

            foreach (var complexType in coreCompSchema.Items.OfType<XmlSchemaComplexType>())
            {
                complexType.Name = "Cct" + complexType.Name;
                complexType.IsAbstract = true;
            }

            Action<XmlSchema, string, string> process = (schema, prefix, oldNs) =>
                {
                    foreach (var complexType in schema.Items
                        .OfType<XmlSchemaComplexType>()
                        .Where(t => t.BaseXmlSchemaType != null && t.BaseXmlSchemaType.QualifiedName.Namespace.Equals(oldNs)))
                    {
                        var name = new XmlQualifiedName(prefix + complexType.BaseXmlSchemaType.QualifiedName.Name, complexType.BaseXmlSchemaType.QualifiedName.Namespace);
                        var content = complexType.ContentModel as XmlSchemaSimpleContent;
                        if (content?.Content is XmlSchemaSimpleContentRestriction)
                        {
                            ((XmlSchemaSimpleContentRestriction)content.Content).BaseTypeName = name;
                        }
                        else if (content?.Content is XmlSchemaSimpleContentExtension)
                        {
                            ((XmlSchemaSimpleContentExtension)content.Content).BaseTypeName = name;
                        }
                    }
                };

            process(unqualSchema, "Cct", Namespaces.Cct);

            schemaSet.Reprocess(coreCompSchema);
            schemaSet.Reprocess(unqualSchema);

            RenameXmlDSigTypes(schemaSet);
        }

        private static void RenameXmlDSigTypes(XmlSchemaSet schemaSet)
        {
            var dsigSchemas = schemaSet.Schemas(Namespaces.Xmldsig).OfType<XmlSchema>()
                .ToList();
            if (dsigSchemas.Count == 0)
            {
                throw new InvalidOperationException("Cannot find Xmldsig schema");
            }

            foreach (var dsigSchema in dsigSchemas)
            {
                var types = new[] { "SignatureType" };

                foreach (var complexType in dsigSchema.Items.OfType<XmlSchemaComplexType>().Where(x => types.Contains(x.Name)))
                {
                    var complexName = complexType.Name;
                    complexType.Name = "Xml" + complexName;
                    var t = dsigSchema.Items.OfType<XmlSchemaElement>().Single(x => x.Name == complexName.Remove(complexName.Length - 5, 4));
                    t.SchemaTypeName = new XmlQualifiedName("Xml" + complexName, t.SchemaTypeName.Namespace);
                }

                schemaSet.Reprocess(dsigSchema);
            }

            //var sacSchema = schemaSet.Schemas(Namespaces.Sac).OfType<XmlSchema>().FirstOrDefault();
            //if (sacSchema != null)
            //{
            //    schemaSet.Reprocess(sacSchema);
            //}

            //var xadesSchema13 = schemaSet.Schemas(Namespaces.Xades132).OfType<XmlSchema>().FirstOrDefault();
            //if (xadesSchema13 != null)
            //{
            //    schemaSet.Reprocess(xadesSchema13);
            //}
        }
    }
}
