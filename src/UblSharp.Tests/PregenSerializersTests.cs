#if FEATURE_SGEN
using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Xunit;

namespace UblSharp.Tests
{
    public class PregenSerializersTests
    {
        // [Fact(Skip = "Disable sgen test until supported")]
        [Fact]
        public void CanLoadXmlSerializers()
        {
            AssertCanLoadXmlSerializers(typeof(OrderType));
        }

        /// <summary>Generates an identifier for the assembly of a specified type</summary>
        /// <remarks>Code copied from the .NET serialization classes - to emulate the same bahavior</remarks>
        /// <param name="type">The type</param>
        /// <returns>String identifying the type's assembly</returns>
        static string GenerateAssemblyId(Type type)
        {
            var modules = type.Assembly.GetModules();
            var list = new ArrayList();
            for (var i = 0; i < modules.Length; i++)
            {
                list.Add(modules[i].ModuleVersionId.ToString());
            }
            list.Sort();
            var sb = new StringBuilder();
            for (var i = 0; i < list.Count; i++)
            {
                sb.Append(list[i].ToString());
                sb.Append(",");
            }
            return sb.ToString();
        } // GenerateAssemblyId

        /// <summary>Verifies that the serialization assembly for the specified type can be loaded</summary>
        /// <remarks>Code copied from the .NET serialization classes - to emulate the same behavior and tests</remarks>
        /// <param name="type">The type</param>
        static void AssertCanLoadXmlSerializers(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            Assembly serializerAssembly = null;
            // Create the name of the XML serilizers assembly from the type's one
            var name = type.Assembly.GetName(true);
            name.Name = name.Name + ".XmlSerializers";
            name.CodeBase = null;
            name.CultureInfo = CultureInfo.InvariantCulture;
            try
            {
                serializerAssembly = Assembly.Load(name);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to load XML serialization assembly for type '{type.FullName}': {e.Message}");
            }

            var attrs = serializerAssembly.GetCustomAttributes(typeof(XmlSerializerVersionAttribute), false);
            if (attrs == null || attrs.Length == 0)
            {
                throw new Exception($"Unable to use XML serialization assembly '{serializerAssembly.FullName}' for type '{type.FullName}': it does not contain XmlSerializerVersionAttribute");
            }
            if (attrs.Length > 1)
            {
                throw new Exception($"Unable to use XML serialization assembly '{serializerAssembly.FullName}' for type '{type.FullName}': it contains multiple XmlSerializerVersionAttribute");
            }

            var assemblyInfo = (XmlSerializerVersionAttribute)attrs[0];
            var assemblyId = GenerateAssemblyId(type);
            if (assemblyInfo.ParentAssemblyId != assemblyId)
            {
                throw new Exception($"Unable to use XML serialization assembly '{serializerAssembly.FullName}' for type '{type.FullName}': it does not match assembly id '{assemblyId}'");
            }
        } // AssertCanLoadXmlSerializers
    }
}
#endif
