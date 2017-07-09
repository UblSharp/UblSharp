using System.IO;
using System.Reflection;

namespace UblSharp.Tests.Util
{
    public static class ResourceHelper
    {
        public static Stream GetResource(string name)
        {
            var assembly = typeof(ResourceHelper).GetTypeInfo().Assembly;
            return assembly.GetManifestResourceStream(assembly.GetName().Name + "." + name);
        }
    }
}
