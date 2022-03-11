using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Extensions;

namespace CosmosDbExplorer.Controls
{
    public class TreeViewEx : TreeView
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItemEx();
        }
    }

    public class TreeViewItemEx : TreeViewItem
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItemEx();
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as UIElement)?.GetAncestorOrSelf<TreeViewItemEx>();

            if (item != this)
            {
                return;
            }

            e.Handled = ExecuteCommand();
            base.OnMouseDoubleClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Enter && ExecuteCommand();
            base.OnKeyDown(e);
        }

        private bool ExecuteCommand()
        {
            if (DataContext is IHaveOpenCommand vm)
            {
                if (vm.OpenCommand != null && vm.OpenCommand.CanExecute(null))
                {
                    vm.OpenCommand.Execute(null);
                    return true;
                }
            }

            return false;
        }
    }
}
