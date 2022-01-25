using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CosmosDbExplorer.Converters
{
    public class StringToFontFamillyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return value;
            }

            var con = new FontFamilyConverter();
            var ff = con.ConvertFromString((string)value);
            return ff;
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }
    }
}
