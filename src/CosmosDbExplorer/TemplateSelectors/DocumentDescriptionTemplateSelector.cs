using System.Windows;
using System.Windows.Controls;
using CosmosDbExplorer.Core.Contracts;

namespace CosmosDbExplorer.TemplateSelectors
{
    public class DocumentDescriptionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate PartitionTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var dd = (ICosmosDocument)item;

            if (!string.IsNullOrEmpty(dd.PartitionKey))
            {
                return PartitionTemplate;
            }

            return DefaultTemplate;
        }
    }
}
