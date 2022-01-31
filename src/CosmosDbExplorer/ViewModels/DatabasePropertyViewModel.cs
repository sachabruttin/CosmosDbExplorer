using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
    public class DatabasePropertyViewModel : UIViewModelBase, INavigationAware
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;
        private CosmosDatabaseService _databaseService;
        private AsyncRelayCommand _saveCommand;

        public DatabasePropertyViewModel(IServiceProvider serviceProvider, IDialogService dialogService, IUIServices uiServices)
            : base(uiServices)
        {
            //IsFixedStorage = true;
            Throughput = 400;
            Title = "Add Container";
            _serviceProvider = serviceProvider;
            _dialogService = dialogService;
        }

        public Action<bool?>? SetResult { get; set; }

        public string Title { get; }
        
        public CosmosConnection Connection { get; private set; }

        [OnChangedMethod(nameof(UpdateSaveCommandStatus))]
        public string DatabaseId { get; set; }

        [OnChangedMethod(nameof(UpdateSaveCommandStatus))]
        public bool ProvisionThroughput { get; set; } = true;

        public bool IsThroughputAutoscale { get; set; } = true;

        public int MaxThroughput => IsThroughputAutoscale ? 10000 : 100000;

        public int MinThroughput => IsThroughputAutoscale ? 400 : 1000;

        [OnChangedMethod(nameof(UpdateSaveCommandStatus))]
        public int Throughput { get; set; }

        protected void UpdateSaveCommandStatus() => SaveCommand.NotifyCanExecuteChanged();

        public AsyncRelayCommand SaveCommand => _saveCommand ??= new(SaveCommandExecute, SaveCommandCanExecute);

        private async Task SaveCommandExecute()
        {
            try
            {
                var database = new CosmosDatabase(DatabaseId);
                var throughput = ProvisionThroughput ? Throughput : (int?)null;
                var isAutoScale = ProvisionThroughput ? IsThroughputAutoscale : (bool?)null;

                var createdDatabase = await _databaseService.CreateDatabaseAsync(database, throughput, isAutoScale, new System.Threading.CancellationToken());

                Messenger.Send(new Messages.UpdateOrCreateNodeMessage<CosmosDatabase, CosmosConnection>(createdDatabase, Connection, null));
                OnClose();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowError(ex, "Error during Database creation");
            }
        }

        private bool SaveCommandCanExecute() => string.IsNullOrEmpty(((IDataErrorInfo)this).Error);

        public void OnNavigatedFrom()
        {

        }

        public void OnNavigatedTo(object parameter)
        {
            Connection = (CosmosConnection)parameter;

            _databaseService = ActivatorUtilities.CreateInstance<CosmosDatabaseService>(_serviceProvider, Connection);
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
            RuleFor(x => x.Throughput).NotEmpty()
                                      .When(x => x.ProvisionThroughput);
        }
    }
}
