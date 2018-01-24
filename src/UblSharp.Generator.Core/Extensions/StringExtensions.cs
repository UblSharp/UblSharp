using System.Linq;

namespace UblSharp.Generator.Extensions
{
    public static class StringExtensions
    {
        public static string MakePrivateFieldName(this string str) => string.Concat(str.Select((c, i) => i == 0 ? "_" + char.ToLowerInvariant(c) : c.ToString()));

        public static string MakePrivatePropertyName(this string str) => "__" + str;
    }
}