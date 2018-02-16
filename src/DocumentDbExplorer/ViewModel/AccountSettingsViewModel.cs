﻿using System;
using System.ComponentModel;
using System.Windows.Media;
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
        private Connection _connection;

        public AccountSettingsViewModel(IMessenger messenger, IDialogService dialogService, ISettingsService settingsService) : base(messenger)
        {
            _dialogService = dialogService;
            _settingsService = settingsService;
        }

        public void SetConnection(Connection connection)
        {
            _connection = connection;

            AccountEndpoint = _connection.DatabaseUri?.ToString();
            AccountSecret = _connection.AuthenticationKey;
            Label = _connection.Label;
            UseLocalEmulator = _connection.IsLocalEmulator();
            ConnectionType = _connection.ConnectionType;
            AccentColor = _connection.AccentColor;
        }

        public string Title => "Account Settings";
        public string AccountEndpoint { get; set; }
        public string AccountSecret { get; set; }
        public string Label { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public Color? AccentColor { get; set; }

        public void OnAccentColorChanged()
        {
            if (AccentColor != null && AccentColor.Value.Equals(Colors.Transparent))
            {
                AccentColor = null;
            }
        }

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
                            var connection = new Connection(_connection.Id, Label, new Uri(AccountEndpoint), AccountSecret, ConnectionType, AccentColor);
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
