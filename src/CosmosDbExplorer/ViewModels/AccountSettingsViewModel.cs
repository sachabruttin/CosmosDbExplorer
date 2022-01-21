using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.Properties;
using FluentValidation;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using PropertyChanged;
using Validar;

namespace CosmosDbExplorer.ViewModels
{
    [InjectValidation]
    public class AccountSettingsViewModel : ObservableRecipient, INavigationAware
    {
        private CosmosConnection? _connection = default;
        private readonly IPersistAndRestoreService _persistAndRestoreService;

        public AccountSettingsViewModel(IPersistAndRestoreService persistAndRestoreService)
        {
            _persistAndRestoreService = persistAndRestoreService;
        }

        public string Title => "Account Settings";
        public object Icon => App.Current.FindResource("AddConnectionIcon");


        public bool IsBusy { get; set; } = false;
        public string? AccountEndpoint { get; set; }
        public string? AccountSecret { get; set; }
        public string? Label { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public bool EnableEndpointDiscovery { get; set; }
        public Color? AccentColor { get; set; } 
        public Action<bool?>? SetResult { get; set; }

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
        }

        [DependsOn(nameof(AccountEndpoint), 
            nameof(AccountSecret), 
            nameof(Label))]
        public RelayCommand SaveAccountCommand => new(SaveCommandExecute, SaveCommandCanExecute);

        public void SaveCommandExecute()
        {
            if (_connection == null || AccountEndpoint == null)
            {
                return;
            }

            IsBusy = true;

            var accentColor = AccentColor is null 
                ? System.Drawing.Color.Transparent
                : System.Drawing.Color.FromArgb(AccentColor.Value.A, AccentColor.Value.R, AccentColor.Value.G, AccentColor.Value.B);

            var connection = new CosmosConnection(_connection.Id, Label, new Uri(AccountEndpoint), AccountSecret, ConnectionType, EnableEndpointDiscovery, accentColor);

            _persistAndRestoreService.PersistConnection(connection);
            Messenger.Send(new ConnectionSettingSavedMessage(connection));
            OnClose();

            IsBusy = false;
        }

        public bool SaveCommandCanExecute() => string.IsNullOrEmpty(((IDataErrorInfo)this).Error); //!((INotifyDataErrorInfo)this).HasErrors;

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

            AccountEndpoint = connection.DatabaseUri?.ToString();
            AccountSecret = connection.AuthenticationKey;
            Label = connection.Label;
            UseLocalEmulator = connection.IsLocalEmulator();
            ConnectionType = connection.ConnectionType;

            if (connection.AccentColor is not null)
            {
                AccentColor = Color.FromArgb(connection.AccentColor.Value.A, connection.AccentColor.Value.R, connection.AccentColor.Value.G, connection.AccentColor.Value.B);
            }

            EnableEndpointDiscovery = _connection.EnableEndpointDiscovery;
        }

        private void OnClose()
        {
            SetResult?.Invoke(true);
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
