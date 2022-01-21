﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class ConnectionNodeViewModel : TreeViewItemViewModel, ICanRefreshNode
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWindowManagerService _windowManagerService;
        private readonly IDialogService _dialogService;
        private readonly IPersistAndRestoreService _persistAndRestoreService;

        public ConnectionNodeViewModel(IServiceProvider serviceProvider, CosmosConnection connection)
            : base(null, true)
        {
            _serviceProvider = serviceProvider;
            _dialogService = serviceProvider.GetRequiredService<IDialogService>();
            _windowManagerService = serviceProvider.GetRequiredService<IWindowManagerService>();
            _persistAndRestoreService = serviceProvider.GetRequiredService<IPersistAndRestoreService>();
            Connection = connection;
        }

        //public ConnectionNodeViewModel(IDocumentDbService dbService, IMessenger messenger, IDialogService dialogService, ISettingsService settingsService) : base(null, messenger, true)
        //{
        //    _dbService = dbService;
        //    _dialogService = dialogService;
        //    _settingsService = settingsService;
        //}

        public CosmosConnection Connection { get; set; }

        public IList<CosmosDatabase> Databases { get; protected set; }

        public string Name => Connection.DatabaseUri.ToString();

        protected override async Task LoadChildren(CancellationToken token)
        {

            try
            {
                IsLoading = true;

                var service = ActivatorUtilities.CreateInstance<CosmosDatabaseService>(_serviceProvider, Connection);
                Databases = await service.GetDatabasesAsync(token);

                // TODO: Handle cancellation

                foreach (var db in Databases)
                {
                    Children.Add(new DatabaseNodeViewModel(_serviceProvider, db, this));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public RelayCommand EditConnectionCommand => new(() => _windowManagerService.OpenInDialog("CosmosDbExplorer.ViewModels.AccountSettingsViewModel", Connection));

        public RelayCommand RemoveConnectionCommand => new(RemoveConnectionCommandExecute);

        private async void RemoveConnectionCommandExecute()
        {
            void confirmed(bool confirm)
            {
                if (confirm)
                {
                    _persistAndRestoreService.RemoveConnection(Connection);
                    Messenger.Send(new RemoveConnectionMessage(Connection));
                }
            }

            await _dialogService.ShowQuestion(
                $"Are you sure that you want to delete this connection '{Connection.Label}'?", 
                "Delete connection", 
                confirmed);
        }

        //public RelayCommand AddNewCollectionCommand
        //{
        //    get
        //    {
        //        return _addNewCollectionCommand
        //            ?? (_addNewCollectionCommand = new RelayCommand(
        //                async () =>
        //                {
        //                    var form = new AddCollectionView();
        //                    var vm = (AddCollectionViewModel)form.DataContext;

        //                    vm.Databases = Databases;
        //                    vm.Connection = Connection;

        //                    if (form.ShowDialog().GetValueOrDefault(false))
        //                    {
        //                        Children.Clear();
        //                        await LoadChildren();
        //                    }
        //                }));
        //    }

        public RelayCommand RefreshCommand => new(async () =>
        {
            Children.Clear();
            await LoadChildren(new CancellationToken());
        });
    }
}
