using System;
using System.Globalization;
using System.Windows.Data;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.Infrastructure.Converters
{
    public class QueryMetricsToDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is QueryMetrics metrics)
            {
                return new TextDocument(metrics.ToString());
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
