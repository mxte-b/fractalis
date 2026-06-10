using fractalis.Core.Numbers;
using System.ComponentModel;
using System.Globalization;

namespace fractalis.Core.Converters
{
    public class BigFloatConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is not string s) return base.ConvertFrom(context, culture, value);

            return BigFloat.TryParse(s, out var result) ? result : null;
        }
    }
}
