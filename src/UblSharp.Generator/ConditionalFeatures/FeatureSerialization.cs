using System.Collections.Generic;

namespace UblSharp.Generator.ConditionalFeatures
{
    public class FeatureSerialization
    {
        const string SerializableAttributeText = "[System.SerializableAttribute";

        public static int Add(List<string> lines, int lineNum)
        {
            lineNum = lines.FindIndex(lineNum, s => s.Contains(SerializableAttributeText));
            if (lineNum < 0)
                return lineNum;

            lines.Insert(lineNum, "#if FEATURE_SERIALIZATION");
            lines.Insert(lineNum + 2, "#endif");
            lineNum += 3;
            return lineNum;
        }
    }
}