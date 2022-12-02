using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.ViewModels;
using CosmosDbExplorer.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace CosmosDbExplorer.Services
{
    public class PageService : IPageService
    {
        private readonly Dictionary<Type, Type> _pages = new();
        private readonly IServiceProvider _serviceProvider;

        public PageService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Configure<SettingsViewModel, SettingsPage>();
            Configure<AboutViewModel, AboutPage>();
            Configure<AccountSettingsViewModel, AccountSettingsPage>();
            Configure<ContainerPropertyViewModel, ContainerPropertyPage>();
            Configure<DatabasePropertyViewModel, DatabasePropertyPage>();
        }

        public Type GetPageType(Type key)
        {
            Type? pageType;
            lock (_pages)
            {
                if (!_pages.TryGetValue(key, out pageType))
                {
                    throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
                }
            }

            return pageType;
        }

        public Page GetPage(Type key)
        {
            var pageType = GetPageType(key);
            return (Page)_serviceProvider.GetRequiredService(pageType);
        }

        public Page GetPage(Type key, object parameters)
        {
            var pageType = GetPageType(key);

            // Instantiate ViewModel with paramters
            var vm = ActivatorUtilities.CreateInstance(_serviceProvider, key, parameters) ;
            return (Page)ActivatorUtilities.CreateInstance(_serviceProvider, pageType, vm);
        }

        private void Configure<VM, V>()
            where VM : ObservableObject
            where V : Page
        {
            lock (_pages)
            {
                //var key = typeof(VM).FullName;
                var key = typeof(VM);   
                if (_pages.ContainsKey(key))
                {
                    throw new ArgumentException($"The key {key.FullName} is already configured in PageService");
                }

                var type = typeof(V);
                if (_pages.Any(p => p.Value == type))
                {
                    throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key.FullName}");
                }

                _pages.Add(key, type);
            }
        }
    }
}
