using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using CosmosDbExplorer.Properties;
using CosmosDbExplorer.Services;
using CosmosDbExplorer.ViewModels;
using CosmosDbExplorer.Views;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommunityToolkit.Mvvm.Messaging;

namespace CosmosDbExplorer
{
    // For more inforation about application lifecyle events see https://docs.microsoft.com/dotnet/framework/wpf/app-development/application-management-overview

    // WPF UI elements use language en-US by default.
    // If you need to support other cultures make sure you add converters and review dates and numbers in your UI to ensure everything adapts correctly.
    // Tracking issue for improving this is https://github.com/dotnet/wpf/issues/1946
    public partial class App : Application
    {
        private IHost? _host;

        public T? GetService<T>()
            where T : class
            => _host?.Services.GetService(typeof(T)) as T;

        public App()
        {
        }

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            AvalonEdit.AvalonSyntax.LoadHighlighting();
            var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

            // Upgrade user settings in case of assembly version change
            if (!System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal).HasFile)
            {
                Settings.Default.Upgrade();
            }

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
            services.AddSingleton<IDialogService, MetroDialogService>();

            services.AddSingleton<IDialogService>(GetDialogService());

            services.AddSingleton<IUIServices, UIServices>();

            // Cosmos Services
            services.AddSingleton<ICosmosClientService, CosmosClientService>();

            services.AddTransient<CosmosDatabaseService>();
            services.AddTransient<CosmosContainerService>();
            services.AddTransient<CosmosDocumentService>();
            services.AddTransient<CosmosScriptService>();

            // Views and ViewModels
            services.AddTransient<IShellWindow, ShellWindow>();
            services.AddTransient<ShellViewModel>();

            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();

            services.AddTransient<AccountSettingsViewModel>();
            services.AddTransient<AccountSettingsPage>();

            services.AddTransient<ContainerPropertyViewModel>();
            services.AddTransient<ContainerPropertyPage>();

            services.AddTransient<DatabasePropertyViewModel>();
            services.AddTransient<DatabasePropertyPage>();

            services.AddTransient<AboutViewModel>();
            services.AddTransient<AboutPage>();

            services.AddTransient<IShellDialogWindow, ShellDialogWindow>();
            services.AddTransient<ShellDialogViewModel>();

            services.AddTransient<DatabaseViewModel>();
            services.AddTransient<DocumentsTabViewModel>();
            services.AddTransient<QueryEditorViewModel>();
            services.AddTransient<MetricsTabViewModel>();
            services.AddTransient<ImportDocumentViewModel>();
            services.AddTransient<DatabaseScaleViewModel>();
            services.AddTransient<ContainerScaleSettingsViewModel>();
            services.AddTransient<UserEditViewModel>();
            services.AddTransient<PermissionEditViewModel>();

            services.AddTransient<ViewModels.Assets.TriggerTabViewModel>();
            services.AddTransient<ViewModels.DatabaseNodes.TriggerNodeViewModel>();
            services.AddTransient<ViewModels.Assets.StoredProcedureTabViewModel>();
            services.AddTransient<ViewModels.DatabaseNodes.StoredProcedureNodeViewModel>();
            services.AddTransient<ViewModels.Assets.UserDefFuncTabViewModel>();
            services.AddTransient<ViewModels.DatabaseNodes.UserDefFuncNodeViewModel>();

            // Configuration
            services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
            services.AddSingleton<IRightPaneService, RightPaneService>();
        }

        private async void OnExit(object sender, ExitEventArgs e)
        {
            if (_host is not null)
            {
                await _host.StopAsync();
                _host.Dispose();
                _host = null;
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // TODO WTS: Please log and handle the exception as appropriate to your scenario
            // For more info see https://docs.microsoft.com/dotnet/api/system.windows.application.dispatcherunhandledexception?view=netcore-3.0
            if (Debugger.IsAttached)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
                ShowUnhandledException(e.Exception, false);
            }
        }

        private static void ShowUnhandledException(Exception exception, bool isTerminating)
        {
            var details = exception.Message + (exception.InnerException != null ? "\n" + exception.InnerException.Message : null);
            var errorMessage = $@"An application error occurred.
Please check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it.

Error: {details}";

            if (!isTerminating)
            {
                errorMessage += "\nDo you want to continue?";

                if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
                {
                    if (MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        Application.Current.Shutdown();
                    }
                }
            }
            else
            {
                MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static IDialogService GetDialogService()
        {
            return Settings.Default.DialogService switch
            {
                "Metro" => new MetroDialogService(),
                _ => new DialogService(),
            };
        }
    }
}
