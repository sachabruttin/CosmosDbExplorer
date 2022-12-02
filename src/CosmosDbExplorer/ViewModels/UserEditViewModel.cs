using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Models;
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
        private readonly IDialogService _dialogService;
        private readonly CosmosUserService _userService;

        private AsyncRelayCommand? _saveCommand;
        private AsyncRelayCommand? _deleteCommand;
        private RelayCommand? _discardCommand;

        public UserEditViewModel(IServiceProvider serviceProvider, IDialogService dialogService, IUIServices uiServices, string contentId, NodeContext<UserNodeViewModel> nodeContext)
            : base(uiServices, contentId, nodeContext)
        {
            _dialogService = dialogService;
            Header = "New User";
            Title = "User";
            IconSource = System.Windows.Application.Current.FindResource("UserIcon");

            if (nodeContext.Node is null || nodeContext.Connection is null || nodeContext.Container is null || nodeContext.Database is null )
            {
                throw new NullReferenceException("Node context is not correctly initialized!");
            }

            Node = nodeContext.Node;
            Connection = nodeContext.Connection;
            Header = Node.Name ?? "New User";
            Title = "User";
            AccentColor = Node.Parent.Parent.Parent.Connection.AccentColor;
            ToolTip = $"{Connection.Label}/{nodeContext.Database.Id}";

            _userService = ActivatorUtilities.CreateInstance<CosmosUserService>(serviceProvider, Connection, nodeContext.Database);
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

        public override Task InitializeAsync()
        {
            SetInformation();
            return Task.CompletedTask;
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

                Header = result.Items?.Id ?? string.Empty;
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
