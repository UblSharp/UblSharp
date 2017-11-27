using System;
using System.Net;
using System.Reflection;
using System.Xml;

namespace UblSharp.Validation.Internal
{
    public class UblXsdResolver : XmlUrlResolver
    {
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            var xsdResourceAssembly = typeof(UblXsdResolver).Assembly;

            if (absoluteUri.IsFile && absoluteUri.Segments.Length >= 2)
            {
                var assemblyName = xsdResourceAssembly.GetName().Name;
                var documentName = absoluteUri.Segments[absoluteUri.Segments.Length - 2].Trim('/') + "." +
                                   absoluteUri.Segments[absoluteUri.Segments.Length - 1].Trim('/');
                var manifestName = $"{assemblyName}.Resources.{documentName}";

                var manifestStream = xsdResourceAssembly.GetManifestResourceStream(manifestName);
                if (manifestStream == null)
                {
                    return base.GetEntity(absoluteUri, role, ofObjectToReturn);
                }

                return manifestStream;
            }
            
            return base.GetEntity(absoluteUri, role, ofObjectToReturn);
        }        
    }
}
