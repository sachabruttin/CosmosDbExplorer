using System.Windows;
using System.Windows.Controls;
using CosmosDbExplorer.ViewModels;

namespace CosmosDbExplorer.Views.Pane
{
    public class PaneStyleSelector : StyleSelector
    {
        public Style? ToolStyle { get; set; }
        public Style? DocumentStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            return item switch
            {
                ToolViewModel => ToolStyle ?? base.SelectStyle(item, container),
                PaneViewModelBase => DocumentStyle ?? base.SelectStyle(item, container),
                _ => base.SelectStyle(item, container)
            };
        }
    }
}
