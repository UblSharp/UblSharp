# UblSharp

UblSharp is a C# / .NET / XML library for working with OASIS UBL 2.0 and 2.1 documents.

It supports all .NET full framework versions from .NET 2.0 - 4.6 and .NET Standard library 1.1 and higher. 

# Installation

Use the nuget packages. See the table below for an overview of available packages.

For example, using the Visual Studio package manager console:

    Install-Package UblSharp
    Install-Package UblSharp.Validation

## Available packages

| Package               | Description 
| --------------------- | ----------- 
| UblSharp              | Contains all UBL 2.0/2.1 common, aggregate and document types, annoted with .NET Xml* attributes. Also contains some basic serialization functions. 
| UblSharp.Validation   | Contains validation functions to validate XML documents and UblSharp (.NET) objects using the OASIS UBL 2.1 xsd specifications. 


# License

[The MIT License (MIT)](LICENSE)
