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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PropertyChanged;
using Validar;
using System.Windows;
using System.Windows.Controls;

namespace CosmosDbExplorer.ViewModels
{
    [InjectValidation]
    public class AccountSettingsViewModel : UIViewModelBase, INavigationAware
    {
        private CosmosConnection? _connection = default;
        private RelayCommand? _saveAccountCommand;
        private readonly IPersistAndRestoreService _persistAndRestoreService;

        public AccountSettingsViewModel(IPersistAndRestoreService persistAndRestoreService, IUIServices uiServices)
            : base(uiServices)
        {
            _persistAndRestoreService = persistAndRestoreService;
        }

        public static string Title => "Account Settings";
        public static object Icon => App.Current.FindResource("AddConnectionIcon");

        [OnChangedMethod(nameof(UpdateSaveCommandStatus))]
        public string? AccountEndpoint { get; set; }

        [OnChangedMethod(nameof(UpdateSaveCommandStatus))]
        public string? AccountSecret { get; set; }

        [OnChangedMethod(nameof(UpdateSaveCommandStatus))]
        public string? Label { get; set; }

        public ConnectionType ConnectionType { get; set; }

        public bool EnableEndpointDiscovery { get; set; }

        public Color? AccentColor { get; set; }

        public Action<bool?>? SetResult { get; set; }

        protected void OnAccentColorChanged()
        {
            if (AccentColor != null && AccentColor.Value.Equals(Colors.Transparent))
            {
                AccentColor = null;
            }
        }

        protected void UpdateSaveCommandStatus() => SaveAccountCommand.NotifyCanExecuteChanged();

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

        public RelayCommand SaveAccountCommand => _saveAccountCommand ??= new(SaveCommandExecute, SaveCommandCanExecute);

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

        public bool SaveCommandCanExecute() => string.IsNullOrEmpty(((IDataErrorInfo)this).Error);

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

    public static class PasswordBoxAssistant
    {
        public static readonly DependencyProperty BoundPassword =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxAssistant), new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static readonly DependencyProperty BindPassword = DependencyProperty.RegisterAttached(
            "BindPassword", typeof(bool), typeof(PasswordBoxAssistant), new PropertyMetadata(false, OnBindPasswordChanged));

        private static readonly DependencyProperty UpdatingPassword =
            DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordBoxAssistant), new PropertyMetadata(false));

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox box = d as PasswordBox;

            // only handle this event when the property is attached to a PasswordBox
            // and when the BindPassword attached property has been set to true
            if (d == null || !GetBindPassword(d))
            {
                return;
            }

            // avoid recursive updating by ignoring the box's changed event
            box.PasswordChanged -= HandlePasswordChanged;

            string newPassword = (string)e.NewValue;

            if (!GetUpdatingPassword(box))
            {
                box.Password = newPassword;
            }

            box.PasswordChanged += HandlePasswordChanged;
        }

        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            // when the BindPassword attached property is set on a PasswordBox,
            // start listening to its PasswordChanged event

            PasswordBox box = dp as PasswordBox;

            if (box == null)
            {
                return;
            }

            bool wasBound = (bool)(e.OldValue);
            bool needToBind = (bool)(e.NewValue);

            if (wasBound)
            {
                box.PasswordChanged -= HandlePasswordChanged;
            }

            if (needToBind)
            {
                box.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox box = sender as PasswordBox;

            // set a flag to indicate that we're updating the password
            SetUpdatingPassword(box, true);
            // push the new password into the BoundPassword property
            SetBoundPassword(box, box.Password);
            SetUpdatingPassword(box, false);
        }

        public static void SetBindPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(BindPassword, value);
        }

        public static bool GetBindPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(BindPassword);
        }

        public static string GetBoundPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(BoundPassword);
        }

        public static void SetBoundPassword(DependencyObject dp, string value)
        {
            dp.SetValue(BoundPassword, value);
        }

        private static bool GetUpdatingPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(UpdatingPassword);
        }

        private static void SetUpdatingPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(UpdatingPassword, value);
        }
    }
}
