using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace UblSharp.UnqualifiedDataTypes
{
    public partial class TimeType
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [XmlText]
        public string ValueString
        {
            get
            {
                var val = XmlConvert.ToString(Value).Split('T');
                return val[val.Length - 1];
            }
            set
            {
                Value = XmlConvert.ToDateTimeOffset(value);
            }
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="TimeType"/> to <see cref="DateTime"/> in local time.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator DateTime(TimeType value)
        {
            return value.Value.LocalDateTime;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="DateTime"/> to <see cref="TimeType"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator TimeType(DateTime value)
        {
            return new TimeType { Value = value };
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="TimeType"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator TimeType(string value)
        {
            return string.IsNullOrEmpty(value) ? null : new TimeType
            {
                ValueString = value
            };
        }
    }
}