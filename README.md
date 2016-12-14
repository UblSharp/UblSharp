# UblSharp

[![Build status](https://ci.appveyor.com/api/projects/status/rlhygf9fm4w5l0e4/branch/master?svg=true)](https://ci.appveyor.com/project/ublsharp/ublsharp/branch/master)
[![MyGet](https://img.shields.io/myget/ublsharp/v/ublsharp.svg)](https://www.myget.org/gallery/ublsharp)
[![NuGet](https://img.shields.io/nuget/v/ublsharp.svg)](https://www.nuget.org/packages/ublsharp)

UblSharp is a C# / .NET / XML library for working with OASIS UBL 2.0 and 2.1 documents.

It supports all .NET full framework versions from .NET 2.0 - 4.6 and .NET Standard 1.0 and higher. 


# Installation

Use the nuget packages. See the table below for an overview of available packages.

For example, using the Visual Studio package manager console:

    Install-Package UblSharp
    Install-Package UblSharp.Validation

## Available packages

| Package               | .NET Support | Description |
| --------------------- | ------------ | ----------- |
| UblSharp              | net20 - net46,<br /> netstandard1.0 | Contains all UBL 2.0/2.1 common, aggregate and document types, annotated with System.Xml.* attributes. Also contains some basic serialization functions. | 
| UblSharp.Validation   | net20 - net46 | Contains validation functions to validate XML documents and UblSharp (.NET) objects using the OASIS UBL 2.1 xsd specifications. |

> System.Xml.Schema is not yet available for netstandard. As soon as schema validation becomes available in .NET standard 2.0, support will be added to UblSharp.Validation. 

# License

[The MIT License (MIT)](LICENSE)
