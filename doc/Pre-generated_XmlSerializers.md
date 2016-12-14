# Pre-generated XmlSerializers assembly

TODO

- sgen support (pre-generated xml serializer assembly)
  - for unit testing! speeds it up A LOT !!!
  - some explaining on start-up (constructor) performance of XmlSerializer.
  - sgen UblSharp(.Types) for .net desktop frameworks and include them in nuget package
  - support sgen on .net core (when it's available) or investigate other performance improvements

Note: performance issue is one-time only (first time XmlSerializer for a type is created), this can become an issue for startup time or in server (web) applications.


sgen command for reference:

"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6 Tools\sgen.exe" /assembly:C:\Projects\UblSharp\src\UblSharp\bin\Debug\net45\UblSharp.dll /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\mscorlib.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.Core.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.Data.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.Xml.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.Xml.Linq.dll" /verbose /force
