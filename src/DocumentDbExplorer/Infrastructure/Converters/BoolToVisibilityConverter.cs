using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CosmosDbExplorer.Infrastructure.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = (bool)value;

            if (bool.TryParse(parameter as string, out var inverted) && inverted)
            {
                visible = !visible;
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
