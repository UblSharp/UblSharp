using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Serialization.Advanced;
using UblSharp.Generator.CodeFixers;
using UblSharp.Generator.ConditionalFeatures;
using UblSharp.Generator.Extensions;
using UblSharp.Generator.XsdFixers;

namespace UblSharp.Generator
{
    public class UblGenerator
    {
        private UblGeneratorOptions _options;
        private readonly List<Action<XmlSchemaSet>> _schemaFixers = new List<Action<XmlSchemaSet>>
            {
                UblCommonBasicComponentFixer.FlattenCommonBasicComponents,
                UblCoreComponentsRenamer.RenameCoreComponentTypes,
                UblBaseDocumentFixer.FixBaseDocumentInheritance
            };

        private readonly CodeNamespaceFixer _codeFixer = new CodeNamespaceFixer(
            new UblDocumentationFixer(),
            new XmlAttributesFixer(),
            new ArrayToListConverter(),
            new ImplicitAssignmentFixer(),
            new AddISharedPropertiesInterface(),
            new FieldToPropertyConverter() // make sure this is done last
        );

        private readonly List<Func<List<string>, int, int>> _conditionalFeatureFixers = new List<Func<List<string>, int, int>>
        {
            FeatureSerialization.Add,
            FeatureXmlDocument.Add
        };

        public void Generate(UblGeneratorOptions options)
        {
            _options = options;
            options.Validate();

            var baseInputDirectory = options.XsdBasePath;
            var commonDirectory = Path.Combine(baseInputDirectory, "common");
            var xmldsigFilename = new DirectoryInfo(commonDirectory).GetFiles("UBL-xmldsig-core-schema-*.xsd").Single().FullName;
            var maindocDirectory = Path.Combine(baseInputDirectory, "maindoc");
            var maindocfiles = new DirectoryInfo(maindocDirectory).GetFiles("*.xsd").ToList();
            // var extrafiles = new DirectoryInfo(commonDirectory).GetFiles("UBL-CommonSignatureComponents*.xsd").ToList();
            // var maindocfiles = new DirectoryInfo(maindocDirectory).GetFiles("UBL-Order-2.1.xsd").ToList();
            // maindocfiles.Add(new DirectoryInfo(maindocDirectory).GetFiles("UBL-BaseDocument-*.xsd").Single());

            var maindocSchemaSet = new XmlSchemaSet();

            var nameTable = new NameTable();
            var readerSettings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                DtdProcessing = DtdProcessing.Parse,
                NameTable = nameTable,
            };

            using (var reader = XmlReader.Create(xmldsigFilename, readerSettings))
            {
                var schema = XmlSchema.Read(reader, null);
                maindocSchemaSet.Add(schema);
            }

            foreach (var maindocfile in maindocfiles)
            {
                using (var reader = XmlReader.Create(maindocfile.FullName, readerSettings))
                {
                    var schema = XmlSchema.Read(reader, null);
                    maindocSchemaSet.Add(schema);
                }
            }

            //foreach (var extrafile in extrafiles)
            //{
            //    using (var reader = XmlReader.Create(extrafile.FullName, readerSettings))
            //    {
            //        var schema = XmlSchema.Read(reader, null);
            //        maindocSchemaSet.Add(schema);
            //    }
            //}

            maindocSchemaSet.Compile();

            foreach (var schemaFixer in _schemaFixers)
            {
                schemaFixer(maindocSchemaSet);
                maindocSchemaSet.Compile();
            }

            var rootNamespaces = maindocSchemaSet.Schemas().OfType<XmlSchema>()
                .Where(x => x.SourceUri.Contains("maindoc"))
                .Select(x => x.TargetNamespace)
                .Concat(new[]
                {
                    Namespaces.Xmldsig,
                    Namespaces.Sac,
                    Namespaces.Csc,
                    Namespaces.Xades141,
                });

            var tempCodeNamespace = CreateCodeNamespace(maindocSchemaSet, rootNamespaces.ToArray());

            _codeFixer.Fix(tempCodeNamespace);

            var codeDeclsBySchema = (from t in tempCodeNamespace.Types.Cast<CodeTypeDeclaration>()
                                     group t by t.GetSchema() into g
                                     select g)
                                     .ToDictionary(k => k.Key, v => v.ToArray());

            var codeProvider = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider();
            var codegenOptions = new CodeGeneratorOptions()
            {
                BracingStyle = "C",
                IndentString = "    ",
                BlankLinesBetweenMembers = true,
                VerbatimOrder = true
            };

            var namespaceProvider = new CodeNamespaceProvider(maindocSchemaSet, options);
            foreach (var schema in maindocSchemaSet.Schemas().Cast<XmlSchema>())
            {
                var codeNamespace = namespaceProvider.CreateCodeNamespace(schema.TargetNamespace);
                if (codeDeclsBySchema.ContainsKey(schema))
                {
                    codeNamespace.Types.AddRange(codeDeclsBySchema[schema]);
                }

                if (codeNamespace.Types.Count == 0)
                {
                    continue;
                }

                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    codeProvider.GenerateCodeFromNamespace(codeNamespace, sw, codegenOptions);
                }

