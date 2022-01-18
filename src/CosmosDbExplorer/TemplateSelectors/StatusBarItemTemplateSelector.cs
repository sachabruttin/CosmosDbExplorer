using System.Windows;
using System.Windows.Controls;
using CosmosDbExplorer.Models;

namespace CosmosDbExplorer.TemplateSelectors
{
    public class StatusBarItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? UsedMemoryTemplate { get; set; }
        public DataTemplate? ZoomTemplate { get; set; }
        public DataTemplate? SimpleTextTemplate { get; set; }
        public DataTemplate? ProgressBarTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return ((StatusBarItem)item).Type switch
            {
                StatusBarItemType.UsedMemory => UsedMemoryTemplate,
                StatusBarItemType.Zoom => ZoomTemplate,
                StatusBarItemType.SimpleText => SimpleTextTemplate,
                StatusBarItemType.ProgessBar => ProgressBarTemplate,
                _ => base.SelectTemplate(item, container),
            };
        }
    }
}
