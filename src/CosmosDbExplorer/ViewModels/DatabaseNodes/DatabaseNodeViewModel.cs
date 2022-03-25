using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class DatabaseNodeViewModel : ResourceNodeViewModelBase<ConnectionNodeViewModel>
    {
        private RelayCommand _addNewCollectionCommand;
        private AsyncRelayCommand _deleteDatabaseCommand;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRightPaneService _rightPaneService;
        private readonly IWindowManagerService _windowManagerService;
        private readonly IDialogService _dialogService;
        private readonly CosmosContainerService _containerService;
        private readonly CosmosDatabaseService _databaseService;

        public DatabaseNodeViewModel(IServiceProvider serviceProvider, CosmosDatabase database, ConnectionNodeViewModel parent)
            : base(database, parent, true)
        {
            _serviceProvider = serviceProvider;
            _rightPaneService = _serviceProvider.GetRequiredService<IRightPaneService>();
            _windowManagerService = _serviceProvider.GetRequiredService<IWindowManagerService>();
            _dialogService = _serviceProvider.GetRequiredService<IDialogService>();
            _containerService = ActivatorUtilities.CreateInstance<CosmosContainerService>(_serviceProvider, Parent.Connection, database);
            _databaseService = ActivatorUtilities.CreateInstance<CosmosDatabaseService>(_serviceProvider, Parent.Connection);

            Database = database;

            Messenger.Register<DatabaseNodeViewModel, UpdateOrCreateNodeMessage<CosmosContainer, CosmosConnection>>(this, static (r, m) => r.OnNewContainerCreated(m));

        }

        private void OnNewContainerCreated(UpdateOrCreateNodeMessage<CosmosContainer, CosmosConnection> message)
        {
            if (message.Parent == Parent.Connection)
            {
                Children.Add(new ContainerNodeViewModel(_serviceProvider, message.Resource, this));
                _rightPaneService.CleanUp();
            }
        }

        public CosmosDatabase Database { get; }

        protected override async Task LoadChildren(CancellationToken token)
        {
            try
            {
                IsLoading = true;

                AddNewContainerCommand.NotifyCanExecuteChanged();

                var containers = await _containerService.GetContainersAsync(token);

                // TODO: Handle cancellation
                if (Database.Throughput != null)
                {
                    Children.Add(new DatabaseScaleNodeViewModel(this));
                }

                Children.Add(new UsersNodeViewModel(_serviceProvider, Database, this));

                foreach (var container in containers.OrderBy(c => c.Id))
                {
                    Children.Add(new ContainerNodeViewModel(_serviceProvider, container, this));
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

        public RelayCommand AddNewContainerCommand => _addNewCollectionCommand ??= new(AddNewContainerCommandExecute, () => !HasDummyChild);

        private void AddNewContainerCommandExecute()
        {
            var vmName = typeof(ContainerPropertyViewModel).FullName;

            if (string.IsNullOrEmpty(vmName))
            {
                return;
            }

            _rightPaneService.OpenInRightPane(vmName, (Parent.Connection, Database));
        }


        public AsyncRelayCommand DeleteDatabaseCommand => _deleteDatabaseCommand ??= new(DeleteDatabaseCommandExecuteAsync);

        private async Task DeleteDatabaseCommandExecuteAsync()
        {
            async void OnDialogClose(bool confirm)
            {
                if (!confirm)
                {
                    return;
                }

                try
                {
                    await _databaseService.DeleteDatabaseAsync(Database, new CancellationToken());
                    Messenger.Send(new RemoveNodeMessage(Database.SelfLink));
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowError(ex, $"Error during database {Database.Id} deletion");
                }
            }

            var msg = $"Are you sure you want to delete the database '{Database.Id}' and all his content?";
            await _dialogService.ShowQuestion(msg, "Delete Database", OnDialogClose);
        }


        protected override void NotifyCanExecuteChanged()
        {
            AddNewContainerCommand.NotifyCanExecuteChanged();
            DeleteDatabaseCommand.NotifyCanExecuteChanged();
        }
    }
}
