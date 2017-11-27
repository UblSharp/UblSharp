using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using UblSharp.Generator.CodeFixers;
using UblSharp.Generator.ConditionalFeatures;
using UblSharp.Generator.Extensions;
using UblSharp.Generator.Internal;
using UblSharp.Generator.XsdFixers;

namespace UblSharp.Generator
{
    public class UblGenerator
    {
        private readonly ValidationEventHandler _schemaValHandler = (s, e) => { throw new Exception(e.Message); };
        private UblGeneratorOptions _options;
        private readonly List<Action<XmlSchemaSet>> _schemaFixers = new List<Action<XmlSchemaSet>>
            {
                UblCommonBasicComponentFixer.FlattenCommonBasicComponents,
                UblCoreComponentsRenamer.RenameCoreComponentTypes,
                UblBaseDocumentFixer.FixBaseDocumentInheritance
            };

        private readonly CodeNamespaceFixer _globalCodeFixer = new CodeNamespaceFixer(
            new ImplicitAssignmentFixer()
        );

        private readonly CodeNamespaceFixer _namespacedCodeFixer = new CodeNamespaceFixer(
            new RemoveConflictingTypes(),
            new RenameXadesIntPropertyFixer(),
            new ItemsChoiceTypeRenamer(), // and this really last (after properties are converted)
            new RenameTypesWithSuffix(),
            new FixAmbigiousIdentifierType(),
            new UblDocumentationFixer(),
            new XmlAttributesFixer(),
            // new AddClsCompliantAttribute(),
            new AddXmlMemberOrderToFields(),
            new ArrayToListConverter(),
            new AddISharedPropertiesInterface(),
            new FieldToPropertyConverter() // make sure this is done last,
        );

        private readonly List<Func<List<string>, int, int>> _conditionalFeatureFixers = new List<Func<List<string>, int, int>>
        {
            FeatureSerialization.Add,
            FeatureXmlDocument.Add
        };

        public void Generate(UblGeneratorOptions options)
        {
            Console.WriteLine("Validating options...");

            _options = options;
            options.Validate();

            var baseInputDirectory = options.XsdBasePath;
            //var commonDirectory = Path.Combine(baseInputDirectory, "common");
            // var extDirectory = Path.Combine(baseInputDirectory, "ext");
            //var commonfiles = new DirectoryInfo(commonDirectory).GetFiles("*.xsd").ToList();
            //var preAddFiles = new List<string>();
            //preAddFiles.Add(new DirectoryInfo(commonDirectory).GetFiles("UBL-xmldsig-core-schema-*.xsd").Single().FullName);
            //preAddFiles.Add(new DirectoryInfo(commonDirectory).GetFiles("UBL-XAdESv141-2.1.xsd").Single().FullName);

            // var maindocDirectory = Path.Combine(baseInputDirectory);
            var maindocfiles = new DirectoryInfo(baseInputDirectory).GetFiles("*.xsd").ToList();
            //var extfiles = new DirectoryInfo(extDirectory).GetFiles("*.xsd").ToList();
            // var maindocfiles = new DirectoryInfo(maindocDirectory).GetFiles("UBL-Order-2.1.xsd").ToList();
            // maindocfiles.Add(new DirectoryInfo(maindocDirectory).GetFiles("UBL-BaseDocument-*.xsd").Single());
            //var knownSchema = commonfiles.ToDictionary(x => x.Name, x => x.FullName);
            // var xsdResolver = new UblXsdResolver(commonfiles.ToDictionary(x => x.Name, x => x.FullName));

            var xsdResolver = new UblXsdResolver();
            var maindocSchemaSet = new XmlSchemaSet()
            {
                // XmlResolver = xsdResolver
                XmlResolver = null
            };

            Console.WriteLine("Read common UBL files from embedded resources...");

            var nameTable = new NameTable();
            var readerSettings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                DtdProcessing = DtdProcessing.Parse,
                NameTable = nameTable
            };

            AddCommonFiles(maindocSchemaSet, readerSettings);

            //foreach (var preAddFile in preAddFiles)
            //{
            //    using (var reader = XmlReader.Create(preAddFile, readerSettings))
            //    {
            //        var schema = XmlSchema.Read(reader, null);
            //        maindocSchemaSet.Add(schema);
            //    }
            //}

            Console.WriteLine($"Add schema files from {baseInputDirectory}...");

            var schemasToGenerate = new List<XmlSchema>();
            foreach (var maindocfile in maindocfiles)
            {
                using (var reader = XmlReader.Create(maindocfile.FullName, readerSettings))
                {
                    var schema = XmlSchema.Read(reader, null);

                    // ReplaceKnownSchemaIncludes(schema, knownSchema);
                    maindocSchemaSet.Add(schema);
                    schemasToGenerate.Add(schema);
                }
            }

            //var extRootNamespaces = new List<string>();
            //foreach (var extfile in extfiles)
            //{
            //    using (var reader = XmlReader.Create(extfile.FullName, readerSettings))
            //    {
            //        var schema = XmlSchema.Read(reader, null);
            //        ReplaceKnownSchemaIncludes(schema, knownSchema);

