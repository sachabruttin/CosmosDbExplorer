﻿using System;
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
        private RelayCommand _deleteDatabaseCommand;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRightPaneService _rightPaneService;
        private readonly IWindowManagerService _windowManagerService;
        private readonly CosmosContainerService _containerService;

        public DatabaseNodeViewModel(IServiceProvider serviceProvider, CosmosDatabase database, ConnectionNodeViewModel parent)
            : base(database, parent, true)
        {
            _serviceProvider = serviceProvider;
            _rightPaneService = _serviceProvider.GetRequiredService<IRightPaneService>();
            _windowManagerService = _serviceProvider.GetRequiredService<IWindowManagerService>();
            _containerService = ActivatorUtilities.CreateInstance<CosmosContainerService>(_serviceProvider, Parent.Connection, database);

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

                Children.Add(new UsersNodeViewModel(Database, this));

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


        public RelayCommand DeleteDatabaseCommand => _deleteDatabaseCommand ??= new(() => throw new System.NotImplementedException());
        //{
        //    get
        //    {
        //        return _deleteDatabaseCommand
        //            ?? (_deleteDatabaseCommand = new RelayCommand(
        //                async () =>
        //                {
        //                    var msg = $"Are you sure you want to delete the database '{Name}' and all his content?";
        //                    await DialogService.ShowMessage(msg, "Delete", null, null,
        //                        async confirm =>
        //                        {
        //                            if (confirm)
        //                            {
        //                                UIServices.SetBusyState(true);
        //                                await DbService.DeleteDatabaseAsync(Parent.Connection, Database).ConfigureAwait(true);
        //                                Parent.Children.Remove(this);
        //                                UIServices.SetBusyState(false);
        //                            }
        //                        }).ConfigureAwait(true);
        //                }));
        //    }
        //}

        protected override void NotifyCanExecuteChanged()
        {
            AddNewContainerCommand.NotifyCanExecuteChanged();
            DeleteDatabaseCommand.NotifyCanExecuteChanged();
        }
    }
}
