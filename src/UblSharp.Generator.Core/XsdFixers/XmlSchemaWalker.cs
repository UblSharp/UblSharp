using System.Collections.Generic;
using System.Xml.Schema;

namespace UblSharp.Generator.XsdFixers
{
    // Super simple walker, very incomplete, no stack overflow protections
    public abstract class XmlSchemaWalker
    {
        private readonly Dictionary<XmlSchemaObject, object> _visited = new Dictionary<XmlSchemaObject, object>();

        public virtual void Visit(XmlSchema schema)
        {
            foreach (XmlSchemaObject schemaObject in schema.Items)
            {
                VisitObject(schemaObject);
            }
        }

        private void VisitObject(XmlSchemaObject schemaObject)
        {
            if (_visited.ContainsKey(schemaObject))
            {
                return;
            }

            _visited[schemaObject] = null;

            switch (schemaObject)
            {
                case XmlSchemaComplexType complexType:
                    VisitComplexType(complexType);
                    break;
                case XmlSchemaSimpleType simpleType:
                    VisitSimpleType(simpleType);
                    break;
                case XmlSchemaElement element:
                    VisitElement(element);
                    break;
            }
        }

        protected virtual void VisitComplexType(XmlSchemaComplexType complexType)
        {
            switch (complexType.Particle)
            {
                case XmlSchemaSequence seq:
                    foreach (XmlSchemaObject schemaObject in seq.Items)
                    {
                        VisitObject(schemaObject);
                    }
                    break;
                case XmlSchemaElement el:
                    VisitElement(el);
                    break;
            }
        }

        protected virtual void VisitSimpleType(XmlSchemaSimpleType simpleType)
        {
        }

        protected virtual void VisitElement(XmlSchemaElement element)
        {
            if (element.ElementSchemaType != null)
            {
                VisitObject(element.ElementSchemaType);
            }
        }
    }
}
