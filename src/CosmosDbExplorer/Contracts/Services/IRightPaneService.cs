using System;
using System.Windows.Controls;

using MahApps.Metro.Controls;

namespace CosmosDbExplorer.Contracts.Services
{
    public interface IRightPaneService
    {
        event EventHandler PaneOpened;

        event EventHandler PaneClosed;

        void OpenInRightPane(Type pageKey, object parameter);

        void Initialize(Frame rightPaneFrame, SplitView splitView);

        void CleanUp();
    }
}
