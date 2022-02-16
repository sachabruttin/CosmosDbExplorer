using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class UsersNodeViewModel : TreeViewItemViewModel<DatabaseNodeViewModel>, ICanRefreshNode
    {
        private RelayCommand _refreshCommand;
        private RelayCommand _addUserCommand;

        public UsersNodeViewModel(CosmosDatabase database, DatabaseNodeViewModel parent)
            : base(parent, true)
        {
            Name = "Users";
            Database = database;
        }

        public string Name { get; set; }

        public CosmosDatabase Database { get; }

        public ICommand RefreshCommand => _refreshCommand;
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

        //public RelayCommand AddUserCommand
        //{
        //    get
        //    {
        //        return _addUserCommand ?? (_addUserCommand = new RelayCommand(
        //            () => MessengerInstance.Send(new EditUserMessage(new UserNodeViewModel(new User(), this), Parent.Parent.Connection, null)
        //            )));
        //    }
        //}

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            //var users = await _dbService.GetUsersAsync(Parent.Parent.Connection, Database).ConfigureAwait(false);

            //await DispatcherHelper.RunAsync(() =>
            //{
            //    foreach (var user in users)
            //    {
            //        Children.Add(new UserNodeViewModel(user, this));
            //    }
            //});

            IsLoading = false;
        }
    }
}
