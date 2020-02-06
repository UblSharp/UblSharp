using FluentAssertions;
using UblSharp.SCSN;
using UblSharp.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace UblSharp.Tests.SCSN
{
    public class BillOfMaterialTests
    {
        private readonly ITestOutputHelper _output;

        public BillOfMaterialTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanDeserialize()
        {
            var seefV2Doc = ResourceHelper.GetResource("SCSN.Samples.20181207_BillOfMaterialsExample_v1.0_1.xml");

            var billOfMaterials = UblDocument.Load<BillOfMaterialsType>(seefV2Doc);

            billOfMaterials.Should().NotBeNull();
            billOfMaterials.ID.Value.Should().Be("123");
        }
    }
}
