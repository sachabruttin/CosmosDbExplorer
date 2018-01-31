using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class UserNodeViewModel : TreeViewItemViewModel, ICanRefreshNode
    {
        private readonly UsersNodeViewModel _parent;
        private readonly IDocumentDbService _dbService;
        private RelayCommand _refreshCommand;
        private RelayCommand _openCommand;
        private RelayCommand _addPermissionCommand;

        public UserNodeViewModel(User user, UsersNodeViewModel parent)
            : base(parent, parent.MessengerInstance, true)
        {
            User = user;
            _parent = parent;
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
        }

        public string Name => User.Id;

        public string ContentId => User.AltLink;

        public new UsersNodeViewModel Parent
        {
            get { return base.Parent as UsersNodeViewModel; }
        }

        protected override async Task LoadChildren()
        {
            IsLoading = true;

            var permissions = await _dbService.GetPermission(Parent.Parent.Parent.Connection, User);

            await DispatcherHelper.RunAsync(() =>
            {
                foreach (var permission in permissions)
                {
                    Children.Add(new PermissionNodeViewModel(permission, this));
                }
            });

            IsLoading = false;
        }

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                        async x =>
                        {
                            Children.Clear();
                            await LoadChildren();
                        }));
            }
        }

        public RelayCommand OpenCommand
        {
            get
            {
                return _openCommand
                    ?? (_openCommand = new RelayCommand(
                        x => MessengerInstance.Send(new EditUserMessage(this))));
            }
        }

        public RelayCommand AddPermissionCommand
        {
            get
            {
                return _addPermissionCommand ?? (_addPermissionCommand = new RelayCommand(
                    x => MessengerInstance.Send(new EditPermissionMessage(new PermissionNodeViewModel(new Permission(), this))
                    )));
            }
        }

        public User User { get; set; }
    }
}
