﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Views;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class DatabaseNodeViewModel : ResourceNodeViewModelBase<ConnectionNodeViewModel>
    {
        private RelayCommand _addNewCollectionCommand;
        private RelayCommand _deleteDatabaseCommand;
        private readonly IServiceProvider _serviceProvider;

        public DatabaseNodeViewModel(IServiceProvider serviceProvider, CosmosDatabase database, ConnectionNodeViewModel parent)
            : base(database, parent, true)
        {
            _serviceProvider = serviceProvider;
            Database = database;
        }

        public CosmosDatabase Database { get; }

        protected override async Task LoadChildren(CancellationToken token)
        {
            try
            {
                IsLoading = true;

                var service = ActivatorUtilities.CreateInstance<CosmosContainerService>(_serviceProvider, Parent.Connection, Database);
                var containers = await service.GetContainersAsync(token);

                // TODO: Handle cancellation
               
                Children.Add(new UsersNodeViewModel(Database, this));
                
                foreach (var container in containers)
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

            //IsLoading = true;

            //var collections = await DbService.GetCollectionsAsync(Parent.Connection, Database).ConfigureAwait(false);

            //await DispatcherHelper.RunAsync(() =>
            //{
            //    Children.Add(new UsersNodeViewModel(Database, this));
            //    foreach (var collection in collections)
            //    {
            //        Children.Add(new CollectionNodeViewModel(collection, this));
            //    }
            //});

            //IsLoading = false;
        }

        public RelayCommand AddNewCollectionCommand => throw new System.NotImplementedException();
        //{
        //    get
        //    {
        //        return _addNewCollectionCommand
        //            ?? (_addNewCollectionCommand = new RelayCommand(
        //                async () =>
        //                {
        //                    var form = new AddCollectionView();
        //                    var vm = (AddCollectionViewModel)form.DataContext;

        //                    vm.Databases = Parent.Databases;
        //                    vm.Connection = Parent.Connection;
        //                    vm.SelectedDatabase = Database.Id;

        //                    if (form.ShowDialog().GetValueOrDefault(false))
        //                    {
        //                        Children.Clear();
        //                        await LoadChildren().ConfigureAwait(false);
        //                    }
        //                }));
        //    }
        //}

        public RelayCommand DeleteDatabaseCommand => throw new System.NotImplementedException();
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
    }
}