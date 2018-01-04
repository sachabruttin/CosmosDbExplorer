using System.Windows;
using System.Windows.Controls;
using DocumentDbExplorer.Infrastructure.Models;

namespace DocumentDbExplorer.Infrastructure.TemplateSelectors
{
    public class StatusBarItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UsedMemoryTemplate { get; set; }
        public DataTemplate ZoomTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (((StatusBarItem)item).Type)
            {
                case StatusBarItemType.UsedMemory:
                    return UsedMemoryTemplate;
                case StatusBarItemType.Zoom:
                    return ZoomTemplate;
                default:
                    return base.SelectTemplate(item, container);
            }
        }
    }
}
