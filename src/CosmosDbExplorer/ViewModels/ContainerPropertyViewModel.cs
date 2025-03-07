﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PropertyChanged;
using Validar;
using Microsoft.Azure.Cosmos.Linq;

namespace CosmosDbExplorer.ViewModels
{
    [InjectValidation]
    public class ContainerPropertyViewModel : UIViewModelBase, INavigationAware
    {
        //private readonly IDocumentDbService _dbService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;

        private readonly CosmosContainerService _containerService;
        private AsyncRelayCommand? _saveCommand;
        private RelayCommand? _addPartitionKeyCommand;
        private RelayCommand? _removePartitionKeyCommand;

        public ContainerPropertyViewModel(IServiceProvider serviceProvider, IDialogService dialogService, IUIServices uiServices, object parameter)
            : base(uiServices)
        {
            IsFixedStorage = true;
            Throughput = 400;
            Title = "Add Container";
            //_dbService = dbService;
            _serviceProvider = serviceProvider;
            _dialogService = dialogService;

            var (connection, database) = ((CosmosConnection, CosmosDatabase))parameter;
            Connection = connection;
            Database = database;

            IsServerless = database.IsServerless;

            _containerService = ActivatorUtilities.CreateInstance<CosmosContainerService>(_serviceProvider, Connection, Database);
        }

        public Action<bool?>? SetResult { get; set; }

        public string Title { get; }

        public CosmosConnection Connection { get; protected set; }

        [OnChangedMethod(nameof(UpdateSaveCommandStatus))]
        public string ContainerId { get; set; } = string.Empty;

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

        [OnChangedMethod(nameof(UpdateSaveCommandStatus))]
        public string PartitionKey { get; set; } = string.Empty;

        public RelayCommand AddPartitionCommand => _addPartitionKeyCommand ??= new(AddPartitionKeyExecute, AddPartitionKeyCanExecute);
        private void AddPartitionKeyExecute()
        {
            if (!HasSecondPartitionKey)
            {
                HasSecondPartitionKey = true;
            }
            else
            {
                HasThirdPartitionKey = true;
            }

            RemovePartitionCommand.NotifyCanExecuteChanged();
            AddPartitionCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
        }

        private bool AddPartitionKeyCanExecute()
        {
            return !HasSecondPartitionKey || !HasThirdPartitionKey;
        }

        public RelayCommand RemovePartitionCommand => _removePartitionKeyCommand ??= new(RemovePartitionKeyExecute, RemovePartitionKeyCanExecute);

        public void RemovePartitionKeyExecute()
        {
            if (HasThirdPartitionKey)
            {
                HasThirdPartitionKey = false;
            }
            else
            {
                HasSecondPartitionKey = false;
            }
            
            RemovePartitionCommand.NotifyCanExecuteChanged();
            AddPartitionCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
        }

        public bool RemovePartitionKeyCanExecute()
        {
            return HasSecondPartitionKey || HasThirdPartitionKey;
        }

        public bool HasSecondPartitionKey { get; set; }

        [DependsOn(nameof(HasSecondPartitionKey))]
        [OnChangedMethod(nameof(UpdateSaveCommandStatus))]
        public string SecondPartitionKey { get; set; }

        [DependsOn(nameof(HasThirdPartitionKey))]
        [OnChangedMethod(nameof(UpdateSaveCommandStatus))]
        public string ThirdPartitionKey { get; set; }

        public bool HasThirdPartitionKey { get; set; }

        public bool IsThroughputAutoscale { get; set; } = true;

        public bool IsLargePartition { get; set; }

        [DependsOn(nameof(IsUnlimitedStorage), nameof(IsFixedStorage))]
        public int MaxThroughput => IsFixedStorage ? 10000 : 100000;

        [DependsOn(nameof(IsUnlimitedStorage), nameof(IsFixedStorage))]
        public int MinThroughput => IsFixedStorage ? 400 : 1000;

        [OnChangedMethod(nameof(UpdateSaveCommandStatus))]
        [AlsoNotifyFor(nameof(EstimatedPrice))]
        public int Throughput { get; set; }

        public bool IsServerless { get; set; }

        protected void UpdateSaveCommandStatus() => SaveCommand.NotifyCanExecuteChanged();

        public AsyncRelayCommand SaveCommand => _saveCommand ??= new(SaveCommandExecuteAsync, SaveCommandCanExecute);

        private async Task SaveCommandExecuteAsync()
        {
            IsBusy = true;

            try
            {
                var pkp = new List<string> { PartitionKey };

                if (HasSecondPartitionKey)
                {
                    pkp.Add(SecondPartitionKey);
                }

                if (HasThirdPartitionKey)
                {
                    pkp.Add(ThirdPartitionKey);
                }


                var container = new CosmosContainer(ContainerId, IsLargePartition)
                {
                    PartitionKeyPath = pkp

                };

                var createdContainer = await _containerService.CreateContainerAsync(container, Throughput, IsThroughputAutoscale, new System.Threading.CancellationToken());

                Messenger.Send(new Messages.UpdateOrCreateNodeMessage<CosmosContainer, CosmosConnection>(createdContainer, Connection, null));

                OnClose();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowError(ex, "Error during Container Creation");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool SaveCommandCanExecute() => string.IsNullOrEmpty(((IDataErrorInfo)this).Error);

        public CosmosDatabase Database { get; set; }


        public string EstimatedPrice
        {
            get
            {
                const decimal hourly = 0.00008m;
                return $"${hourly * Throughput:N3} hourly / {hourly * Throughput * 24:N2} daily.";
            }
        }


        public void OnNavigatedFrom()
        {
        }

        public void OnNavigatedTo(object parameter)
        {
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
            RuleFor(x => x.PartitionKey)
                .NotEmpty()
                .Matches("^/(.)").WithMessage("'{PropertyName}' must start with a '/'.");
            
            RuleFor(x => x.SecondPartitionKey)
                .NotEmpty()
                .Matches("^/(.)").WithMessage("'{PropertyName}' must start with a '/'.")
                .When(x => x.HasSecondPartitionKey);

            RuleFor(x => x.ThirdPartitionKey)
                .Matches("^/(.)").WithMessage("'{PropertyName}' must start with a '/'.")
                .NotEmpty()
                .When(x => x.HasThirdPartitionKey);

            RuleFor(x => x.Throughput).NotEmpty().When(x => x.ProvisionThroughput);
        }
    }
}
