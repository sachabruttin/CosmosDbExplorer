using System.Windows;
using System.Windows.Controls;
using DocumentDbExplorer.Infrastructure.Models;

namespace DocumentDbExplorer.Views.Pane
{
    public class PaneStyleSelector : StyleSelector
    {
        public Style ToolStyle { get; set; }
        public Style DocumentStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is ToolViewModel)
            {
                return ToolStyle;
            }

            if (item is PaneViewModel)
            {
                return DocumentStyle;
            }

            return base.SelectStyle(item, container);
        }
    }
}
