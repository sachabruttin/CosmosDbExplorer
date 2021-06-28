using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.Services;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModel
{
    public class UserNodeViewModel : TreeViewItemViewModel<UsersNodeViewModel>, ICanRefreshNode, IContent
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

        public override string Name => User.Id;

        public string ContentId => User.AltLink ?? "NewUser";

        protected override async Task LoadChildren()
        {
            IsLoading = true;

            var permissions = await _dbService.GetPermissionAsync(Parent.Parent.Parent.Connection, User).ConfigureAwait(false);

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
                        async () =>
                        {
                            await DispatcherHelper.RunAsync(async () =>
                            {
                                Children.Clear();
                                await LoadChildren().ConfigureAwait(false);
                            });
                        }));
            }
        }

        public RelayCommand OpenCommand
        {
            get
            {
                return _openCommand
                    ?? (_openCommand = new RelayCommand(
                        () => MessengerInstance.Send(new EditUserMessage(this, Parent.Parent.Parent.Connection, null))));
            }
        }

        public RelayCommand AddPermissionCommand
        {
            get
            {
                return _addPermissionCommand ?? (_addPermissionCommand = new RelayCommand(
                    () => MessengerInstance.Send(new EditPermissionMessage(new PermissionNodeViewModel(null, this), Parent.Parent.Parent.Connection, null)
                    )));
            }
        }

        public User User { get; set; }
    }
}
