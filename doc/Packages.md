# UblSharp packages

## Package overview

| Package                 | .NET Support | Description
| ----------------------- | ------------ | -----------
| UblSharp                | net20 - net46,<br /> netstandard1.0 | Contains all UBL 2.0/2.1 common, aggregate and document types, annotated with System.Xml.* attributes. UBL document serialization, deserialization and formatting features. 
| UblSharp.Validation     | net20 - net46 | Validation for UBL XML documents and UblSharp .NET objects against the OASIS UBL 2.1 XML schema definitions (embedded).


## UblSharp

> Libraries for: net20, net35, net40, net45, netstandard1.0, netstandard1.3+

- Contains all UBL 2.0/2.1 common, aggregate and document types, annotated with System.Xml.* attributes.
- Serialization/deserialization functionality. Reading, saving and formattting for UBL XML documents and UblSharp .NET objects.

## UblSharp.Validation

> Libraries for: net20, net35, net40, net45

Validation for UBL XML documents and UblSharp .NET objects against the OASIS UBL 2.1 XML schema definitions (embedded).

TODO:

- Decide on final package structure / naming (meta package or not)
- Generate XmlSerializers.dll with sgen.exe and include them in the nuget package
- Support netstandard2.0 in UblSharp.Validation when released
- Track https://github.com/dotnet/corefx/issues/4561 for sgen support in .net standard/core.
