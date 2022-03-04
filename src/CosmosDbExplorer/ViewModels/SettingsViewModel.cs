using System;
using System.Windows.Input;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.Properties;

using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels
{
    // TODO WTS: Change the URL for your privacy policy in the appsettings.json file, currently set to https://YourPrivacyUrlGoesHere
    public class SettingsViewModel : ObservableObject, INavigationAware
    {
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private readonly IThemeSelectorService _themeSelectorService;
        private RelayCommand? _resetSettingsCommand;

        public AppTheme Theme { get; set; }

        public DialogStyles DialogStyle
        {
            get { return Enum.Parse<DialogStyles>(Settings.Default.DialogService); }
            set { Settings.Default.DialogService = value.ToString(); }
        }

        public string VersionDescription { get; set; }

        public RelayCommand ResetSettingsCommand => _resetSettingsCommand ??= new(OnResetSettingsCommand);

        public SettingsViewModel(IPersistAndRestoreService persistAndRestoreService, IThemeSelectorService themeSelectorService, IApplicationInfoService applicationInfoService)
        {
            _persistAndRestoreService = persistAndRestoreService;
            _themeSelectorService = themeSelectorService;

            VersionDescription = $"{Properties.Resources.AppDisplayName} - {applicationInfoService.GetVersion()}";
            Theme = _themeSelectorService.GetCurrentTheme();
        }

        public void OnNavigatedTo(object parameter)
        {
        }

        public void OnNavigatedFrom()
        {
            _persistAndRestoreService.PersistData();
        }

        protected void OnThemeChanged()
        {
            _themeSelectorService.SetTheme(Theme);
        }

        private void OnResetSettingsCommand()
        {
            _persistAndRestoreService.ResetData();
            Theme = _themeSelectorService.GetCurrentTheme();
        }
    }
}
