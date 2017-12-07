using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight.Messaging;
using Its.Validation.Configuration;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class AddCollectionViewModel : WindowViewModelBase
    {
        private RelayCommand _saveCommand;
        private readonly IDocumentDbService _dbService;
        private List<Database> _databases;
        private string _selectedDatabase;

        public AddCollectionViewModel(IMessenger messenger, IDocumentDbService dbService) : base(messenger)
        {
            IsFixedStorage = true;
            Throughput = 400;
            Title = "New Collection";
            _dbService = dbService;
            DatabaseNames = new ObservableCollection<string>();
        }

        public string Title { get; }

        public Connection Connection { get; set; }

        public string CollectionId { get; set; }

        public bool IsFixedStorage { get; set; }

        public bool IsUnlimitedStorage { get; set; }

        public string PartitionKey { get; set; }

        public int MaxThroughput => IsFixedStorage ? 10000 : 1000000; 

        public int MinThroughput => IsFixedStorage ? 400 : 1000; 

        public int Throughput { get; set; }

        public void OnPropertyChanged(string propertyName, object before, object after)
        {
            if (propertyName == "IsFixedStorage" && (bool)after)
            {
                if (Throughput > 10000)
                {
                    Throughput = 10000;
                }
            }

            if (propertyName == "IsUnlimitedStorage" && (bool)after)
            {
                if (Throughput < 1000)
                {
                    Throughput = 1000;
                }
            }

            RaisePropertyChanged(() => MinThroughput);
            RaisePropertyChanged(() => MaxThroughput);
            RaisePropertyChanged(() => Throughput);
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand
                    ?? (_saveCommand = new RelayCommand(
                        async x =>
                        {
                            var collection = new DocumentCollection { Id = CollectionId.Trim() };

                            if (IsUnlimitedStorage)
                            {
                                collection.PartitionKey.Paths.Add(PartitionKey);
                            }

                            var db = Databases.FirstOrDefault(_ => _.Id == SelectedDatabase.Trim()) ?? new Database { Id = SelectedDatabase.Trim() };
                            await _dbService.CreateCollection(Connection, db, collection, Throughput);

                            Close();
                        },
                        x =>
                        {
                            var rule = Validate.That<AddCollectionViewModel>(vm => !string.IsNullOrEmpty(vm.CollectionId?.Trim()) && !string.IsNullOrEmpty(vm.SelectedDatabase?.Trim()));
                            return rule.Check(this);
                        }));
            }
        }

        public List<Database> Databases
        {
            get { return _databases; }
            set
            {
                _databases = value;

                if (_databases != null)
                {
                    _databases.ForEach(db => DatabaseNames.Add(db.Id));
                }
            }
        }

        public ObservableCollection<string> DatabaseNames { get; protected set; }

        public string SelectedDatabase
        {
            get { return _selectedDatabase; }
            set
            {
                _selectedDatabase = value;
                RaisePropertyChanged(() => SelectedDatabase);
            }
        }
    }
}
