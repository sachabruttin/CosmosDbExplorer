using System;
using System.Windows.Data;
using System.Windows.Media;

namespace CosmosDbExplorer.Infrastructure.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return Binding.DoNothing;
            }

            if (!(value is Color))
            {
                throw new InvalidOperationException("Value must be a Color");
            }

            var color = value;

            return !color.Equals(Colors.Transparent) ? new SolidColorBrush((Color)value) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