            //        maindocSchemaSet.Add(schema);
            //        extRootNamespaces.Add(schema.TargetNamespace);
            //    }
            //}

            Console.WriteLine("Fixing schema...");

            maindocSchemaSet.Compile();

            foreach (var schemaFixer in _schemaFixers)
            {
                schemaFixer(maindocSchemaSet);
                maindocSchemaSet.Compile();
            }

            Console.WriteLine("Creating compile unit...");

            var namespaceProvider = new CodeNamespaceProvider(maindocSchemaSet, options);
            var compileUnit = CreateCodeNamespace(maindocSchemaSet, namespaceProvider);

            foreach (var ns in compileUnit.Namespaces.OfType<CodeNamespace>())
            {
                _globalCodeFixer.Fix(ns);
            }

            var codeProvider = new CSharpCodeProvider();
            var codegenOptions = new CodeGeneratorOptions()
            {
                BracingStyle = "C",
                IndentString = "    ",
                BlankLinesBetweenMembers = true,
                VerbatimOrder = true
            };

            Console.WriteLine("Generating source files...");

            if (options.GenerateCommonFiles)
            {
                foreach (var schema in maindocSchemaSet.Schemas().Cast<XmlSchema>())
                {
                    GenerateCodeForSchema(options, compileUnit, namespaceProvider, codeProvider, codegenOptions, schema);
                }
            }
            else
            {
                foreach (var schema in schemasToGenerate)
                {
                    GenerateCodeForSchema(options, compileUnit, namespaceProvider, codeProvider, codegenOptions, schema);
                }
            }
        }

