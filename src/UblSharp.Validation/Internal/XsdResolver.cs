using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Xml;

namespace UblSharp.Validation.Internal
{
    internal class XsdResolver : XmlResolver
    {
        private static readonly Dictionary<Uri, object> _xsdCache = new Dictionary<Uri, object>();

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            if (_xsdCache.ContainsKey(absoluteUri))
            {
                return _xsdCache[absoluteUri];
            }

            var xsdResourceAssembly = Assembly.GetExecutingAssembly();

            if (absoluteUri.IsFile && absoluteUri.Segments.Length >= 2)
            {
                var manifestName = string.Format("{0}.Resources.{1}.{2}",
                                                 xsdResourceAssembly.GetName().Name,
                                                 absoluteUri.Segments[absoluteUri.Segments.Length - 2].Trim('/'),
                                                 absoluteUri.Segments[absoluteUri.Segments.Length - 1]);

                var manifestStream = xsdResourceAssembly.GetManifestResourceStream(manifestName);
                if (manifestStream == null)
                {
                    throw new Exception("Could not find xsd: " + manifestName);
                }

                _xsdCache[absoluteUri] = manifestStream;
                return manifestStream;
            }

            return null;
        }

        public override ICredentials Credentials { set { throw new NotImplementedException(); } }
    }
}