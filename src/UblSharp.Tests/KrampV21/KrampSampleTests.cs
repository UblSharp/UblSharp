using System.Linq;
using UblSharp.Tests.Util;
using UblSharp.Validation;
using Xunit;
using Xunit.Abstractions;

namespace UblSharp.Tests.KrampV21
{
    public class KrampSampleTests
    {
        private readonly ITestOutputHelper _output;

        public KrampSampleTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(Skip = "Invalid sample")]
        public void OrderSample()
        {
            var ordersample = UblDocument.FromStream<OrderType>(ResourceHelper.GetResource("KrampV21.UBL20-Order-sample.xml"));

            var errors = ordersample.Validate();

            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }

            Assert.True(ordersample.IsValid());
            Assert.Equal(0, errors.Count());
        }

        [Fact]
        public void OrderSampleCorrected()
        {
            var ordersample = UblDocument.FromStream<OrderType>(ResourceHelper.GetResource("KrampV21.UBL20-Order-sample-corrected.xml"));

            var errors = ordersample.Validate();

            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }

            Assert.True(ordersample.IsValid());
            Assert.Equal(0, errors.Count());
        }

        [Fact(Skip = "Invalid sample")]
        public void OrderResponseSample()
        {
            var orderconfirmationsample = UblDocument.FromStream<OrderResponseType>(ResourceHelper.GetResource("KrampV21.UBL20-Orderconfirmation-sample.xml"));

            var errors = orderconfirmationsample.Validate();

            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }

            Assert.True(orderconfirmationsample.IsValid());
            Assert.Equal(0, errors.Count());
        }

        [Fact]
        public void OrderResponseSampleCorrected()
        {
            var orderconfirmationsample = UblDocument.FromStream<OrderResponseType>(ResourceHelper.GetResource("KrampV21.UBL20-Orderconfirmation-sample-corrected.xml"));

            var errors = orderconfirmationsample.Validate();

            foreach (var error in errors)
            {
                _output.WriteLine($"{error.Severity}: {error.Message}");
            }

            Assert.True(orderconfirmationsample.IsValid());
            Assert.Equal(0, errors.Count());
        }
    }
}