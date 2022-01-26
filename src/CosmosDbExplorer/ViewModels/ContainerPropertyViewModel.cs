using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using PropertyChanged;
using Validar;

namespace CosmosDbExplorer.ViewModels
{
    [InjectValidation]
    public class ContainerPropertyViewModel : ObservableRecipient, INavigationAware
    {
        //private readonly IDocumentDbService _dbService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;

        public ContainerPropertyViewModel(IServiceProvider serviceProvider, IDialogService dialogService, IUIServices uiServices)
        {
            IsFixedStorage = true;
            Throughput = 400;
            Title = "Add Container";
            //_dbService = dbService;
            _serviceProvider = serviceProvider;
            _dialogService = dialogService;
        }

        public Action<bool?>? SetResult { get; set; }

        public string Title { get; }

        public CosmosConnection Connection { get; protected set; }

        public string ContainerId { get; set; }

        public bool IsFixedStorage { get; set; }

        public void OnIsFixedStorageChanged()
        {
            if (Throughput > 10000)
            {
                Throughput = 10000;
            }
        }

        public bool IsUnlimitedStorage { get; set; }

        public void OnIsUnlimitedStorageChanged()
        {
            if (Throughput < 1000)
            {
                Throughput = 1000;
            }
        }

        public bool ProvisionThroughput { get; set; } = false;

        public string PartitionKey { get; set; }

        public bool IsThroughputAutoscale { get; set; } = true;

        public bool IsLargePartition { get; set; }

        [DependsOn(nameof(IsUnlimitedStorage), nameof(IsFixedStorage))]
        public int MaxThroughput => IsFixedStorage ? 10000 : 100000;

        [DependsOn(nameof(IsUnlimitedStorage), nameof(IsFixedStorage))]
        public int MinThroughput => IsFixedStorage ? 400 : 1000;

        public int Throughput { get; set; }

        public void OnThroughputChanged()
        {
            const decimal hourly = 0.00008m;
            EstimatedPrice = $"${hourly * Throughput:N3} hourly / {hourly * Throughput * 24:N2} daily.";
        }

        [DependsOn(nameof(Throughput),
            nameof(Database),
            nameof(ProvisionThroughput),
            nameof(ContainerId),
            nameof(PartitionKey))]
        public RelayCommand SaveCommand => new(SaveCommandExecute, SaveCommandCanExecute);

        private async void SaveCommandExecute()
        {
            //IsBusy = true;

            //var collection = new DocumentCollection { Id = CollectionId.Trim() };

            //if (IsUnlimitedStorage)
            //{
            //    collection.PartitionKey.Paths.Add(PartitionKey);
            //}

            //try
            //{
            //    var db = Databases.Find(_ => _.Id == SelectedDatabase.Trim()) ?? new Database { Id = SelectedDatabase.Trim() };
            //    await _dbService.CreateCollectionAsync(Connection, db, collection, Throughput).ConfigureAwait(true);
            //    IsBusy = false;
            //    Close();
            //}
            //catch (DocumentClientException clientEx)
            //{
            //    await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null).ConfigureAwait(false);
            //}
            //catch (Exception ex)
            //{
            //    await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
            //}
            try
            {
                var container = new CosmosContainer(ContainerId, IsLargePartition)
                {
                    PartitionKeyPath = PartitionKey,
                };

                var createdContainer = await _containerService.CreateContainerAsync(container, Throughput, IsThroughputAutoscale, new System.Threading.CancellationToken());

                Messenger.Send(new Messages.UpdateOrCreateNodeMessage<CosmosContainer>(createdContainer, createdContainer, null));

                OnClose();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowError(ex, "Error during Container Creation");
            }
        }

        private bool SaveCommandCanExecute() => string.IsNullOrEmpty(((IDataErrorInfo)this).Error);

        public CosmosDatabase Database { get; set; }

        private CosmosContainerService _containerService;

        public string EstimatedPrice { get; set; }


        public void OnNavigatedFrom()
        {

        }

        public void OnNavigatedTo(object parameter)
        {
            var (connection, database) = ((CosmosConnection, CosmosDatabase))parameter;
            Connection = connection;
            Database = database;

            _containerService = ActivatorUtilities.CreateInstance<CosmosContainerService>(_serviceProvider, Connection, Database);
        }

        private void OnClose()
        {
            SetResult?.Invoke(true);
        }
    }

    public class ContainerPropertyViewModelValidator : AbstractValidator<ContainerPropertyViewModel>
    {
        public ContainerPropertyViewModelValidator()
        {
            RuleFor(x => x.ContainerId).NotEmpty();
            RuleFor(x => x.Throughput).NotEmpty();
            RuleFor(x => x.PartitionKey).NotEmpty();
        }
    }
}
