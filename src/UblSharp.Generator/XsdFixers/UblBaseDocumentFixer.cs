using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace UblSharp.Generator.XsdFixers
{
    public class UblBaseDocumentFixer
    {
        public const string AbstractBaseSchemaComplexTypeName = "BaseDocument";
        public const string AbstractBaseSchemaElementName = "BaseDocument";
        public const string AbstractBaseSchemaName = "BaseDocument";

        public static void FixBaseDocumentInheritance(XmlSchemaSet schemaSet)
        {
            var schemas = schemaSet.Schemas().Cast<XmlSchema>().ToList();
            var maindocSchemas = schemas.Where(x => x.SourceUri.Contains("maindoc")).ToList();
            var baseDocSchema = schemas.Single(x => x.SourceUri.Contains("BaseDocument"));

            var elementsToRemove = (baseDocSchema.Items.OfType<XmlSchemaComplexType>().Single().ContentTypeParticle as XmlSchemaSequence)
                                       ?.Items.Cast<XmlSchemaElement>()
                                       .ToLookup(x => x.QualifiedName.Name) ?? new XmlSchemaElement[0].ToLookup(x => "");

            var baseDocSchemaImport = new XmlSchemaImport { Namespace = baseDocSchema.TargetNamespace, Schema = baseDocSchema };

            foreach (var maindocSchema in maindocSchemas)
            {
                if (maindocSchema.SourceUri.Contains("BaseDocument"))
                {
                    continue;
                }

                maindocSchema.Namespaces.Add("abs", baseDocSchema.TargetNamespace);
                maindocSchema.Includes.Add(baseDocSchemaImport);

                var maindocSchemaComplexType = maindocSchema.Items.OfType<XmlSchemaComplexType>().Single();
                var sequence = (XmlSchemaSequence)maindocSchemaComplexType.Particle;
                var removed = 0;
                for (var i = sequence.Items.Count - 1; i >= 0; i--)
                {
                    var el = (XmlSchemaElement)sequence.Items[i];
                    if (elementsToRemove.Contains(el.QualifiedName.Name))
                    {
                        sequence.Items.RemoveAt(i);
                        removed++;
                    }
                }

                if (removed != elementsToRemove.Count)
                {
                    throw new InvalidOperationException("Invalid base document, not all maindocs have all the base document properties.");
                }

                maindocSchemaComplexType.ContentModel = new XmlSchemaComplexContent()
                {
                    Content = new XmlSchemaComplexContentExtension
                    {
                        BaseTypeName = new XmlQualifiedName(baseDocSchema.Items.OfType<XmlSchemaComplexType>().Single().Name, baseDocSchema.TargetNamespace),
                        Particle = sequence
                    }
                };
            }

            maindocSchemas.ForEach(s => schemaSet.Reprocess(s));
        }

        public static void ModifyMaindocSchemasForInheritance(XmlSchemaSet schemaSet)
        {
            var maindocSchemas = schemaSet.Schemas().Cast<XmlSchema>().Where(x => x.SourceUri.Contains("maindoc")).ToList();

            var sharedElementCount = GetSharedElementCount(maindocSchemas);
            if (0 == sharedElementCount)
            {
                throw new InvalidOperationException("Could not find any shared elements.");
            }

            var templateSchema = maindocSchemas.First();
            var abstractBaseSchema = CreateAbstractBaseSchemaFromMaindocSchema(templateSchema, sharedElementCount);

            var abstactBaseSchemaImport = new XmlSchemaImport { Namespace = abstractBaseSchema.TargetNamespace, Schema = abstractBaseSchema };
            var abstactBaseSchemaQNameToInheritFrom = new XmlQualifiedName(AbstractBaseSchemaComplexTypeName, abstractBaseSchema.TargetNamespace);

            foreach (var maindocSchema in maindocSchemas)
            {
                maindocSchema.Namespaces.Add("abs", abstractBaseSchema.TargetNamespace);
                maindocSchema.Includes.Add(abstactBaseSchemaImport);

                var maindocSchemaComplexType = maindocSchema.Items.OfType<XmlSchemaComplexType>().Single();
                var nonSharedElementSequence = maindocSchemaComplexType.Particle as XmlSchemaSequence;
                if (nonSharedElementSequence != null)
                {
                    for (var i = 0; i < sharedElementCount; i++)
                    {
                        nonSharedElementSequence.Items.RemoveAt(0);
                    }
                }

                maindocSchemaComplexType.ContentModel = new XmlSchemaComplexContent
                {
                    Content = new XmlSchemaComplexContentExtension
                    {
                        BaseTypeName = abstactBaseSchemaQNameToInheritFrom,
                        Particle = nonSharedElementSequence
                    }
                };
            }

            schemaSet.Add(abstractBaseSchema);
            maindocSchemas.ForEach(s => schemaSet.Reprocess(s));
        }

        private static XmlSchema CreateAbstractBaseSchemaFromMaindocSchema(XmlSchema templateSchema, int sharedElementCount)
        {
            var abstractBaseSchema = DeepCopy(templateSchema);
            var abstractBaseElement = abstractBaseSchema.Items.OfType<XmlSchemaElement>().Single();
            var abstractBaseComplexType = abstractBaseSchema.Items.OfType<XmlSchemaComplexType>().Single();

            abstractBaseSchema.TargetNamespace = abstractBaseSchema.TargetNamespace.Replace(abstractBaseElement.Name, AbstractBaseSchemaName);
            abstractBaseSchema.Namespaces.Add("", abstractBaseSchema.TargetNamespace);
            abstractBaseSchema.SourceUri = templateSchema.SourceUri.Replace(abstractBaseElement.Name, AbstractBaseSchemaName);

            abstractBaseComplexType.IsAbstract = true;
            abstractBaseComplexType.Annotation.Items.Clear();
            abstractBaseComplexType.Name = AbstractBaseSchemaComplexTypeName;

            var elementCollection = ((XmlSchemaSequence)abstractBaseComplexType.Particle).Items;
            while (sharedElementCount < elementCollection.Count)
            {
                elementCollection.RemoveAt(sharedElementCount);
            }

            abstractBaseElement.Name = AbstractBaseSchemaElementName;
            abstractBaseElement.SchemaTypeName = new XmlQualifiedName(AbstractBaseSchemaComplexTypeName, abstractBaseSchema.TargetNamespace);

            foreach (var baseSchemaImports in abstractBaseSchema.Includes.OfType<XmlSchemaImport>())
            {
                baseSchemaImports.SchemaLocation = null;
            }

            return abstractBaseSchema;
        }

        private static int GetSharedElementCount(IEnumerable<XmlSchema> maindocSchemas)
        {
            return maindocSchemas
                .Select(s => s.Items.OfType<XmlSchemaComplexType>().Single())
                .Select(c => ((XmlSchemaSequence)c.ContentTypeParticle).Items.Cast<XmlSchemaElement>().ToArray())
                .OrderBy(c => c.Length)
                .Aggregate((acc, next) => acc.AsEnumerable().TakeWhile((s, i) => s.QualifiedName == next[i].QualifiedName).ToArray())
                .Count();
        }

        private static XmlSchema DeepCopy(XmlSchema sourceSchema)
        {
            using (var stream = new MemoryStream())
            {
                sourceSchema.Write(stream);
                stream.Position = 0;
                return XmlSchema.Read(stream, null);
            }
        }
    }
}