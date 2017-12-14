using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Services;
using DocumentDbExplorer.Views;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class DatabaseNodeViewModel : TreeViewItemViewModel
    {
        private readonly Database _database;
        private RelayCommand _refreshCommand;
        private RelayCommand _addNewCollectionCommand;
        private readonly IDocumentDbService _dbService;
        private readonly IDialogService _dialogService;
        private RelayCommand _deleteDatabaseCommand;

        public DatabaseNodeViewModel(Database database, ConnectionNodeViewModel parent)
            : base(parent, parent.MessengerInstance, true)
        {
            Name = database.Id;
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
            _dialogService = SimpleIoc.Default.GetInstance<IDialogService>();
            _database = database;
        }

        public string Name { get; set; }

        public new ConnectionNodeViewModel Parent
        {
            get { return base.Parent as ConnectionNodeViewModel; }
        }

        protected override async Task LoadChildren()
        {
            IsLoading = true;
            var collections = await _dbService.GetCollections(Parent.Connection, _database);

            foreach (var collection in collections)
            {
                await DispatcherHelper.RunAsync(() => Children.Add(new CollectionNodeViewModel(collection, this)));
            }

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

        public RelayCommand AddNewCollectionCommand
        {
            get
            {
                return _addNewCollectionCommand
                    ?? (_addNewCollectionCommand = new RelayCommand(
                        async x =>
                        {
                            var form = new AddCollectionView();
                            var vm = (AddCollectionViewModel)form.DataContext;

                            vm.Databases = Parent.Databases;
                            vm.Connection = Parent.Connection;
                            vm.SelectedDatabase = _database.Id;

                            
                            if (form.ShowDialog().GetValueOrDefault(false))
                            {
                                Children.Clear();
                                await LoadChildren();
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
                        async x =>
                        {
                            var msg = $"Are you sure you want to delete the database '{Name}' and all his content?";
                            await _dialogService.ShowMessage(msg, "Delete", null, null,
                                async confirm =>
                                {
                                    if (confirm)
                                    {
                                        await _dbService.DeleteDatabase(Parent.Connection, _database);
                                        await DispatcherHelper.RunAsync(() => Parent.Children.Remove(this));
                                    }
                                });
                        }));
            }
        }
    }
}
