using System.IO;
using FluentAssertions;
using UblSharp.SEeF;
using UblSharp.Tests.Util;
using Xunit;

namespace UblSharp.Tests.SEeF
{
    public class InvoiceExtensionTests
    {
        [Fact]
        public void CanDeserializeSEeF_V1()
        {
            var xmlSerializer = UblSharp.SEeF.XmlSerializerFactory.Default.GetSerializer(SEeFVersion.V1);

            var seefV2Doc = ResourceHelper.GetResource("SEeF.Samples.20160704_SEeF - Voorbeeldfactuur 001 - levering.xml");
            var invoice = UblDocument.Load<InvoiceType>(seefV2Doc);

            var extContent = invoice.UBLExtensions[0].ExtensionContent.OuterXml;
            using (var sr = new StringReader(extContent))
            {
                var seef = (SEEFExtensionWrapperType)xmlSerializer.Deserialize(sr);

                seef.UtilityConsumptionPoint.Should().NotBeEmpty();
                seef.UtilityConsumptionPoint[0].ID.Value.Should().Be("871687400001234567");
            }
        }

        [Fact]
        public void CanDeserializeSEeF_V2()
        {
            var xmlSerializer = UblSharp.SEeF.XmlSerializerFactory.Default.GetSerializer(SEeFVersion.V2);

            var seefV2Doc = ResourceHelper.GetResource("SEeF.Samples.20170713_SEeF - Voorbeeldfactuur 001 - levering.xml");
            var invoice = UblDocument.Load<InvoiceType>(seefV2Doc);

            var extContent = invoice.UBLExtensions[0].ExtensionContent.OuterXml;
            using (var sr = new StringReader(extContent))
            {
                var seef = (SEEFExtensionWrapperType)xmlSerializer.Deserialize(sr);

                seef.UtilityConsumptionPoint.Should().NotBeEmpty();
                seef.UtilityConsumptionPoint[0].ID.Value.Should().Be("871687400001234567");
            }
        }
    }
}
