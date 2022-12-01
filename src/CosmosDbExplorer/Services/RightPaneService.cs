﻿using System;
using System.Windows.Controls;
using System.Windows.Navigation;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;

using MahApps.Metro.Controls;

namespace CosmosDbExplorer.Services
{
    public class RightPaneService : IRightPaneService
    {
        private readonly IPageService _pageService;
        private Frame? _frame;
        private object? _lastParameterUsed;
        private SplitView? _splitView;

        public event EventHandler? PaneOpened;

        public event EventHandler? PaneClosed;

        public RightPaneService(IPageService pageService)
        {
            _pageService = pageService;
        }

        public void Initialize(Frame rightPaneFrame, SplitView splitView)
        {
            _frame = rightPaneFrame;
            _splitView = splitView;
            _frame.Navigated += OnNavigated;
            _splitView.PaneClosed += OnPaneClosed;
        }

        public void CleanUp()
        {
            if (_frame != null)
            {
                _frame.Navigated -= OnNavigated;
            }
            if (_splitView != null)
            {
                _splitView.PaneClosed -= OnPaneClosed;
            }
        }

        public void OpenInRightPane(Type pageKey, object parameter)
        {
            if (_frame is not null && _splitView is not null)
            {
                var pageType = _pageService.GetPageType(pageKey);
                if (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed)))
                {
                    var page = _pageService.GetPage(pageKey, parameter);
                    var navigated = _frame.Navigate(page, parameter);
                    if (navigated)
                    {
                        _lastParameterUsed = parameter;
                        var dataContext = _frame.GetDataContext();
                        if (dataContext is INavigationAware navigationAware)
                        {
                            navigationAware.OnNavigatedFrom();
                        }
                    }
                }

                _splitView.IsPaneOpen = true;
                PaneOpened?.Invoke(_splitView, EventArgs.Empty);
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (sender is Frame frame)
            {
                frame.CleanNavigation();
                var dataContext = frame.GetDataContext();
                if (dataContext is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedTo(e.ExtraData);
                }
            }
        }

        private void OnPaneClosed(object? sender, EventArgs e)
            => PaneClosed?.Invoke(sender, e);
    }
}
