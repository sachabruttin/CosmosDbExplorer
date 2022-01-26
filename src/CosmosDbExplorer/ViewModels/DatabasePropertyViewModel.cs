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
    public class DatabasePropertyViewModel : ObservableRecipient, INavigationAware
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;
        private CosmosDatabaseService _containerService;

        public DatabasePropertyViewModel(IServiceProvider serviceProvider, IDialogService dialogService, IUIServices uiServices)
        {
            //IsFixedStorage = true;
            //Throughput = 400;
            Title = "Add Container";
            //_dbService = dbService;
            _serviceProvider = serviceProvider;
            _dialogService = dialogService;
        }

        public Action<bool?>? SetResult { get; set; }

        public string Title { get; }
        
        public CosmosConnection Connection { get; private set; }

        public string DatabaseId { get; set; }
        
        public bool ProvisionThroughput { get; set; } = true;
        
        public bool IsThroughputAutoscale { get; set; } = true;

        [DependsOn(nameof(IsThroughputAutoscale))]
        public int MaxThroughput => IsThroughputAutoscale ? 10000 : 100000;

        [DependsOn(nameof(IsThroughputAutoscale))]
        public int MinThroughput => IsThroughputAutoscale ? 400 : 1000;

        public int Throughput { get; set; }
        [DependsOn(nameof(Throughput),
                   nameof(DatabaseId),
                   nameof(ProvisionThroughput))]
        public RelayCommand SaveCommand => new(SaveCommandExecute, SaveCommandCanExecute);

        private void SaveCommandExecute()
        {
            var database = new CosmosDatabase(DatabaseId);
            Messenger.Send(new Messages.UpdateOrCreateNodeMessage<CosmosDatabase, CosmosConnection>(database, Connection, null));

            OnClose();
        }

        private bool SaveCommandCanExecute() => string.IsNullOrEmpty(((IDataErrorInfo)this).Error);

        public void OnNavigatedFrom()
        {

        }

        public void OnNavigatedTo(object parameter)
        {
            Connection = (CosmosConnection)parameter;

            _containerService = ActivatorUtilities.CreateInstance<CosmosDatabaseService>(_serviceProvider, Connection);
        }

        private void OnClose()
        {
            SetResult?.Invoke(true);
        }
    }

    public class DatabasePropertyViewModelValidator : AbstractValidator<DatabasePropertyViewModel>
    {
        public DatabasePropertyViewModelValidator()
        {
            RuleFor(x => x.DatabaseId).NotEmpty();
            RuleFor(x => x.Throughput).NotEmpty();
        }
    }
}
