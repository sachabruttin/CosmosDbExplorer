using System.Windows;
using System.Windows.Controls;
using CosmosDbExplorer.ViewModel.Assets;

namespace CosmosDbExplorer.Infrastructure.TemplateSelectors
{
    public class StoredProcParameterTemplateSelector : DataTemplateSelector
    {
        public DataTemplate JsonDataTemplate { get; set; }
        public DataTemplate FileDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch ((StoredProcParameterKind)item)
            {
                case StoredProcParameterKind.Json:
                    return JsonDataTemplate;
                case StoredProcParameterKind.File:
                    return FileDataTemplate;
                default:
                    return base.SelectTemplate(item, container);
            }
        }
    }
}
