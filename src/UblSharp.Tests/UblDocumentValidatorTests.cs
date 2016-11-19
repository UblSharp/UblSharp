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

        [Fact]
        public void CanValidatorOrder()
        {
            var order = new OrderType();
            order.ID = "test";

            var validator = new UblDocumentValidator();
            var errors = validator.Validate(order);

            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }
        }
    }
}