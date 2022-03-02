using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Messages;

using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class UserNodeViewModel : TreeViewItemViewModel<UsersNodeViewModel>, ICanRefreshNode, IContent
    {
        private AsyncRelayCommand? _refreshCommand;
        private RelayCommand? _openCommand;
        private RelayCommand? _addPermissionCommand;
        private readonly CosmosUserService _cosmosUserService;

        public UserNodeViewModel(CosmosUser user, UsersNodeViewModel parent, CosmosUserService cosmosUserService)
            : base(parent, true)
        {
            User = user;
            _cosmosUserService = cosmosUserService;
        }

        public string Name => User.Id;

        public string ContentId => User.SelfLink ?? "NewUser";

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            var permissions = await _cosmosUserService.GetPermissionsAsync(User, token);

            foreach (var permission in permissions)
            {
                Children.Add(new PermissionNodeViewModel(permission, this));
            }

            IsLoading = false;
        }

        public ICommand RefreshCommand => _refreshCommand ??= new(RefreshCommandExecute);

        private Task RefreshCommandExecute()
        {
            Children.Clear();
            return LoadChildren(new CancellationToken());
        }


        public RelayCommand OpenCommand => _openCommand ??= new(() => Messenger.Send(new EditUserMessage(this, Parent.Parent.Parent.Connection, Parent.Database)));


        public RelayCommand AddPermissionCommand => _addPermissionCommand ??= new(() => Messenger.Send(new EditPermissionMessage(new PermissionNodeViewModel(new CosmosPermission(), this), Parent.Parent.Parent.Connection, null)));

        public CosmosUser User { get; set; }
    }
}
