using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class UserNodeViewModel : TreeViewItemViewModel<UsersNodeViewModel>, ICanRefreshNode, IContent
    {
        private RelayCommand _refreshCommand;
        private RelayCommand _openCommand;
        private RelayCommand _addPermissionCommand;

        public UserNodeViewModel(CosmosUser user, UsersNodeViewModel parent)
            : base(parent, true)
        {
            User = user;
        }

        public string Name => User.Id;

        public string ContentId => User.SelfLink ?? "NewUser";

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            //var permissions = await _dbService.GetPermissionAsync(Parent.Parent.Parent.Connection, User).ConfigureAwait(false);

            //await DispatcherHelper.RunAsync(() =>
            //{
            //    foreach (var permission in permissions)
            //    {
            //        Children.Add(new PermissionNodeViewModel(permission, this));
            //    }
            //});

            IsLoading = false;
        }

        public RelayCommand RefreshCommand => _refreshCommand;
        //{
        //    get
        //    {
        //        return _refreshCommand
        //            ?? (_refreshCommand = new RelayCommand(
        //                async () =>
        //                {
        //                    await DispatcherHelper.RunAsync(async () =>
        //                    {
        //                        Children.Clear();
        //                        await LoadChildren().ConfigureAwait(false);
        //                    });
        //                }));
        //    }
        //}

        //public RelayCommand OpenCommand
        //{
        //    get
        //    {
        //        return _openCommand
        //            ?? (_openCommand = new RelayCommand(
        //                () => MessengerInstance.Send(new EditUserMessage(this, Parent.Parent.Parent.Connection, null))));
        //    }
        //}

        //public RelayCommand AddPermissionCommand
        //{
        //    get
        //    {
        //        return _addPermissionCommand ?? (_addPermissionCommand = new RelayCommand(
        //            () => MessengerInstance.Send(new EditPermissionMessage(new PermissionNodeViewModel(null, this), Parent.Parent.Parent.Connection, null)
        //            )));
        //    }
        //}

        public CosmosUser User { get; set; }
    }
}
