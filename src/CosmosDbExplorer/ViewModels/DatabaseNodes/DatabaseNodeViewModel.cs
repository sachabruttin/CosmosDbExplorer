using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Views;
using Microsoft.Azure.Documents;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class DatabaseNodeViewModel : ResourceNodeViewModelBase<ConnectionNodeViewModel>
    {
        private RelayCommand _addNewCollectionCommand;
        private RelayCommand _deleteDatabaseCommand;

        public DatabaseNodeViewModel(CosmosDatabase database, ConnectionNodeViewModel parent)
            : base(database, parent, true)
        {
            Database = database;
        }

        public CosmosDatabase Database { get; }

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            //var collections = await DbService.GetCollectionsAsync(Parent.Connection, Database).ConfigureAwait(false);

            //await DispatcherHelper.RunAsync(() =>
            //{
            //    Children.Add(new UsersNodeViewModel(Database, this));
            //    foreach (var collection in collections)
            //    {
            //        Children.Add(new CollectionNodeViewModel(collection, this));
            //    }
            //});

            IsLoading = false;
        }

        public RelayCommand AddNewCollectionCommand => throw new System.NotImplementedException();
        //{
        //    get
        //    {
        //        return _addNewCollectionCommand
        //            ?? (_addNewCollectionCommand = new RelayCommand(
        //                async () =>
        //                {
        //                    var form = new AddCollectionView();
        //                    var vm = (AddCollectionViewModel)form.DataContext;

        //                    vm.Databases = Parent.Databases;
        //                    vm.Connection = Parent.Connection;
        //                    vm.SelectedDatabase = Database.Id;

        //                    if (form.ShowDialog().GetValueOrDefault(false))
        //                    {
        //                        Children.Clear();
        //                        await LoadChildren().ConfigureAwait(false);
        //                    }
        //                }));
        //    }
        //}

        public RelayCommand DeleteDatabaseCommand => throw new System.NotImplementedException();
        //{
        //    get
        //    {
        //        return _deleteDatabaseCommand
        //            ?? (_deleteDatabaseCommand = new RelayCommand(
        //                async () =>
        //                {
        //                    var msg = $"Are you sure you want to delete the database '{Name}' and all his content?";
        //                    await DialogService.ShowMessage(msg, "Delete", null, null,
        //                        async confirm =>
        //                        {
        //                            if (confirm)
        //                            {
        //                                UIServices.SetBusyState(true);
        //                                await DbService.DeleteDatabaseAsync(Parent.Connection, Database).ConfigureAwait(true);
        //                                Parent.Children.Remove(this);
        //                                UIServices.SetBusyState(false);
        //                            }
        //                        }).ConfigureAwait(true);
        //                }));
        //    }
        //}
    }
}
