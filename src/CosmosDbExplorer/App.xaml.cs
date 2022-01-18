using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.Views;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.Services;
using CosmosDbExplorer.ViewModels;
using CosmosDbExplorer.Views;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer
{
    // For more inforation about application lifecyle events see https://docs.microsoft.com/dotnet/framework/wpf/app-development/application-management-overview

    // WPF UI elements use language en-US by default.
    // If you need to support other cultures make sure you add converters and review dates and numbers in your UI to ensure everything adapts correctly.
    // Tracking issue for improving this is https://github.com/dotnet/wpf/issues/1946
    public partial class App : Application
    {
        private IHost _host;

        public T GetService<T>()
            where T : class
            => _host.Services.GetService(typeof(T)) as T;

        public App()
        {
        }

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            AvalonEdit.AvalonSyntax.LoadHighlighting();
            var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            // For more information about .NET generic host see  https://docs.microsoft.com/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.0
            _host = Host.CreateDefaultBuilder(e.Args)
                    .ConfigureAppConfiguration(c =>
                    {
                        c.SetBasePath(appLocation);
                    })
                    .ConfigureServices(ConfigureServices)
                    .Build();

            await _host.StartAsync();
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            // TODO WTS: Register your services, viewmodels and pages here

            // App Host
            services.AddHostedService<ApplicationHostService>();

            // Activation Handlers

            // Core Services
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IMessenger, WeakReferenceMessenger>();

            // Services
            services.AddSingleton<IWindowManagerService, WindowManagerService>();
            services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
            services.AddSingleton<ISystemService, SystemService>();
            services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddSingleton<IUIServices, UIServices>();

            // Cosmos Services
            services.AddSingleton<ICosmosClientService, CosmosClientService>();
            services.AddTransient(provider =>
            {
                return new Func<CosmosConnection, CosmosDatabaseService>(connection =>
                                new CosmosDatabaseService(provider.GetRequiredService<ICosmosClientService>(), connection));
            });

            services.AddTransient(provider =>
            {
                return new Func<CosmosConnection, CosmosDatabase, CosmosContainerService>((connection, database) =>
                                new CosmosContainerService(provider.GetRequiredService<ICosmosClientService>(), connection, database));
            });

            services.AddTransient(provider =>
            {
                return new Func<CosmosConnection, CosmosDatabase, CosmosContainer, CosmosDocumentService>((connection, database, container) =>
                                new CosmosDocumentService(provider.GetRequiredService<ICosmosClientService>(), connection, database, container));
            });

            // Views and ViewModels
            services.AddTransient<IShellWindow, ShellWindow>();
            services.AddTransient<ShellViewModel>();

            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();

            services.AddTransient<AboutViewModel>();
            services.AddTransient<AboutPage>();

            services.AddTransient<IShellDialogWindow, ShellDialogWindow>();
            services.AddTransient<ShellDialogViewModel>();

            services.AddTransient<DatabaseViewModel>();
            services.AddTransient<DocumentsTabViewModel>();
            services.AddTransient<QueryEditorViewModel>();

            // Configuration
            services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
            services.AddSingleton<IRightPaneService, RightPaneService>();
        }

        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            _host = null;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // TODO WTS: Please log and handle the exception as appropriate to your scenario
            // For more info see https://docs.microsoft.com/dotnet/api/system.windows.application.dispatcherunhandledexception?view=netcore-3.0
        }

        public static List<CosmosConnection> Connections = Current.Properties["Connections"] as List<CosmosConnection>;
    }
}
