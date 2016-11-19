using System.Xml.Schema;

namespace UblSharp.Validation
{
    public class UblDocumentValidationError
    {
        public UblDocumentValidationError(XmlSchemaException exception, string message, XmlSeverityType severity)
        {
            Severity = severity;
            Message = message;
            Exception = exception;
        }

        public XmlSchemaException Exception { get; private set; }

        public string Message { get; private set; }

        public XmlSeverityType Severity { get; private set; }
    }
}