using System.Windows;
using System.Windows.Controls;
using CosmosDbExplorer.Infrastructure.Models;

namespace CosmosDbExplorer.Infrastructure.TemplateSelectors
{
    public class StatusBarItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UsedMemoryTemplate { get; set; }
        public DataTemplate ZoomTemplate { get; set; }
        public DataTemplate SimpleTextTemplate { get; set; }
        public DataTemplate ProgressBarTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (((StatusBarItem)item).Type)
            {
                case StatusBarItemType.UsedMemory:
                    return UsedMemoryTemplate;
                case StatusBarItemType.Zoom:
                    return ZoomTemplate;
                case StatusBarItemType.SimpleText:
                    return SimpleTextTemplate;
                case StatusBarItemType.ProgessBar:
                    return ProgressBarTemplate;
                default:
                    return base.SelectTemplate(item, container);
            }
        }
    }
}
