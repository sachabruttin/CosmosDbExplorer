using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;
using DocumentDbExplorer.Services;
using DocumentDbExplorer.Views;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class ConnectionNodeViewModel : TreeViewItemViewModel, ICanRefreshNode
    {
        private RelayCommand _editConnectionCommand;
        private RelayCommand _removeConnectionCommand;
        private readonly IDocumentDbService _dbService;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private RelayCommand _refreshCommand;
        private RelayCommand _addNewCollectionCommand;

        public ConnectionNodeViewModel(IDocumentDbService dbService, IMessenger messenger, IDialogService dialogService, ISettingsService settingsService) : base(null, messenger, true)
        {
            _dbService = dbService;
            _dialogService = dialogService;
            _settingsService = settingsService;
        }

        public Connection Connection { get; set; }

        public List<Database> Databases { get; protected set; }

        public string Name => Connection.DatabaseUri.ToString();

        protected override async Task LoadChildren()
        {
            try
            {
                IsLoading = true;
                Databases = await _dbService.GetDatabases(Connection);

                foreach (var db in Databases)
                {
                    await DispatcherHelper.RunAsync(() => Children.Add(new DatabaseNodeViewModel(db, this)));
                }
            }
            catch (HttpRequestException ex)
            {
                await _dialogService.ShowError(ex, "Error", null, null);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public RelayCommand EditConnectionCommand
        {
            get
            {
                return _editConnectionCommand
                    ?? (_editConnectionCommand = new RelayCommand(
                        async x => 
                        {
                            var form = new AccountSettingsView();
                            var vm = (AccountSettingsViewModel)form.DataContext;
                            vm.AccountEndpoint = Connection.DatabaseUri.ToString();
                            vm.AccountSecret = Connection.AuthenticationKey;
                            vm.Label = Connection.Label;
                            vm.UseLocalEmulator = Connection.IsLocalEmulator();
                            vm.ConnectionType = Connection.ConnectionType;
                            vm.AccentColor = Connection.AccentColor;

                            if (form.ShowDialog().GetValueOrDefault(false))
                            {
                                Children.Clear();
                                await LoadChildren();
                            }
                        }
                        )); 
            }
        }

        public RelayCommand RemoveConnectionCommand
        {
            get
            {
                return _removeConnectionCommand
                    ?? (_removeConnectionCommand = new RelayCommand(
                        async x => 
                        {
                            await _dialogService.ShowMessage("Are you sure that you want to delete this connection?", "Delete connection", null, null,
                                async confirm =>
                                {
                                    if (confirm)
                                    {
                                        await _settingsService.RemoveConnection(Connection);
                                        MessengerInstance.Send(new RemoveConnectionMessage(Connection));
                                    }
                                });
                        }
                        ));
            }
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

                            vm.Databases = Databases;
                            vm.Connection = Connection;

                            if (form.ShowDialog().GetValueOrDefault(false))
                            {
                                Children.Clear();
                                await LoadChildren();
                            }
                        }));
            }
        }
    }
}
