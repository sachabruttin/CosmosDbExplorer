using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace CosmosDbExplorer.ViewModels
{
    public class AboutViewModel : ObservableObject //, INavigationAware
    {
        private readonly Assembly _assembly;
        private readonly FileVersionInfo _fvi;

        public AboutViewModel()
        {
            _assembly = Assembly.GetEntryAssembly();
            _fvi = FileVersionInfo.GetVersionInfo(_assembly.Location);
            ExternalComponents = new List<ExternalComponent>
            {
                new ExternalComponent { Name = "AvalonEdit", LicenseUrl = "http://opensource.org/licenses/MIT", ProjectUrl = "http://www.avalonedit.net"},
                new ExternalComponent { Name = "Extended WPF Toolkit", LicenseUrl = "https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md", ProjectUrl = "https://github.com/xceedsoftware/wpftoolkit"},
                new ExternalComponent { Name = "Fluent.Ribbon", LicenseUrl = "https://github.com/fluentribbon/Fluent.Ribbon/blob/master/License.txt", ProjectUrl = "https://github.com/fluentribbon/Fluent.Ribbon"},
                new ExternalComponent { Name = "FluentValidation", LicenseUrl = "https://github.com/JeremySkinner/FluentValidation/blob/master/License.txt", ProjectUrl = "https://github.com/JeremySkinner/fluentvalidation"},
                new ExternalComponent { Name = "Fody", LicenseUrl = "http://www.opensource.org/licenses/mit-license.php", ProjectUrl = "http://github.com/Fody/Fody"},
                new ExternalComponent { Name = "MvvmLight", LicenseUrl = "http://www.galasoft.ch/license_MIT.txt", ProjectUrl = "http://www.galasoft.ch/mvvm"},
                new ExternalComponent { Name = "Json.NET", LicenseUrl = "https://raw.githubusercontent.com/JamesNK/Newtonsoft.Json/master/LICENSE.md", ProjectUrl = "https://www.newtonsoft.com/json"},
                new ExternalComponent { Name = "PropertyChanged.Fody", LicenseUrl = "http://www.opensource.org/licenses/mit-license.php", ProjectUrl = "http://github.com/Fody/PropertyChanged"},
                new ExternalComponent { Name = "Validar.Fody", LicenseUrl = "http://www.opensource.org/licenses/mit-license.php", ProjectUrl = "http://github.com/Fody/Validar"},
                new ExternalComponent { Name = "WpfAnimatedGif", LicenseUrl = "http://www.apache.org/licenses/LICENSE-2.0.txt", ProjectUrl = "https://github.com/XamlAnimatedGif/WpfAnimatedGif"},
                new ExternalComponent { Name = "GongSolutions.WPF.DragDrop", LicenseUrl = "https://github.com/punker76/gong-wpf-dragdrop#license", ProjectUrl = "https://github.com/punker76/gong-wpf-dragdrop"},
                new ExternalComponent { Name = "Autoupdater.NET.Official",  LicenseUrl = "https://github.com/ravibpatel/AutoUpdater.NET/blob/master/LICENSE", ProjectUrl = "https://github.com/ravibpatel/AutoUpdater.NET"},
                new ExternalComponent { Name = "LiveCharts",  LicenseUrl = "https://github.com/Live-Charts/Live-Charts/blob/master/LICENSE.TXT", ProjectUrl = "https://lvcharts.net/"},
            }.OrderBy(ec => ec.Name).ToList();
        }

        public string Version => _fvi.FileVersion;

        public string Title => ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyTitleAttribute), false))?.Title ?? "error retrieving assembly title";

        public List<Author> Authors => new List<Author> { new Author("Sacha Bruttin", "sachabruttin"), new Author("savbace", "savbace") };

        public string LicenseUrl => "https://github.com/sachabruttin/CosmosDbExplorer/blob/master/LICENSE";

        public string ProjectUrl => "https://www.bruttin.com/CosmosDbExplorer";

        public List<ExternalComponent> ExternalComponents { get; }
    }
}
