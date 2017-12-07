using System;
using System.Windows.Data;
using DocumentDbExplorer.Infrastructure.Models;

namespace DocumentDbExplorer.Infrastructure.Converters
{
    public class ActiveDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ToolViewModel)
            {
                return Binding.DoNothing;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ToolViewModel)
            {
                return Binding.DoNothing;
            }

            return value;
        }
    }
}
