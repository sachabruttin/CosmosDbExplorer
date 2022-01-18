using System.Windows.Controls;

using CosmosDbExplorer.Behaviors;

using MahApps.Metro.Controls;

namespace CosmosDbExplorer.Contracts.Views
{
    public interface IShellWindow
    {
        //Frame GetNavigationFrame();

        void ShowWindow();

        void CloseWindow();

        Frame GetRightPaneFrame();

        SplitView GetSplitView();

        RibbonTabsBehavior GetRibbonTabsBehavior();
    }
}
