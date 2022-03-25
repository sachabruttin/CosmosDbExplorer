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
            if (item is ToolViewModel)
            {
                return ToolStyle;
            }

            if (item is PaneViewModelBase)
            {
                return DocumentStyle;
            }

            return base.SelectStyle(item, container);
        }
    }
}
