using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Services;
using FluentValidation;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Documents;
using Validar;

namespace DocumentDbExplorer.ViewModel
{
    [InjectValidation]
    public class AddCollectionViewModel : WindowViewModelBase
    {
        private RelayCommand _saveCommand;
        private readonly IDocumentDbService _dbService;
        private List<Database> _databases;
        private string _selectedDatabase;

        public AddCollectionViewModel(IMessenger messenger, IDocumentDbService dbService, IUIServices uiServices)
            : base(messenger, uiServices)
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

        public void OnIsFixedStorageChanged()
        {
            if (Throughput > 10000)
            {
                Throughput = 10000;
            }

            RaisePropertyChanged(() => MinThroughput);
            RaisePropertyChanged(() => MaxThroughput);
        }

        public bool IsUnlimitedStorage { get; set; }

        public void OnIsUnlimitedStorageChanged()
        {
            if (Throughput < 1000)
            {
                Throughput = 1000;
            }

            RaisePropertyChanged(() => MinThroughput);
            RaisePropertyChanged(() => MaxThroughput);
        }

        public string PartitionKey { get; set; }

        public int MaxThroughput => IsFixedStorage ? 10000 : 100000;

        public int MinThroughput => IsFixedStorage ? 400 : 1000;

        public int Throughput { get; set; }

        public void OnThroughputChanged()
        {
            const decimal hourly = 0.00008m;
            EstimatedPrice = $"${hourly * Throughput:N3} hourly / {hourly * Throughput * 24:N2} daily.";
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand
                    ?? (_saveCommand = new RelayCommand(
                        async () =>
                        {
                            IsBusy = true;

                            var collection = new DocumentCollection { Id = CollectionId.Trim() };

                            if (IsUnlimitedStorage)
                            {
                                collection.PartitionKey.Paths.Add(PartitionKey);
                            }

                            var db = Databases.Find(_ => _.Id == SelectedDatabase.Trim()) ?? new Database { Id = SelectedDatabase.Trim() };
                            await _dbService.CreateCollectionAsync(Connection, db, collection, Throughput).ConfigureAwait(true);

                            IsBusy = false;
                            Close();
                        },
                        () => !((INotifyDataErrorInfo)this).HasErrors));
            }
        }

        public List<Database> Databases
        {
            get { return _databases; }
            set
            {
                _databases = value;
                _databases?.ForEach(db => DatabaseNames.Add(db.Id));
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

        public string EstimatedPrice { get; set; }
    }

    public class AddCollectionViewModelValidator : AbstractValidator<AddCollectionViewModel>
    {
        public AddCollectionViewModelValidator()
        {
            RuleFor(x => x.CollectionId).NotEmpty();
            RuleFor(x => x.SelectedDatabase).NotEmpty();
            RuleFor(x => x.Throughput).NotEmpty()
                                      .Must(throughput => throughput % 100 == 0)
                                      .WithMessage("Throughput must be a multiple of 100");
            RuleFor(x => x.PartitionKey).NotEmpty()
                                        .When(x => x.IsUnlimitedStorage);
        }
    }
}
