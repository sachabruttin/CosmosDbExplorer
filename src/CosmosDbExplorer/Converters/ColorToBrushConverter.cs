using System;
using System.Windows.Data;
using System.Windows.Media;

namespace CosmosDbExplorer.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return Binding.DoNothing;
            }

            if (value is not System.Drawing.Color)
            {
                throw new InvalidOperationException("Value must be a Color");
            }

            var drawingColor = (System.Drawing.Color)value;

            var color = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
            return !color.Equals(Colors.Transparent) ? new SolidColorBrush(color) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
