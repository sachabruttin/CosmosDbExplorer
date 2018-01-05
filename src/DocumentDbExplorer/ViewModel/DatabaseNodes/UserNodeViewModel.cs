using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class UserNodeViewModel : TreeViewItemViewModel, ICanRefreshNode
    {
        private readonly User _user;
        private readonly UsersNodeViewModel _parent;
        private readonly IDocumentDbService _dbService;
        private RelayCommand _refreshCommand;

        public UserNodeViewModel(User user, UsersNodeViewModel parent)
            : base(parent, parent.MessengerInstance, true)
        {
            _user = user;
            _parent = parent;
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
        }

        public string Name => _user.Id;

        public new UsersNodeViewModel Parent
        {
            get { return base.Parent as UsersNodeViewModel; }
        }

        protected override async Task LoadChildren()
        {
            IsLoading = true;

            var permissions = await _dbService.GetPermission(Parent.Parent.Parent.Connection, _user);

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
    }
}
