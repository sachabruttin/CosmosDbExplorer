using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels
{
    public class AboutViewModel : ObservableObject, INavigationAware
    {
        private readonly ISystemService _systemService;
        private RelayCommand<string>? _openLinkCommand;
        private RelayCommand<string>? _openGitHubCommand;

        public AboutViewModel(ISystemService systemService)
        {
            var assembly = Assembly.GetEntryAssembly();

            if (assembly == null)
            {
                throw new NullReferenceException("Can not load entry assembly");
            }

            Version = FileVersionInfo.GetVersionInfo(assembly.Location)?.FileVersion ?? "version not found";
            Title = (Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute), false) as AssemblyTitleAttribute)?.Title ?? "error retrieving assembly title";

            ExternalComponents = new List<ExternalComponent>
            {
                new ExternalComponent { Name = "AvalonEdit", LicenseUrl = "http://opensource.org/licenses/MIT", ProjectUrl = "http://www.avalonedit.net"},
                //new ExternalComponent { Name = "Extended WPF Toolkit", LicenseUrl = "https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md", ProjectUrl = "https://github.com/xceedsoftware/wpftoolkit"},
                new ExternalComponent { Name = "Fluent.Ribbon", LicenseUrl = "https://github.com/fluentribbon/Fluent.Ribbon/blob/master/License.txt", ProjectUrl = "https://github.com/fluentribbon/Fluent.Ribbon"},
                new ExternalComponent { Name = "FluentValidation", LicenseUrl = "https://github.com/JeremySkinner/FluentValidation/blob/master/License.txt", ProjectUrl = "https://github.com/JeremySkinner/fluentvalidation"},
                new ExternalComponent { Name = "Fody", LicenseUrl = "http://www.opensource.org/licenses/mit-license.php", ProjectUrl = "http://github.com/Fody/Fody"},
                //new ExternalComponent { Name = "Json.NET", LicenseUrl = "https://raw.githubusercontent.com/JamesNK/Newtonsoft.Json/master/LICENSE.md", ProjectUrl = "https://www.newtonsoft.com/json"},
                new ExternalComponent { Name = "PropertyChanged.Fody", LicenseUrl = "http://www.opensource.org/licenses/mit-license.php", ProjectUrl = "http://github.com/Fody/PropertyChanged"},
                new ExternalComponent { Name = "Validar.Fody", LicenseUrl = "http://www.opensource.org/licenses/mit-license.php", ProjectUrl = "http://github.com/Fody/Validar"},
                new ExternalComponent { Name = "GongSolutions.WPF.DragDrop", LicenseUrl = "https://github.com/punker76/gong-wpf-dragdrop#license", ProjectUrl = "https://github.com/punker76/gong-wpf-dragdrop"},
                new ExternalComponent { Name = "Autoupdater.NET.Official",  LicenseUrl = "https://github.com/ravibpatel/AutoUpdater.NET/blob/master/LICENSE", ProjectUrl = "https://github.com/ravibpatel/AutoUpdater.NET"},
                //new ExternalComponent { Name = "LiveCharts",  LicenseUrl = "https://github.com/Live-Charts/Live-Charts/blob/master/LICENSE.TXT", ProjectUrl = "https://lvcharts.net/"},
            }.OrderBy(ec => ec.Name).ToList();
            _systemService = systemService;
        }

        public string Version { get; set; }

        public string Title { get; set; }

        public static List<Author> Authors => new() 
        { 
            new Author("Sacha Bruttin", "sachabruttin"), 
            new Author("savbace", "savbace"),
            new Author("Curlack", "Curlack")
        };

        public static string LicenseUrl => "https://github.com/sachabruttin/CosmosDbExplorer/blob/master/LICENSE";

        public static string ProjectUrl => "https://www.bruttin.com/CosmosDbExplorer";

        public List<ExternalComponent> ExternalComponents { get; }

        public RelayCommand<string> OpenLinkCommand => _openLinkCommand ??= new((url) => _systemService?.OpenInWebBrowser(url));

        public RelayCommand<string> OpenGitHubCommand => _openGitHubCommand ??= new((url) => _systemService?.OpenInWebBrowser($"https://github.com/{url}"));


        public void OnNavigatedFrom()
        {
        }

        public void OnNavigatedTo(object parameter)
        {
        }
    }
}
