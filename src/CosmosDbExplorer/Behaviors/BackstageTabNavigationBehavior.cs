﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;

using Fluent;

using Microsoft.Xaml.Behaviors;

namespace CosmosDbExplorer.Behaviors
{
    public class BackstageTabNavigationBehavior : Behavior<BackstageTabControl>
    {
        private IPageService? _pageService;

        public BackstageTabNavigationBehavior()
        {
        }

        public void Initialize(IPageService pageService)
        {
            _pageService = pageService;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_pageService is null)
            {
                throw new NullReferenceException("Please call method Initialize() on the BackstageTabNavigationBehavior instance!");
            }

            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is BackstageTabItem oldItem)
            {
                var content = oldItem.Content as Frame;
                if (content?.Content is FrameworkElement element)
                {
                    if (element.DataContext is INavigationAware navigationAware)
                    {
                        navigationAware.OnNavigatedFrom();
                    }
                }
            }

            if (e.AddedItems.Count > 0 && e.AddedItems[0] is BackstageTabItem tabItem)
            {
                var frame = new Frame()
                {
                    Focusable = false,
                    NavigationUIVisibility = NavigationUIVisibility.Hidden
                };

                frame.Navigated += OnNavigated;
                tabItem.Content = frame;
                var page = _pageService.GetPage((Type)tabItem.Tag);
                frame.Navigate(page);
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is FrameworkElement element)
            {
                if (element.DataContext is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedTo(e.ExtraData);
                }
            }
        }
    }
}
