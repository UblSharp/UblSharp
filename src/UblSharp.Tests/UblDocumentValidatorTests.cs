using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UblSharp.Tests.Util;
using UblSharp.Validation;
using Xunit;
using Xunit.Abstractions;

namespace UblSharp.Tests
{
    public class UblDocumentValidatorTests
    {
        private readonly ITestOutputHelper _output;

        public UblDocumentValidatorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public void AssertValidDocument<T>(string resourceName) where T : BaseDocument
        {
            ShouldBeValidDocumentOf<T>(ResourceHelper.GetResource(resourceName));
            ShouldBeValidXmlDocument(ResourceHelper.GetResource(resourceName));
            ShouldBeValidXDocument(ResourceHelper.GetResource(resourceName));
        }

        public void AssertInvalidDocument<T>(string resourceName) where T : BaseDocument
        {
            ShouldBeInvalidDocumentOf<T>(ResourceHelper.GetResource(resourceName));
            ShouldBeInvalidXmlDocument(ResourceHelper.GetResource(resourceName));
            ShouldBeInvalidXDocument(ResourceHelper.GetResource(resourceName));
        }

        public void ShouldBeValidDocumentOf<T>(Stream documentStream) where T : BaseDocument
        {
            var validator = new UblDocumentValidator();
            var document = UblDocument.Load<T>(documentStream);

            // _output.WriteLine(document.GetType().ToString());

            var errors = validator.Validate(document).ToList();
            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }

            Assert.True(validator.IsValid(document));
            Assert.Empty(errors);
        }

        public void ShouldBeValidXmlDocument(Stream documentsStream)
        {
            var validator = new UblDocumentValidator();
            var document = new XmlDocument();
            document.Load(documentsStream);
            var errors = validator.Validate(document).ToList();
            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }

            Assert.True(validator.IsValid(document));
            Assert.Empty(errors);
        }

        public void ShouldBeValidXDocument(Stream documentsStream)
        {
            var validator = new UblDocumentValidator();
            var document = XDocument.Load(documentsStream);
            var errors = validator.Validate(document).ToList();
            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }

            Assert.True(validator.IsValid(document));
            Assert.Empty(errors);
        }

        public void ShouldBeInvalidDocumentOf<T>(Stream documentStream) where T : BaseDocument
        {
            var validator = new UblDocumentValidator();
            var document = UblDocument.Load<T>(documentStream);
            var errors = validator.Validate(document).ToList();
            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }

            Assert.False(validator.IsValid(document));
            Assert.NotEmpty(errors);
        }

        public void ShouldBeInvalidXmlDocument(Stream documentsStream)
        {
            var validator = new UblDocumentValidator();
            var document = new XmlDocument();
            document.Load(documentsStream);
            var errors = validator.Validate(document).ToList();
            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }

            Assert.False(validator.IsValid(document));
            Assert.NotEmpty(errors);
        }

        public void ShouldBeInvalidXDocument(Stream documentsStream)
        {
            var validator = new UblDocumentValidator();
            var document = XDocument.Load(documentsStream);
            var errors = validator.Validate(document).ToList();
            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }

            Assert.False(validator.IsValid(document));
            Assert.NotEmpty(errors);
        }
    }
}