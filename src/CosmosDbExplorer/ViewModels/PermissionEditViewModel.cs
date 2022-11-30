using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

using PropertyChanged;

using Validar;

namespace CosmosDbExplorer.ViewModels
{
    [InjectValidation]
    public class PermissionEditViewModel : PaneViewModel<PermissionNodeViewModel>, IAssetTabCommand
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;
        private AsyncRelayCommand _saveCommand;
        private AsyncRelayCommand _deleteCommand;
        private RelayCommand _discardCommand;
        private RelayCommand _copyToClipboardCommand;
        private CosmosUserService _userService;

        public PermissionEditViewModel(IServiceProvider serviceProvider, IDialogService dialogService, IUIServices uiServices)
            : base(uiServices)
        {
            _serviceProvider = serviceProvider;
            _dialogService = dialogService;
            Header = "New Permission";
            Title = "Permission";
            IconSource = App.Current.FindResource("PermissionIcon");
            PropertyChanged += (s, e) =>
            {
                var properties = new[] { nameof(PermissionId), nameof(PermissionMode), nameof(Container), nameof(ResourcePartitionKey) };

                if (properties.Contains(e.PropertyName))
                {
                    IsDirty = IsEntityChanged();
                }

                OnIsDirtyChanged();
            };
        }

        public bool CanEditName { get; protected set; }
        public string? PermissionId { get; set; }
        public CosmosPermissionMode PermissionMode { get; set; }
        public string Container { get; set; }
        public string? ResourcePartitionKey { get; set; }

        public CosmosPermission Permission { get; protected set; }

        public bool IsEntityChanged()
        {
            if (Permission != null)
            {
                if (PermissionId != Permission.Id)
                {
                    return true;
                }

                if (PermissionMode != Permission.PermissionMode)
                {
                    return true;
                }

                if (Container != Permission.ResourceUri)
                {
                    return true;
                }

                if (ResourcePartitionKey != Permission.PartitionKey?.ToString())
                {
                    return true;
                }
            }

            return false;
        }

        private void SetInformation()
        {
            if (Permission is null)
            {
                throw new NullReferenceException("Permission should not be null");
            }

            PermissionId = Permission.Id;
            PermissionMode = Permission.PermissionMode;
            Container = Permission.ResourceUri;
            ResourcePartitionKey = Permission.PartitionKey;

            IsDirty = false;
        }

        public bool IsNewDocument => Node?.Permission?.SelfLink == null;

        public bool IsValid => string.IsNullOrEmpty(((IDataErrorInfo)this).Error);

        public override void Load(string contentId, NodeContext<PermissionNodeViewModel> nodeContext)
        {
            if (nodeContext.Node is null)
            {
                throw new NullReferenceException(nameof(nodeContext.Node));
            }

            if (nodeContext.Connection is null)
            {
                throw new NullReferenceException(nameof(nodeContext.Connection));
            }

            if (nodeContext.Database is null)
            {
                throw new NullReferenceException(nameof(nodeContext.Database));
            }

            ContentId = contentId;
            Node = nodeContext.Node;
            Permission = Node.Permission ?? new CosmosPermission();
            Header = Node.Name ?? "New Permission";
            Title = "Permission";
            AccentColor = Node.Parent.Parent.Parent.Parent.Connection.AccentColor;
            ToolTip = $"{nodeContext.Connection.Label}/{nodeContext.Database.Id}/{Node.Name}";

            Containers = new ObservableCollection<string>(Node.Parent.Parent.Parent.Children.OfType<ContainerNodeViewModel>().Select(c => c.Name));

            _userService = ActivatorUtilities.CreateInstance<CosmosUserService>(_serviceProvider, nodeContext.Connection, nodeContext.Database);
            SetInformation();
        }

        public PermissionNodeViewModel Node { get; protected set; }

        public ObservableCollection<string>? Containers { get; protected set; }

        public RelayCommand CopyToClipboardCommand => _copyToClipboardCommand ??= new(() => System.Windows.Clipboard.SetText(Permission?.Token), () => !string.IsNullOrEmpty(Permission?.Token));

        public ICommand DiscardCommand => _discardCommand ??= new(SetInformation, () => IsDirty);

        public ICommand SaveCommand => _saveCommand ??= new(SaveCommandExecute, () => IsDirty && IsValid);

        private async Task SaveCommandExecute()
        {
            CosmosPermission permission;
            if (IsNewDocument)
            {
                permission = new CosmosPermission
                {
                    Id = PermissionId,
                };
            }
            else
            {
                permission = Node.Permission;
            }

            permission.Id = PermissionId;
            permission.PermissionMode = PermissionMode;
            permission.PartitionKey = ResourcePartitionKey;

            try
            {
                var result = await _userService.SavePermissionAsync(Node.Parent.User, permission, Container, new System.Threading.CancellationToken());

                Header = result.Items.Id;
                Node.Permission = result.Items;
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
                    var result = await _userService.DeletePermissionAsync(Node.Parent.User, Node.Permission, new System.Threading.CancellationToken());
                    Node.Parent.RefreshCommand.Execute(null); // Send Message?
                    CloseCommand.Execute(null);
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowError(ex, "An error occured`!");
                }
            }

            await _dialogService.ShowQuestion($"Do you want to delete the permission '{PermissionId}' ?", "Delete Permission", deleteUser);
        }

        public bool IsDirty { get; protected set; }

        protected void OnIsDirtyChanged()
        {
            _saveCommand?.NotifyCanExecuteChanged();
            _deleteCommand?.NotifyCanExecuteChanged();
            _discardCommand?.NotifyCanExecuteChanged();
        }
    }

    public class PermissionEditViewModelValidator : AbstractValidator<PermissionEditViewModel>
    {
        public PermissionEditViewModelValidator()
        {
            RuleFor(x => x.PermissionId).NotEmpty();
            RuleFor(x => x.Container).NotEmpty();
            //.Matches(@"dbs\/(\w|\s)*\/colls\/(\w|\s)*").WithMessage("Must be in the format 'dbs/[db id]/colls/[container id]");
        }
    }
}
