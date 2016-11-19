using System.Collections.Generic;

namespace UblSharp.Validation
{
    public static class BaseDocumentExtensions
    {
        public static bool IsValid(this BaseDocument document, bool warningsAsErrors = false)
        {
            return UblDocumentValidator.Default.IsValid(document, warningsAsErrors);
        }

        public static IEnumerable<UblDocumentValidationError> Validate(this BaseDocument document, bool suppressWarnings = false)
        {
            return UblDocumentValidator.Default.Validate(document, suppressWarnings);
        }
    }
}