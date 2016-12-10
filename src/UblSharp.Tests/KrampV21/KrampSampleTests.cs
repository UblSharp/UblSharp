using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UblSharp.Tests.Util;
using UblSharp.Validation;
using Xunit;
using Xunit.Abstractions;

namespace UblSharp.Tests.KrampV21
{
    public class KrampSampleTests : UblDocumentValidatorTests
    {
        private readonly ITestOutputHelper _output;

        public KrampSampleTests(ITestOutputHelper output)
            : base(output)
        {
            _output = output;
        }

        [Fact]
        public void OrderSample()
        {
            AssertInvalidDocument<OrderType>("KrampV21.UBL20-Order-sample.xml");
        }

        [Fact]
        public void OrderSampleCorrected()
        {
            AssertValidDocument<OrderType>("KrampV21.UBL20-Order-sample-corrected.xml");
        }

        [Fact]
        public void OrderResponseSample()
        {
            AssertInvalidDocument<OrderResponseType>("KrampV21.UBL20-Orderconfirmation-sample.xml");
        }

        [Fact]
        public void OrderResponseSampleCorrected()
        {
            AssertValidDocument<OrderResponseType>("KrampV21.UBL20-Orderconfirmation-sample-corrected.xml");
        }
    }
}