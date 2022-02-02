using System.Windows;
using System.Windows.Controls;
using CosmosDbExplorer.Core.Contracts;
using CosmosDbExplorer.Models;

namespace CosmosDbExplorer.TemplateSelectors
{
    public class DocumentDescriptionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? DefaultTemplate { get; set; }
        public DataTemplate? PartitionTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            var dd = ((CheckedItem<ICosmosDocument>)item).Item;

            return dd.HasPartitionKey
                ? PartitionTemplate
                : DefaultTemplate;
        }
    }
}
