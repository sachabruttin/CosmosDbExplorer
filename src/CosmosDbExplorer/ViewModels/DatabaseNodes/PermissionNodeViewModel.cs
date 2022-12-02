using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Messages;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class PermissionNodeViewModel : TreeViewItemViewModel<UserNodeViewModel>
        , ICanRefreshNode
        , IContent
        , IHaveOpenCommand
    {
        private AsyncRelayCommand? _refreshCommand;
        private RelayCommand? _openCommand;

        public PermissionNodeViewModel(CosmosPermission permission, UserNodeViewModel parent)
            : base(parent, false)
        {
            Permission = permission;
        }

        public CosmosPermission Permission { get; set; }

        public string Name => Permission.Id ?? "n/a";

        public string ContentId => Permission.SelfLink ?? "n/a";

        public ICommand RefreshCommand => _refreshCommand ??= new(RefreshCommandExecute);

        private Task RefreshCommandExecute()
        {
            Children.Clear();
            return LoadChildren(new System.Threading.CancellationToken());
        }

        public RelayCommand OpenCommand => _openCommand ??= new(() => Messenger.Send(new EditPermissionMessage(this, Parent.Parent.Parent.Parent.Connection, Parent.Parent.Database)));

    }
}
