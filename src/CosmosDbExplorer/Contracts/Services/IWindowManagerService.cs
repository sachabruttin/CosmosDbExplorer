using System;
using System.Windows;

namespace CosmosDbExplorer.Contracts.Services
{
    public interface IWindowManagerService
    {
        Window MainWindow { get; }

        void OpenInNewWindow(Type pageKey, object? parameter = null);

        bool? OpenInDialog(Type pageKey, object? parameter = null);

        Window? GetWindow(Type pageKey);
    }
}
