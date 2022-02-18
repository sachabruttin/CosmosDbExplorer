using System.Windows;
using System.Windows.Controls;
using CosmosDbExplorer.ViewModels.Assets;

namespace CosmosDbExplorer.TemplateSelectors
{
    public class StoredProcParameterTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? JsonDataTemplate { get; set; }
        public DataTemplate? FileDataTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return (StoredProcParameterKind)item switch
            {
                StoredProcParameterKind.File => FileDataTemplate,
                _ => JsonDataTemplate,
            };
        }
    }
}
