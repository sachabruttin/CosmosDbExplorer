using System.Windows;
using System.Windows.Controls;
using CosmosDbExplorer.Services;

namespace CosmosDbExplorer.TemplateSelectors
{
    public class DocumentDescriptionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate PartitionTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            //var dd = (DocumentDescription)item;

            //if (dd.PartitionKey != null)
            //{
            //    return PartitionTemplate;
            //}
            return DefaultTemplate;
        }
    }
}
