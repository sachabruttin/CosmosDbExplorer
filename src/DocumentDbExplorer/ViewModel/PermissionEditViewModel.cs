using System.ComponentModel;
using System.Windows;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Extensions;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Services;
using DocumentDbExplorer.ViewModel.Interfaces;
using FluentValidation;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;
using Validar;

namespace DocumentDbExplorer.ViewModel
{
    [InjectValidation]
    public class PermissionEditViewModel : PaneViewModel<PermissionNodeViewModel>, IAssetTabCommand
    {
        private readonly IDocumentDbService _dbService;
        private readonly IDialogService _dialogService;
        private RelayCommand _saveCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _discardCommand;
        private RelayCommand _copyToClipboardCommand;

        public PermissionEditViewModel(IMessenger messenger, IDocumentDbService dbService, IDialogService dialogService) : base(messenger)
        {
            _dbService = dbService;
            _dialogService = dialogService;
            Header = "New Permission";
            Title = "Permission";
            PropertyChanged += (s, e) => IsDirty = IsEntityChanged();
        }

        public string PermissionId { get; set; }
        public PermissionMode PermissionMode { get; set; }
        public string ResourceLink { get; set; }
        public string ResourcePartitionKey { get; set; }

        public Permission Permission { get; protected set; }

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

                if (ResourceLink != Permission.ResourceLink)
                {
                    return true;
                }

                if (ResourcePartitionKey != Permission.ResourcePartitionKey?.ToString())
                {
                    return true;
                }
            }

            return false;
        }

        private void SetInformation()
        {
            PermissionId = Permission?.Id;
            PermissionMode = Permission?.PermissionMode ?? PermissionMode.Read;
            ResourceLink = Permission?.ResourceLink;
            ResourcePartitionKey = Permission?.ResourcePartitionKey?.ToString();

            var split = Node.Parent.User.AltLink.Split(new char[] { '/' });
            ToolTip = $"{split[1]}>{split[3]}";

            IsDirty = false;
        }

        public bool IsNewDocument => Node?.Permission?.SelfLink == null;

        public bool IsValid => !((INotifyDataErrorInfo)this).HasErrors;

        public override void Load(string contentId, PermissionNodeViewModel node, Connection connection, DocumentCollection collection)
        {
            ContentId = contentId;
            Node = node;
            Permission = node?.Permission ?? new Permission();
            Header = node.Name ?? "New Permission";
            Title = "Permission";
            AccentColor = Node.Parent.Parent.Parent.Parent.Connection.AccentColor;
            SetInformation();
        }

        public PermissionNodeViewModel Node { get; protected set; }

        public RelayCommand CopyToClipboardCommand
        {
            get
            {
                return _copyToClipboardCommand
                    ?? (_copyToClipboardCommand = new RelayCommand(
                        () => Clipboard.SetText(Permission?.Token)));
            }
        }

        public RelayCommand DiscardCommand
        {
            get
            {
                return _discardCommand
                    ?? (_discardCommand = new RelayCommand(SetInformation, () => IsDirty));
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand
                    ?? (_saveCommand = new RelayCommand(
                        async () =>
                        {
                            Permission permission = null;

                            if (IsNewDocument)
                            {
                                permission = new Permission
                                {
                                    Id = PermissionId,
                                };
                            }
                            else
                            {
                                permission = Node.Permission;
                            }

                            permission.Id = PermissionId;
                            permission.ResourceLink = ResourceLink;
                            permission.PermissionMode = PermissionMode;
                            permission.ResourcePartitionKey = ResourcePartitionKey != null
                                                        ? new PartitionKey(ResourcePartitionKey)
                                                        : null;

                            try
                            {
                                permission = await _dbService.SavePermissionAsync(Node.Parent.Parent.Parent.Parent.Connection, Node.Parent.User, permission).ConfigureAwait(false);

                                Header = permission.Id;
                                Node.Permission = permission;
                                ContentId = Node.ContentId;

                                RaisePropertyChanged(() => IsNewDocument);
                                Node.Parent.RefreshCommand.Execute(null);
                                IsDirty = false;
                            }
                            catch (DocumentClientException ex)
                            {
                                var msg = ex.Parse();
                                await _dialogService.ShowError(msg, "Error", null, null).ConfigureAwait(false);
                            }
                        },
                        () => IsDirty && IsValid));
            }
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return _deleteCommand
                    ?? (_deleteCommand = new RelayCommand(
                        async () =>
                        {
                            await _dialogService.ShowMessage("Are you sure...", "Delete", null, null, async confirm =>
                            {
                                if (confirm)
                                {
                                    try
                                    {
                                        await _dbService.DeletePermissionAsync(Node.Parent.Parent.Parent.Parent.Connection, Node.Permission).ConfigureAwait(false);
                                    }
                                    catch (DocumentClientException ex)
                                    {
                                        await _dialogService.ShowError(ex.Parse(), "Error", null, null).ConfigureAwait(false);
                                    }
                                    finally
                                    {
                                        Node.Parent.RefreshCommand.Execute(null);
                                        await DispatcherHelper.RunAsync(() => CloseCommand.Execute(null));
                                    }
                                }
                            }).ConfigureAwait(false);
                        },
                        () => !IsNewDocument));
            }
        }

        public bool IsDirty { get; private set; }
    }

    public class PermissionEditViewModelValidator : AbstractValidator<PermissionEditViewModel>
    {
        public PermissionEditViewModelValidator()
        {
            RuleFor(x => x.PermissionId).NotEmpty();
            RuleFor(x => x.ResourceLink).NotEmpty();
        }
    }
}
