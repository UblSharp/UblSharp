# UblSharp

[![Build status](https://ci.appveyor.com/api/projects/status/rlhygf9fm4w5l0e4/branch/master?svg=true)](https://ci.appveyor.com/project/ublsharp/ublsharp/branch/master)
[![MyGet](https://img.shields.io/myget/ublsharp/v/ublsharp.svg)](https://www.myget.org/gallery/ublsharp)
[![NuGet](https://img.shields.io/nuget/v/ublsharp.svg)](https://www.nuget.org/packages/ublsharp)

UblSharp is a C# / .NET / XML library for working with OASIS UBL 2.0 and 2.1 documents.

It supports all .NET full framework versions from .NET 2.0 - 4.6 and .NET Standard 1.0 and higher. See 'Available packages' below for a table of available packages and framework compatibility.

## Installation

Use the nuget packages. See the table below for an overview of available packages.

For example, using the Visual Studio package manager console:

    Install-Package UblSharp
    Install-Package UblSharp.Validation

Or using the dotnet cli:

    dotnet add package UblSharp

### Available packages

| Package               | .NET Support | Description |
| --------------------- | ------------ | ----------- |
| UblSharp              | net20 - net46,<br /> netstandard1.0+ | Contains all UBL 2.0/2.1 common, aggregate and document types, annotated with System.Xml.* attributes. Also contains some basic serialization functions. | 
| UblSharp.Validation   | net20 - net46, netstandard2.0 | Contains validation functions to validate XML documents and UblSharp (.NET) objects using the OASIS UBL 2.1 xsd specifications. |
| UblSharp.SEeF   | net20 - net46,<br /> netstandard1.0+ | Additional types to support the UBL extension 'Standaard Energie eFactuur'. |
| UblSharp.Generator.Core   | net46 | The code generator library used to generate C# classes from XSD schemas. |

> We can't support validation on netstandard1.0, because System.Xml.Schema and validation is only available on desktop .net and netstandard2.0 and higher.

## UblSharp / OASIS UBL version compatibility

The table below shows which version of OASIS UBL is implemented in UblSharp. Note that minor updates to UBL (2.x) are backwards compatible. It is possible to read 'UBL 2.0/2.1' documents with UblSharp 2.0 (which is based on UBL 2.2).

In some occasions, UblSharp C# types are not binary compatible with types from earlier releases. In that case UblSharp will use major version increase.

> For example: between UBL 2.1 and 2.2, the cardinality of *CallForTenders.ContractingParty* changed from 1..1 to 1..n. This changed the C# class to contain a property of *List\<ContractingPartyType\>* instead of just *ContractingPartyType*. Due to the nature of XML, this was not a breaking change for UBL, but it is in C#.

| UblSharp version      | UBL Version  |
| --------------------- | ------------ |
| 1.*                   | 2.0 / 2.1    |
| 2.*                   | 2.2          |

## Credits

- The (test) generator of UblSharp was taken from https://github.com/Gammern/ubllarsen (a lot has changed since).

## License

[The MIT License (MIT)](LICENSE)
