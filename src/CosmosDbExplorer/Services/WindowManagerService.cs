using System;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Contracts.Views;

using MahApps.Metro.Controls;

using Microsoft.Extensions.DependencyInjection;

namespace CosmosDbExplorer.Services
{
    public class WindowManagerService : IWindowManagerService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPageService _pageService;
        private readonly IApplicationInfoService _applicationInfoService;

        public Window MainWindow
            => Application.Current.MainWindow;

        public WindowManagerService(IServiceProvider serviceProvider, IPageService pageService, IApplicationInfoService applicationInfoService)
        {
            _serviceProvider = serviceProvider;
            _pageService = pageService;
            _applicationInfoService = applicationInfoService;
        }

        public void OpenInNewWindow(Type key, object? parameter = null)
        {
            var window = GetWindow(key);
            if (window != null)
            {
                window.Activate();
            }
            else
            {
                window = new MetroWindow()
                {
                    Title = _applicationInfoService.GetTitle(),
                    Style = Application.Current.FindResource("CustomMetroWindow") as Style
                };
                var frame = new Frame()
                {
                    Focusable = false,
                    NavigationUIVisibility = NavigationUIVisibility.Hidden
                };

                window.Content = frame;
                var page = _pageService.GetPage(key);
                window.Closed += OnWindowClosed;
                window.Show();
                frame.Navigated += OnNavigated;
                var navigated = frame.Navigate(page, parameter);
            }
        }

        public bool? OpenInDialog(Type key, object? parameter = null)
        {
            var shellWindow = (Window)_serviceProvider.GetRequiredService<IShellDialogWindow>();
            var frame = ((IShellDialogWindow)shellWindow).GetDialogFrame();
            frame.Navigated += OnNavigated;
            shellWindow.Closed += OnWindowClosed;
            var page = _pageService.GetPage(key);
            var navigated = frame.Navigate(page, parameter);
            return shellWindow.ShowDialog();
        }

        public Window? GetWindow(Type key)
        {
            foreach (Window window in Application.Current.Windows)
            {
                var dataContext = window.GetDataContext();
                if (dataContext?.GetType() == key)
                {
                    return window;
                }
            }

            return null;
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (sender is Frame frame)
            {
                var dataContext = frame.GetDataContext();
                if (dataContext is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedTo(e.ExtraData);
                }
            }
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            if (sender is Window window)
            {
                if (window.Content is Frame frame)
                {
                    frame.Navigated -= OnNavigated;
                }

                window.Closed -= OnWindowClosed;
            }
        }
    }
}
