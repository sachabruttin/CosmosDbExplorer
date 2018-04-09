using System;
using System.Globalization;
using System.Windows.Data;

namespace CosmosDbExplorer.Infrastructure.Converters
{
    public class BytesToGigaBytesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Math.Round((long)value / (1024 * 1024.0), 3);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
