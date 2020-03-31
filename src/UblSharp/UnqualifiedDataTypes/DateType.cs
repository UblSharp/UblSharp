using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace UblSharp.UnqualifiedDataTypes
{
    public partial class DateType
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [XmlText]
        public string ValueString
        {
            get
            {
                return XmlConvert.ToString(Value, "yyyy-MM-dd");
            }
            set
            {
                try
                {
                    Value = XmlConvert.ToDateTimeOffset(value);

                }
                catch (ArgumentOutOfRangeException)
                {
                    Value = DateTimeOffset.MinValue;
                }
            }
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="DateType"/> to <see cref="DateTime"/> in local time.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator DateTime(DateType value)
        {
            return value.Value.LocalDateTime;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="DateTime"/> to <see cref="DateType"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator DateType(DateTime value)
        {
            return new DateType { Value = value };
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="DateType"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator DateType(string value)
        {
            return string.IsNullOrEmpty(value) ? null : new DateType
            {
                ValueString = value
            };
        }
    }
}
