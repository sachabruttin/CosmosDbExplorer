using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Properties;
using FluentValidation;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Validar;

namespace CosmosDbExplorer.ViewModels
{
    [InjectValidation]
    public class AccountSettingsViewModel : ObservableRecipient, INavigationAware
    {
        private CosmosConnection? _connection;

        public AccountSettingsViewModel()
        {

        }

        public string Title => "Account Settings";
        public object Icon => App.Current.FindResource("AddConnectionIcon");

        public string? AccountEndpoint { get; set; }
        public string? AccountSecret { get; set; }
        public string Label { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public bool EnableEndpointDiscovery { get; set; }
        public Color? AccentColor { get; set; }

        public void OnAccentColorChanged()
        {
            if (AccentColor != null && AccentColor.Value.Equals(Colors.Transparent))
            {
                AccentColor = null;
            }
        }

        public bool UseLocalEmulator { get; set; }

        public void OnUseLocalEmulatorChanged()
        {
            if (UseLocalEmulator)
            {
                AccountEndpoint = Resources.EmulatorEndpoint;
                AccountSecret = Resources.EmulatorSecret;
            }
            else
            {
                AccountEndpoint = null;
                AccountSecret = null;
            }

            OnPropertyChanged(nameof(UseLocalEmulator));
        }

        public RelayCommand AddAccountCommand => new(AddAccountCommandExecute, AddAccountCommandCanExecute);

        public void AddAccountCommandExecute()
        { 
        }

        public bool AddAccountCommandCanExecute() => string.IsNullOrEmpty(((IDataErrorInfo)this).Error); //!((INotifyDataErrorInfo)this).HasErrors;

        public void OnNavigatedTo(object parameter)
        {
            SetConnection((CosmosConnection)parameter);
        }

        public void OnNavigatedFrom()
        {
           
        }

        private void SetConnection(CosmosConnection connection)
        {
            _connection = connection;

            AccountEndpoint = _connection.DatabaseUri?.ToString();
            AccountSecret = _connection.AuthenticationKey;
            Label = _connection.Label;
            UseLocalEmulator = _connection.IsLocalEmulator();
            ConnectionType = _connection.ConnectionType;

            if (connection.AccentColor is not null)
            {
                AccentColor = Color.FromArgb(_connection.AccentColor.Value.A, _connection.AccentColor.Value.R, _connection.AccentColor.Value.G, _connection.AccentColor.Value.B);
            }

            EnableEndpointDiscovery = _connection.EnableEndpointDiscovery;
        }
    }

    public class AccountSettingsViewModelValidator : AbstractValidator<AccountSettingsViewModel>
    {
        public AccountSettingsViewModelValidator()
        {
            RuleFor(x => x.AccountEndpoint).NotEmpty().When(x => !x.UseLocalEmulator);
            RuleFor(x => x.AccountSecret).NotEmpty().When(x => !x.UseLocalEmulator);
            RuleFor(x => x.Label).NotEmpty();
        }
    }
}
