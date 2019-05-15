using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Extensions;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Services;
using FluentValidation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Documents;
using Validar;

namespace CosmosDbExplorer.ViewModel
{
    [InjectValidation]
    public class AddCollectionViewModel : WindowViewModelBase
    {
        private RelayCommand _saveCommand;
        private readonly IDocumentDbService _dbService;
        private List<Database> _databases;
        private string _selectedDatabase;
        private readonly IDialogService _dialogService;

        public AddCollectionViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService, IUIServices uiServices, ThroughputViewModel throughput)
            : base(messenger, uiServices)
        {
            IsFixedStorage = true;
            Title = "New Collection";
            _dbService = dbService;
            DatabaseNames = new ObservableCollection<string>();
            _dialogService = dialogService;
            Throughput = throughput;
        }

        public string Title { get; }

        public Connection Connection { get; set; }

        public string CollectionId { get; set; }

        public bool IsFixedStorage { get; set; }

        public bool IsUnlimitedStorage { get; set; }

        public string PartitionKey { get; set; }

        public ThroughputViewModel Throughput { get; set; }

        public bool IsDatabaseThroughput { get; set; }

        protected void OnIsDatabaseThroughputChanged()
        {
            IsUnlimitedStorage = IsDatabaseThroughput;
        }

        public bool StorageKindIsEnabled => !IsDatabaseThroughput;

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

                            try
                            {
                                var db = Databases.Find(_ => _.Id == SelectedDatabase.Trim()) ?? new Database { Id = SelectedDatabase.Trim() };
                                await _dbService.CreateCollectionAsync(Connection, db, collection, Throughput.Value, IsDatabaseThroughput).ConfigureAwait(true);
                                IsBusy = false;
                                Close();
                            }
                            catch (DocumentClientException clientEx)
                            {
                                await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
                            }
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
            RuleFor(x => x.Throughput).SetValidator(new ThroughputViewModelValidator());
            RuleFor(x => x.PartitionKey).NotEmpty()
                                        .When(x => x.IsUnlimitedStorage);
        }
    }

    [InjectValidation]
    public class ThroughputViewModel : ViewModelBase
    {
        public ThroughputViewModel()
        {
            Value = 400;
            IsChanged = false;
        }

        public int MaxThroughput => 1000000;

        public int MinThroughput => 400;

        public int Value { get; set; }

        public void OnValueChanged()
        {
            const decimal hourly = 0.00008m;
            EstimatedPrice = $"${hourly * Value:N3} hourly / {hourly * Value * 24:N2} daily.";
            IsChanged = true;
        }

        public async Task LoadData(Func<Task<int?>> loadThroughput)
        {
            Value = (await loadThroughput()).GetValueOrDefault(400);
            IsChanged = false;
        }

        public string EstimatedPrice { get; set; }

        public bool IsChanged { get; set; }

        public bool IsValid
        {
            get
            {
                return !((INotifyDataErrorInfo)this).HasErrors;
            }
        }
    }

    public class ThroughputViewModelValidator : AbstractValidator<ThroughputViewModel>
    {
        public ThroughputViewModelValidator()
        {
            RuleFor(x => x.Value).NotEmpty()
                           .Must(throughput => throughput % 100 == 0)
                           .WithMessage("Throughput must be a multiple of 100");

            RuleFor(x => x.Value).GreaterThanOrEqualTo(x => x.MinThroughput)
                                            .LessThanOrEqualTo(x => x.MaxThroughput)
                                            .WithMessage(x => $"Throughput must be between {x.MinThroughput:n0} and {x.MaxThroughput:n0}");

        }
    }
}
