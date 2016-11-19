using System.Collections.Generic;
using UblSharp.CommonAggregateComponents;
using UblSharp.CommonExtensionComponents;
using UblSharp.UnqualifiedDataTypes;

namespace UblSharp
{
    public interface IBaseDocument
    {
        List<UBLExtensionType> UBLExtensions { get; set; }

        IdentifierType UBLVersionID { get; set; }

        IdentifierType CustomizationID { get; set; }

        IdentifierType ProfileID { get; set; }

        IdentifierType ProfileExecutionID { get; set; }

        IdentifierType ID { get; set; }

        IdentifierType UUID { get; set; }

        List<SignatureType> Signature { get; set; }
    }
}