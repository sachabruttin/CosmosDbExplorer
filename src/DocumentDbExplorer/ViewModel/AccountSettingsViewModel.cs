using System;
using System.ComponentModel;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;
using DocumentDbExplorer.Services;
using FluentValidation;
using GalaSoft.MvvmLight.Messaging;
using Validar;

namespace DocumentDbExplorer.ViewModel
{ 
    [InjectValidation]
    public class AccountSettingsViewModel : WindowViewModelBase
    {
        private RelayCommand _addAccountCommand;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private bool _useLocalEmulator;

        public AccountSettingsViewModel(IMessenger messenger, IDialogService dialogService, ISettingsService settingsService) : base(messenger)
        {
            _dialogService = dialogService;
            _settingsService = settingsService;
        }
        
        public string Title => "Account Settings";
        public string AccountEndpoint { get; set; }
        public string AccountSecret { get; set; }
        public string Label { get; set; }
        public ConnectionType ConnectionType { get; set; }

        public bool UseLocalEmulator
        {
            get
            {
                return _useLocalEmulator;
            }
            set
            {
                _useLocalEmulator = value;

                if (_useLocalEmulator)
                {
                    AccountEndpoint = Constants.Emulator.Endpoint.ToString();
                    AccountSecret = Constants.Emulator.Secret;
                }
                else
                {
                    AccountEndpoint = null;
                    AccountSecret = null;
                }

                RaisePropertyChanged(() => UseLocalEmulator);
            }
        }

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand AddAccountCommand
        {
            get
            {
                return _addAccountCommand
                    ?? (_addAccountCommand = new RelayCommand(
                        async x =>
                        {
                            var connection = new Connection(Label, new Uri(AccountEndpoint), AccountSecret, ConnectionType);
                            await _settingsService.SaveConnectionAsync(connection);
                            MessengerInstance.Send(new ConnectionSettingSavedMessage(connection));
                            Close();
                        },       
                        x => !((INotifyDataErrorInfo)this).HasErrors));
            }
        }
    }

    public class AccountSettingsViewModelValidator : AbstractValidator<AccountSettingsViewModel>
    {
        public AccountSettingsViewModelValidator()
        {
            RuleFor(x => x.AccountEndpoint).NotEmpty().When(x => !x.UseLocalEmulator);
            RuleFor(x => x.AccountSecret).NotEmpty().When(x => !x.UseLocalEmulator);
            RuleFor(x => x.Label).NotEmpty().When(x => !x.UseLocalEmulator);
        }
    }
}
