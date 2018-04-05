using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Services;
using CosmosDbExplorer.Views;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModel
{
    public class DatabaseNodeViewModel : ResourceNodeViewModelBase<ConnectionNodeViewModel>
    {
        private readonly Database _database;
        private RelayCommand _addNewCollectionCommand;
        private RelayCommand _deleteDatabaseCommand;

        public DatabaseNodeViewModel(Database database, ConnectionNodeViewModel parent)
            : base(database, parent, true)
        {
            _database = database;
        }

        protected override async Task LoadChildren()
        {
            IsLoading = true;

            var collections = await DbService.GetCollectionsAsync(Parent.Connection, _database).ConfigureAwait(false);

            await DispatcherHelper.RunAsync(() =>
            {
                Children.Add(new UsersNodeViewModel(_database, this));
                foreach (var collection in collections)
                {
                    Children.Add(new CollectionNodeViewModel(collection, this));
                }
            });

            IsLoading = false;
        }

        public RelayCommand AddNewCollectionCommand
        {
            get
            {
                return _addNewCollectionCommand
                    ?? (_addNewCollectionCommand = new RelayCommand(
                        async () =>
                        {
                            var form = new AddCollectionView();
                            var vm = (AddCollectionViewModel)form.DataContext;

                            vm.Databases = Parent.Databases;
                            vm.Connection = Parent.Connection;
                            vm.SelectedDatabase = _database.Id;

                            if (form.ShowDialog().GetValueOrDefault(false))
                            {
                                Children.Clear();
                                await LoadChildren().ConfigureAwait(false);
                            }
                        }));
            }
        }

        public RelayCommand DeleteDatabaseCommand
        {
            get
            {
                return _deleteDatabaseCommand
                    ?? (_deleteDatabaseCommand = new RelayCommand(
                        async () =>
                        {
                            var msg = $"Are you sure you want to delete the database '{Name}' and all his content?";
                            await DialogService.ShowMessage(msg, "Delete", null, null,
                                async confirm =>
                                {
                                    if (confirm)
                                    {
                                        UIServices.SetBusyState(true);
                                        await DbService.DeleteDatabaseAsync(Parent.Connection, _database).ConfigureAwait(true);
                                        Parent.Children.Remove(this);
                                        UIServices.SetBusyState(false);
                                    }
                                }).ConfigureAwait(true);
                        }));
            }
        }
    }
}
