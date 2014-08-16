using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using EasyDOC.Model;
using Type = System.Type;

namespace EasyDOC.Pdf.Interface
{
    public class UnitConverter : TypeConverter
    {
        private readonly Dictionary<string, Unit> _dictionary = new Dictionary<string, Unit>
        {
            {"m", Unit.Meters},
            {"meter", Unit.Meters},
            {"stk", Unit.Units},
            {"un", Unit.Units},
            {"s", Unit.Units}
        };

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value != null)
            {
                var str = value.ToString().ToLower();
                if (_dictionary.ContainsKey(str))
                {
                    return _dictionary[str];
                }
            }

            return Unit.Other;
        }
    }
}