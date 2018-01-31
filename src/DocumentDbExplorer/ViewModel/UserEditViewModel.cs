using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
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
    public class UserEditViewModel : PaneViewModel, IAssetTabCommand
    {
        private readonly IDocumentDbService _dbService;
        private readonly IDialogService _dialogService;
        private UserNodeViewModel _node;
        private RelayCommand _saveCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _discardCommand;

        public UserEditViewModel(IMessenger messenger, IDocumentDbService dbService, IDialogService dialogService) : base(messenger)
        {
            _dbService = dbService;
            _dialogService = dialogService;
        }

        public string UserId { get; set; }

        public void OnUserIdChanged()
        {
            IsDirty = UserId != _node.User.Id;
        }

        private void SetInformation()
        {
            UserId = _node?.User?.Id;
            IsDirty = false;
        }

        public bool IsNewDocument => Node?.User?.SelfLink == null;

        public bool IsValid => !((INotifyDataErrorInfo)this).HasErrors;

        public UserNodeViewModel Node
        {
            get { return _node; }
            set
            {
                if (_node != value)
                {
                    _node = value;
                    Header = value.Name;
                    Title = "User";
                    ContentId = value.ContentId;

                    SetInformation();
                }
            }
        }

        public RelayCommand DiscardCommand
        {
            get
            {
                return _discardCommand
                    ?? (_discardCommand = new RelayCommand(
                        x =>
                        {
                            SetInformation();
                        },
                        x => IsDirty));
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand
                    ?? (_saveCommand = new RelayCommand(
                        async x =>
                        {
                            User user = null;

                            if (IsNewDocument)
                            {
                                user = new User { Id = UserId };
                            }
                            else
                            {
                                user = _node.User;
                                user.Id = UserId;    
                            }

                            user = await _dbService.SaveUser(Node.Parent.Parent.Parent.Connection, Node.Parent.Database, user);

                            Header = user.Id;
                            Node.User = user;
                            ContentId = Node.ContentId;

                            RaisePropertyChanged(() => IsNewDocument);
                            Node.Parent.RefreshCommand.Execute(null);
                            IsDirty = false;
                        },
                        x => IsDirty && IsValid));
            }
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return _deleteCommand
                    ?? (_deleteCommand = new RelayCommand(
                        async x =>
                        {
                            await _dialogService.ShowMessage("Are you sure...", "Delete", null, null, async confirm =>
                            {
                                if (confirm)
                                {
                                    await _dbService.DeleteUser(Node.Parent.Parent.Parent.Connection, Node.User);
                                    Node.Parent.RefreshCommand.Execute(null);

                                    await DispatcherHelper.RunAsync(() => CloseCommand.Execute(null));
                                }
                            });
                        },
                        x => !IsNewDocument));
            }
        }

        public bool IsDirty { get; private set; }
    }

    public class UserEditViewModelValidator : AbstractValidator<UserEditViewModel>
    {
        public UserEditViewModelValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