                sb = sb.Replace("Namespace=\"", "Namespace = \"");

                var fileContent = sb.ToString();
                var lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

                foreach (var fixer in _conditionalFeatureFixers)
                {
                    int lineNum = 0;
                    while (true)
                    {
                        lineNum = fixer(lines, lineNum);
                        if (lineNum < 0)
                            break;
                    }
                }

                sb = new StringBuilder(string.Join(Environment.NewLine, lines));

                var xsdFilename = new Uri(schema.SourceUri).LocalPath;
                var fi = new FileInfo(xsdFilename);
                var foldername = namespaceProvider.GetNamespaceFolderName(schema);
                string targetPath = Path.Combine(options.OutputPath, foldername);
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                var outputFile = Path.Combine(targetPath, Path.ChangeExtension(fi.Name, ".generated.cs"));

                using (var ofile = File.CreateText(outputFile))
                {
                    ofile.Write(sb);
                }
            }
        }

        private CodeNamespace CreateCodeNamespace(XmlSchemaSet schemaSet, string[] rootNamespaces)
        {
            var schemas = new XmlSchemas();
            foreach (var s in schemaSet.Schemas())
            {
                schemas.Add((XmlSchema)s);
            }

            schemas.Compile(_options.ValidationHandler, true);
            var schemaImporter = new XmlSchemaImporter(schemas);
            schemaImporter.Extensions.Clear();

            var codeNamespace = new CodeNamespace(_options.Namespace);
            var codeOptions = CodeGenerationOptions.GenerateOrder | CodeGenerationOptions.GenerateNewAsync;
            var codeExporter = new XmlCodeExporter(codeNamespace, null, codeOptions);

            var maps = new List<XmlTypeMapping>();

            //foreach (XmlSchema schema in schemaSet.Schemas().Cast<XmlSchema>().Where(x => x.SourceUri.Contains("maindoc")))
            //{
            //    foreach (XmlSchemaElement element in schema.Elements.Values)
            //    {
            //        var xmlTypeMapping = schemaImporter.ImportTypeMapping(element.QualifiedName);
            //        maps.Add(xmlTypeMapping);
            //    }
            //}

            //foreach (var schemaType in schemaSet.Schemas().Cast<XmlSchema>().Where(x => !x.SourceUri.Contains("maindoc")).SelectMany(x => x.SchemaTypes.Values.Cast<XmlSchemaType>()))
            //{
            //    var xmlTypeMapping = schemaImporter.ImportSchemaType(schemaType.QualifiedName);
            //    maps.Add(xmlTypeMapping);
            //}

            // var qnamesMap = new Dictionary<XmlQualifiedName, string>();

            foreach (var ns in rootNamespaces)
            {
                var schema = schemas[ns];

                Console.WriteLine($"Import root namespace: {ns}");
                foreach (XmlSchemaElement element in schema.Elements.Values)
                {
                    Console.WriteLine($"Import type mapping for {element.QualifiedName.Namespace} : {element.QualifiedName.Name}");
                    var xmlTypeMapping = schemaImporter.ImportTypeMapping(element.QualifiedName);
                    maps.Add(xmlTypeMapping);
                }
            }

            //foreach (XmlSchemaType element in schemaSet.Schemas().Cast<XmlSchema>().Where(x => !rootNamespaces.Contains(x.TargetNamespace)).SelectMany(x => x.SchemaTypes.Values.Cast<XmlSchemaType>()))
            //{
            //    var xmlTypeMapping = schemaImporter.ImportSchemaType(element.QualifiedName);
            //    maps.Add(xmlTypeMapping);
            //}

            foreach (var xmlTypeMapping in maps)
            {
                codeExporter.ExportTypeMapping(xmlTypeMapping);
            }

            var codeDeclarations = codeNamespace.Types.Cast<CodeTypeDeclaration>().ToList();

            foreach (var codeDecl in codeDeclarations)
            {
                codeDecl.Comments.Clear();
                foreach (var item in codeDecl.Members.OfType<CodeTypeMember>())
                {
                    item.Comments.Clear();
                }

                var qname = codeDecl.GetQualifiedName();
                codeDecl.UserData[CodeTypeMemberExtensions.QualifiedNameKey] = qname;
                var schema = schemas[qname.Namespace];
                codeDecl.UserData[CodeTypeMemberExtensions.XmlSchemaKey] = schema;
                var xmlSchemaType = (XmlSchemaType)schema.SchemaTypes[qname];
                codeDecl.UserData[CodeTypeMemberExtensions.XmlSchemaTypeKey] = xmlSchemaType;

                foreach (CodeTypeMember member in codeDecl.Members)
                {
                    member.UserData[CodeTypeMemberExtensions.XmlSchemaKey] = schema;
                    member.UserData[CodeTypeMemberExtensions.XmlSchemaTypeKey] = xmlSchemaType;
                }
            }

            return codeNamespace;
        }
    }
}