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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class ConnectionNodeViewModel : TreeViewItemViewModel, ICanRefreshNode
    {
        private RelayCommand _editConnectionCommand;
        private RelayCommand _removeConnectionCommand;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWindowManagerService _windowManagerService;

        //private readonly IDialogService _dialogService;
        //private readonly ISettingsService _settingsService;
        private RelayCommand _refreshCommand;
        private RelayCommand _addNewCollectionCommand;

        public ConnectionNodeViewModel(IServiceProvider serviceProvider, CosmosConnection connection)
            : base(null, true)
        {
            _serviceProvider = serviceProvider;
            _windowManagerService = serviceProvider.GetRequiredService<IWindowManagerService>();
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
                    //await DispatcherHelper.RunAsync(() => Children.Add(new DatabaseNodeViewModel(db, this)));
                }
            }
            //catch (HttpRequestException ex)
            //{
            //    await DispatcherHelper.RunAsync(async () => await _dialogService.ShowError(ex, "Error", null, null));
            //}
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

        //public RelayCommand EditConnectionCommand
        //{
        //    get
        //    {
        //        return _editConnectionCommand
        //            ?? (_editConnectionCommand = new RelayCommand(
        //                async () =>
        //                {
        //                    var form = new AccountSettingsView();
        //                    var vm = (AccountSettingsViewModel)form.DataContext;
        //                    vm.SetConnection(Connection);

        //                    if (form.ShowDialog().GetValueOrDefault(false))
        //                    {
        //                        Children.Clear();
        //                        await LoadChildren();
        //                    }
        //                }
        //                ));
        //    }
        //}

        //public RelayCommand RemoveConnectionCommand
        //{
        //    get
        //    {
        //        return _removeConnectionCommand
        //            ?? (_removeConnectionCommand = new RelayCommand(
        //                async () =>
        //                {
        //                    await _dialogService.ShowMessage("Are you sure that you want to delete this connection?", "Delete connection", null, null,
        //                        async confirm =>
        //                        {
        //                            if (confirm)
        //                            {
        //                                await _settingsService.RemoveConnection(Connection);
        //                                MessengerInstance.Send(new RemoveConnectionMessage(Connection));
        //                            }
        //                        });
        //                }
        //                ));
        //    }
        //}

        public RelayCommand RefreshCommand => new(async () =>
        {
            Children.Clear();
            await LoadChildren(new CancellationToken());
        });

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
        //}
    }
}
