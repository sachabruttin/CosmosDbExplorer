using System;
using System.Globalization;
using System.Windows.Data;
using CosmosDbExplorer.ViewModels;

namespace CosmosDbExplorer.Converters
{
    public class ActiveDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ToolViewModel)
            {
                return Binding.DoNothing;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ToolViewModel)
            {
                return Binding.DoNothing;
            }

            return value;
        }
    }
}
