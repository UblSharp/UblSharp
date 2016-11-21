using Xunit;

namespace UblSharp.Tests.Util
{
    public class UblSampleFactAttribute : FactAttribute
    {
        public UblSampleFactAttribute(string documentFilename)
        {
            DocumentFilename = documentFilename;
            string reason;
            if (ShouldSkipTest(out reason))
            {
                Skip = reason;
            }
        }

        public string DocumentFilename { get; set; }

        public bool ShouldSkipTest(out string reason)
        {
            return SampleTests.SkippedTests.TryGetValue(DocumentFilename, out reason);
        }
    }
}