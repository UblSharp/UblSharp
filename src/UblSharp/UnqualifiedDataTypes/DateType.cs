using System;
using System.Xml;

namespace UblSharp.UnqualifiedDataTypes
{
    public partial class TimeType
    {
        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="TimeType"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator TimeType(string value)
        {
            return string.IsNullOrEmpty(value) ? null : new TimeType { Value = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind) };
        }
    }

    public partial class DateType
    {
        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="DateType"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator DateType(string value)
        {
            return string.IsNullOrEmpty(value) ? null : new DateType { Value = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind) };
        }
    }
}