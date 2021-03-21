using System;
using System.ComponentModel;
using System.Globalization;
using NodaTime;
using NodaTime.Text;

namespace Demo.Api.Infrastructure
{
    public class InstantTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return InstantPattern.General.Parse(value.ToString()).Value;
        }
    }

}
