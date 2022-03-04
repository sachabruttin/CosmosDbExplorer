using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;

using Validar;

namespace CosmosDbExplorer.ViewModels
{
    [InjectValidation]
    public class UserEditViewModel : PaneViewModel<UserNodeViewModel>, IAssetTabCommand
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;
        private AsyncRelayCommand _saveCommand;
        private AsyncRelayCommand _deleteCommand;
        private RelayCommand _discardCommand;
        private CosmosUserService _userService;

        public UserEditViewModel(IServiceProvider serviceProvider, IDialogService dialogService, IUIServices uiServices)
            : base(uiServices)
        {
            _serviceProvider = serviceProvider;
            _dialogService = dialogService;
            Header = "New User";
            Title = "User";
            IconSource = App.Current.FindResource("UserIcon");
        }

        public string? UserId { get; set; }

        public void OnUserIdChanged()
        {
            IsDirty = UserId != Node.User.Id;
        }

        private void SetInformation()
        {
            if (Node is null)
            {
                throw new NullReferenceException("Node should not be null");
            }

            UserId = Node.User.Id;

            IsDirty = false;
        }

        public bool IsNewDocument => Node?.User?.SelfLink == null;

        public bool IsValid => string.IsNullOrEmpty(((IDataErrorInfo)this).Error);

        public override void Load(string contentId, UserNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database, CosmosContainer? container)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (database is null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            ContentId = contentId;
            Node = node;
            Connection = connection;
            Header = node.Name ?? "New User";
            Title = "User";
            AccentColor = node.Parent.Parent.Parent.Connection.AccentColor;
            ToolTip = $"{connection.Label}/{database.Id}";

            _userService = ActivatorUtilities.CreateInstance<CosmosUserService>(_serviceProvider, connection, database);

            SetInformation();
        }

        protected CosmosConnection Connection { get; set; }

        public UserNodeViewModel Node { get; protected set; }

        public ICommand DiscardCommand => _discardCommand ??= new(SetInformation, () => IsDirty);

        public ICommand SaveCommand => _saveCommand ??= new(SaveCommandExecute, () => IsDirty && IsValid);

        private async Task SaveCommandExecute()
        {
            CosmosUser? user;
            if (IsNewDocument)
            {
                user = new CosmosUser { Id = UserId };
            }
            else
            {
                user = Node.User;
                user.Id = UserId;
            }

            try
            {
                var result = await _userService.SaveUserAsync(user, new System.Threading.CancellationToken());

                Header = result.Items.Id ?? string.Empty;
                Node.User = user;
                ContentId = Node.ContentId;

                OnPropertyChanged(nameof(IsNewDocument));
                Node.Parent.RefreshCommand.Execute(null);
                IsDirty = false;
            }
            catch (Exception ex)
            {
                await _dialogService.ShowError(ex, "Error");
            }
        }

        public ICommand DeleteCommand => _deleteCommand ??= new(DeleteCommandExecute, () => !IsNewDocument);

        private async Task DeleteCommandExecute()
        {
            async void deleteUser(bool confirm) 
            {
                if (!confirm)
                {
                    return;
                }

                try
                {
                    var result = await _userService.DeleteUserAsync(Node.User, new System.Threading.CancellationToken());
                    Node.Parent.RefreshCommand.Execute(null); // Send Message?
                    CloseCommand.Execute(null);
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowError(ex, "An error occured`!");
                }
            }

            await _dialogService.ShowQuestion($"Do you want to delete the user '{Node.User.Id}' ?", "Delete User", deleteUser);
        }

        public bool IsDirty { get; private set; }

        protected void OnIsDirtyChanged()
        {
            _saveCommand?.NotifyCanExecuteChanged();
            _deleteCommand?.NotifyCanExecuteChanged();
            _discardCommand?.NotifyCanExecuteChanged();
        }
    }

    public class UserEditViewModelValidator : AbstractValidator<UserEditViewModel>
    {
        public UserEditViewModelValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
