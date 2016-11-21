using System.IO;
using System.Reflection;

namespace UblSharp.Tests.Util
{
    public static class ResourceHelper
    {
        public static Stream GetResource(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + "." + name);
        }
    }
}