        private void GenerateCodeForSchema(UblGeneratorOptions options, CodeCompileUnit compileUnit, CodeNamespaceProvider namespaceProvider, CSharpCodeProvider codeProvider, CodeGeneratorOptions codegenOptions, XmlSchema schema)
        {
            var tmpCodeNamespace = compileUnit.Namespaces.OfType<CodeNamespace>().Single(x => x.UserData[CodeTypeMemberExtensions.XmlSchemaKey] == schema);
            var codeDeclsBySchema = (from t in tmpCodeNamespace.Types.Cast<CodeTypeDeclaration>()
                                     group t by t.GetSchema()
                                     into g
                                     select g)
                .ToDictionary(k => k.Key, v => v.ToArray());

            var codeNamespace = namespaceProvider.CreateCodeNamespace(schema);
            if (codeDeclsBySchema.ContainsKey(schema))
            {
                codeNamespace.Types.AddRange(codeDeclsBySchema[schema]);
            }

            if (codeNamespace.Types.Count == 0)
            {
                return;
            }

            _namespacedCodeFixer.Fix(codeNamespace);

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
                Console.WriteLine($"Written: {outputFile}");
            }
        }

        private void AddCommonFiles(XmlSchemaSet schemas, XmlReaderSettings readerSettings)
        {
            var thisAssembly = typeof(UblGenerator).Assembly;

            Add("CCTS_CCT_SchemaModule-2.1.xsd");
            // schemas.Compile();

            Add("UBL-UnqualifiedDataTypes-2.1.xsd");
            Add("UBL-QualifiedDataTypes-2.1.xsd");
            Add("UBL-SignatureBasicComponents-2.1.xsd");
            // schemas.Compile();

            Add("UBL-CommonBasicComponents-2.1.xsd");
            // schemas.Compile();

            Add("UBL-CommonAggregateComponents-2.1.xsd");
            // schemas.Compile();

            Add("UBL-xmldsig-core-schema-2.1.xsd");
            Add("UBL-XAdESv132-2.1.xsd");
            Add("UBL-XAdESv141-2.1.xsd");
            Add("UBL-SignatureAggregateComponents-2.1.xsd");
            // schemas.Compile();

            Add("UBL-CommonSignatureComponents-2.1.xsd");
            // schemas.Compile();

            Add("UBL-ExtensionContentDataType-2.1.xsd");
            Add("UBL-CommonExtensionComponents-2.1.xsd");
            // schemas.Compile();

            void Add(string filename)
            {
                var resourceName = $"{typeof(UblGenerator).Namespace}.Resources.common.{filename}";

                using (var manifestStream = thisAssembly.GetManifestResourceStream(resourceName))
                {
                    if (manifestStream == null)
                    {
                        throw new InvalidOperationException($"Error while getting manifest resource '{resourceName}'");
                    }

                    using (var rdr = XmlReader.Create(manifestStream, readerSettings))
                    {
                        var schema = XmlSchema.Read(rdr, _schemaValHandler);
                        schema.SourceUri = "file:///" + filename;
                        schemas.Add(schema);
                    }
                }
            }
        }

        private void ReplaceKnownSchemaIncludes(XmlSchema schema, Dictionary<string, string> knownSchemas)
        {
            foreach (var schemaInclude in schema.Includes.OfType<XmlSchemaImport>())
            {
                var knownSchema = knownSchemas.Keys.FirstOrDefault(x => schemaInclude.SchemaLocation.EndsWith(x, StringComparison.OrdinalIgnoreCase));
                if (knownSchema != null)
                {
                    schemaInclude.SchemaLocation = knownSchemas[knownSchema];
                }
            }
        }

        private CodeCompileUnit CreateCodeNamespace(XmlSchemaSet schemaSet, CodeNamespaceProvider namespaceProvider)
        {
            var compileUnit = new CodeCompileUnit();
            var schemas = new XmlSchemas();
            foreach (var s in schemaSet.Schemas())
            {
                schemas.Add((XmlSchema)s);
            }

            schemas.Compile(_options.ValidationHandler, true);

            foreach (XmlSchema schema in schemas)
            {
                var codeNamespace = namespaceProvider.CreateCodeNamespace(schema);
                compileUnit.Namespaces.Add(codeNamespace);
            }

            var codeOptions = CodeGenerationOptions.GenerateOrder | CodeGenerationOptions.GenerateNewAsync;

            // foreach (var ns in rootNamespaces.Concat(extensionNamespaces))
            foreach (XmlSchema schema in schemas)
            {
                var schemaImporter = new XmlSchemaImporter(schemas);
                schemaImporter.Extensions.Clear();

                // var schema = schemas[ns];
                var codeNamespace = compileUnit.Namespaces.OfType<CodeNamespace>().Single(x => x.UserData[CodeTypeMemberExtensions.XmlSchemaKey] == schema);

                Console.WriteLine($"Import root namespace: {schema.TargetNamespace}");

                var codeExporter = new XmlCodeExporter(codeNamespace, compileUnit, codeOptions);

                foreach (XmlSchemaElement element in schema.Elements.Values.OfType<XmlSchemaElement>())
                {
                    Console.WriteLine($"Import type mapping for {element.QualifiedName.Namespace} : {element.QualifiedName.Name}");
                    var xmlTypeMapping = schemaImporter.ImportTypeMapping(element.QualifiedName);
                    codeExporter.ExportTypeMapping(xmlTypeMapping);
                }

                var baseTypes = schema.Elements.Values.OfType<XmlSchemaElement>().Select(x => x.SchemaTypeName.Name).ToList();

                foreach (XmlSchemaComplexType element in schema.SchemaTypes.Values.OfType<XmlSchemaComplexType>().Where(x => !baseTypes.Contains(x.Name)))
                {
                    Console.WriteLine($"Import schema type for {element.QualifiedName.Namespace} : {element.QualifiedName.Name}");
                    var xmlTypeMapping = schemaImporter.ImportSchemaType(element.QualifiedName);
                    codeExporter.ExportTypeMapping(xmlTypeMapping);
                }
            }

            var codeDeclarations = compileUnit.Namespaces.OfType<CodeNamespace>().SelectMany(x => x.Types.Cast<CodeTypeDeclaration>()).ToList();

            foreach (var codeDecl in codeDeclarations)
            {
                codeDecl.Comments.Clear();
                foreach (var item in codeDecl.Members.OfType<CodeTypeMember>())
                {
                    item.Comments.Clear();
                }

                var qname = codeDecl.GetQualifiedName();
                codeDecl.UserData[CodeTypeMemberExtensions.QualifiedNameKey] = qname;
                // Note, this can fail when there are multiple schemas for one namespace, like urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2
                var schema = schemas.GetSchemas(qname.Namespace).OfType<XmlSchema>().FirstOrDefault();
                // var schema = schemas[qname.Namespace];
                codeDecl.UserData[CodeTypeMemberExtensions.XmlSchemaKey] = schema;
                var xmlSchemaType = (XmlSchemaType)schema.SchemaTypes[qname];
                codeDecl.UserData[CodeTypeMemberExtensions.XmlSchemaTypeKey] = xmlSchemaType;

                foreach (CodeTypeMember member in codeDecl.Members)
                {
                    member.UserData[CodeTypeMemberExtensions.XmlSchemaKey] = schema;
                    member.UserData[CodeTypeMemberExtensions.XmlSchemaTypeKey] = xmlSchemaType;
                }
            }

            return compileUnit;
        }

        //private class UblXsdResolver : XmlUrlResolver
        //{
        //    private readonly IDictionary<string, string> _knownSchemas;

        //    public UblXsdResolver(IDictionary<string, string> knownSchemas)
        //    {
        //        _knownSchemas = knownSchemas;
        //    }

        //    public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        //    {
        //        if (absoluteUri.Scheme.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        //        {
        //            var uri = absoluteUri.ToString();

        //            var knownSchemaName = _knownSchemas.Keys.FirstOrDefault(x => uri.EndsWith(x, StringComparison.OrdinalIgnoreCase));
        //            if (!string.IsNullOrEmpty(knownSchemaName))
        //            {
        //                return File.OpenRead(_knownSchemas[knownSchemaName]);
        //            }
        //        }

        //        return base.GetEntity(absoluteUri, role, ofObjectToReturn);
        //    }

        //    public override ICredentials Credentials { set { throw new NotImplementedException(); } }
        //}
    }
}
