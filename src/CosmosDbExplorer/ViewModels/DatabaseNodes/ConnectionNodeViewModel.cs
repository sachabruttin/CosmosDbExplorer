using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

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
using PropertyChanged;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class ConnectionNodeViewModel : TreeViewItemViewModel, ICanRefreshNode
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWindowManagerService _windowManagerService;
        private readonly IRightPaneService _rightPaneService;
        private readonly IDialogService _dialogService;
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private RelayCommand _addNewDatabaseCommand;
        private RelayCommand _editConnectionCommand;
        private RelayCommand _refreshCommand;
        private AsyncRelayCommand _removeConnectionCommand;

        public ConnectionNodeViewModel(IServiceProvider serviceProvider, CosmosConnection connection)
            : base(null, true)
        {
            _serviceProvider = serviceProvider;
            _dialogService = serviceProvider.GetRequiredService<IDialogService>();
            _windowManagerService = serviceProvider.GetRequiredService<IWindowManagerService>();
            _rightPaneService = _serviceProvider.GetRequiredService<IRightPaneService>();
            _persistAndRestoreService = serviceProvider.GetRequiredService<IPersistAndRestoreService>();
            Connection = connection;

            Messenger.Register<ConnectionNodeViewModel, UpdateOrCreateNodeMessage<CosmosDatabase, CosmosConnection>>(this, static (r, m) => r.OnDatabaseCreated(m));
        }

        private void OnDatabaseCreated(UpdateOrCreateNodeMessage<CosmosDatabase, CosmosConnection> message)
        {
            if (message.Parent == Connection)
            {
                Children.Add(new DatabaseNodeViewModel(_serviceProvider, message.Resource, this));
            }
        }

        public CosmosConnection Connection { get; set; }

        public IList<CosmosDatabase> Databases { get; protected set; }

        public string Name => Connection.DatabaseUri.ToString();

        protected override async Task LoadChildren(CancellationToken token)
        {
            AddNewDatabaseCommand.NotifyCanExecuteChanged();

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

        public RelayCommand EditConnectionCommand => _editConnectionCommand ??= new(EditConnectionCommandExecute);

        private void EditConnectionCommandExecute()
        {
            var vmName = typeof(AccountSettingsViewModel);
            _windowManagerService.OpenInDialog(vmName, Connection);
        }

        public AsyncRelayCommand RemoveConnectionCommand => _removeConnectionCommand ??= new(RemoveConnectionCommandExecute);

        private async Task RemoveConnectionCommandExecute()
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

        public RelayCommand AddNewDatabaseCommand => _addNewDatabaseCommand ??= new(AddNewDatabaseCommandExecute);

        private void AddNewDatabaseCommandExecute()
        {
            var vmName = typeof(DatabasePropertyViewModel);
            _rightPaneService.OpenInRightPane(vmName, new[] { Connection });
        }

        public ICommand RefreshCommand => _refreshCommand ??= new(RefreshCommandExecuteAsync);

        private async void RefreshCommandExecuteAsync()
        {
            Children.Clear();
            await LoadChildren(new CancellationToken());
        }

        protected override void NotifyCanExecuteChanged()
        {
            AddNewDatabaseCommand.NotifyCanExecuteChanged();
            ((RelayCommand)RefreshCommand).NotifyCanExecuteChanged();
            RemoveConnectionCommand.NotifyCanExecuteChanged();
            EditConnectionCommand.NotifyCanExecuteChanged();
        }
    }
}
