using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Messages;

using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class UsersNodeViewModel : TreeViewItemViewModel<DatabaseNodeViewModel>, ICanRefreshNode
    {
        private AsyncRelayCommand? _refreshCommand;
        private RelayCommand? _addUserCommand;

        public UsersNodeViewModel(IServiceProvider serviceProvider, CosmosDatabase database, DatabaseNodeViewModel parent)
            : base(parent, true)
        {
            Name = "Users";
            Database = database;

            _userService = ActivatorUtilities.CreateInstance<CosmosUserService>(serviceProvider, Parent.Parent.Connection, database);
        }

        public string Name { get; set; }

        public CosmosDatabase Database { get; }

        private readonly CosmosUserService _userService;

        public ICommand RefreshCommand => _refreshCommand ??= new(RefreshCommandExecute);

        private Task RefreshCommandExecute()
        {
            Children.Clear();
            return LoadChildren(new CancellationToken());
        }

        public RelayCommand AddUserCommand => _addUserCommand ??= new(() => Messenger.Send(new EditUserMessage(new UserNodeViewModel(new CosmosUser(), this, _userService), Parent.Parent.Connection, Parent.Database)));

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            var users = await _userService.GetUsersAsync(token);

            foreach (var user in users)
            {
                Children.Add(new UserNodeViewModel(user, this, _userService));
            }

            IsLoading = false;
        }
    }
}
