using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.Infrastructure.Converters
{
    public class QueryMetricsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is QueryMetrics metrics)
            {
                return metrics.ToString();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
