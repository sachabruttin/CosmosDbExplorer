using System;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Messages;
using Microsoft.Azure.Documents;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class ContainerNodeViewModel : ResourceNodeViewModelBase<DatabaseNodeViewModel>, IHaveContainerNodeViewModel, IContent
    {
        private readonly IServiceProvider _serviceProvider;
        private RelayCommand _openImportDocumentCommand;
        private RelayCommand _newStoredProcedureCommand;
        private RelayCommand _newUdfCommand;
        private RelayCommand _newTriggerCommand;
        private RelayCommand _openSqlQueryCommand;

        public ContainerNodeViewModel(IServiceProvider serviceProvider, CosmosContainer container, DatabaseNodeViewModel parent)
            : base(container, parent, true)
        {
            _serviceProvider = serviceProvider;
            Container = container;
        }

        protected override Task LoadChildren(CancellationToken token)
        {
            Children.Add(new DocumentNodeViewModel(this));
            Children.Add(new ScaleSettingsNodeViewModel(this));
            Children.Add(new StoredProcedureRootNodeViewModel(this, _serviceProvider));
            Children.Add(new UserDefFuncRootNodeViewModel(this, _serviceProvider));
            Children.Add(new TriggerRootNodeViewModel(this, _serviceProvider));
            Children.Add(new MetricsNodeViewModel(this));

            return Task.CompletedTask;
        }

        public CosmosContainer Container { get; }

        public RelayCommand OpenSqlQueryCommand => _openSqlQueryCommand ??=  new(() => Messenger.Send(new OpenQueryViewMessage(this, Parent.Parent.Connection, Container)));

        //public RelayCommand ClearAllDocumentsCommand
        //{
        //    get
        //    {
        //        return _clearAllDocumentsCommand
        //            ?? (_clearAllDocumentsCommand = new RelayCommand(
        //                async () =>
        //                {
        //                    await DialogService.ShowMessage($"All documents will be removed from the collection {Parent.Name}.\n\nIf you have a lot of documents, this could take a while and be costly and it is perhaps preferable to use the 'Recreate' option.\n\nAre you sure you want to continue?",
        //                        "Cleanup collection", null, null,
        //                        async confirm =>
        //                        {
        //                            if (confirm)
        //                            {
        //                                MessengerInstance.Send(new IsBusyMessage(true));
        //                                await DbService.CleanCollectionAsync(Parent.Parent.Connection, Collection).ConfigureAwait(false);
        //                                MessengerInstance.Send(new IsBusyMessage(false));
        //                                await DispatcherHelper.RunAsync(async () => await DialogService.ShowMessageBox($"Collection {Parent.Name} is now empty.", "Cleanup collection").ConfigureAwait(false));
        //                            }
        //                        }).ConfigureAwait(false);
        //                }));
        //    }
        //}

        //public RelayCommand RecreateAsEmptyCommand
        //{
        //    get
        //    {
        //        return _recreateAsEmptyCommand
        //            ?? (_recreateAsEmptyCommand = new RelayCommand(
        //                async () =>
        //                {
        //                    await DialogService.ShowMessage($"Collection will be deleted and recreated with the same parameters and assets:\n\t- Stored Procedures\n\t- Triggers\n\t- User Defined Functions\n\nThis is fast and cost efficient but could affect your application(s) availability.\n\nAre you sure you want to continue?",
        //                        "Recreate empty collection", null, null,
        //                        async confirm =>
        //                        {
        //                            if (confirm)
        //                            {
        //                                MessengerInstance.Send(new IsBusyMessage(true));
        //                                await DbService.RecreateCollectionAsync(Parent.Parent.Connection, Parent.Database, Collection).ConfigureAwait(false);
        //                                MessengerInstance.Send(new IsBusyMessage(false));
        //                                Parent.RefreshCommand.Execute(null);
        //                                await DispatcherHelper.RunAsync(async () => await DialogService.ShowMessageBox($"Collection {Parent.Name} is now empty.", "Cleanup collection").ConfigureAwait(false));
        //                            }
        //                        }).ConfigureAwait(false);
        //                }));
        //    }
        //}

        public RelayCommand OpenImportDocumentCommand => _openImportDocumentCommand ??= new (() => Messenger.Send(new OpenImportDocumentViewMessage(this, Parent.Parent.Connection, Container)));

        public RelayCommand NewStoredProcedureCommand => _newStoredProcedureCommand ??= new(() => Messenger.Send(new EditStoredProcedureMessage(null, Parent.Parent.Connection, Container)));

        public RelayCommand NewUdfCommand => _newUdfCommand ??= new(() => Messenger.Send(new EditUserDefFuncMessage(null, Parent.Parent.Connection, Container)));

        public RelayCommand NewTriggerCommand => _newTriggerCommand ??= new(() => Messenger.Send(new EditTriggerMessage(null, Parent.Parent.Connection, Container)));

        //public RelayCommand DeleteCollectionCommand
        //{
        //    get
        //    {
        //        return _deleteCollectionCommand
        //            ?? (_deleteCollectionCommand = new RelayCommand(
        //                async () =>
        //                {
        //                    await DialogService.ShowMessage("Are you sure you want to delete this collection?", "Delete", null, null,
        //                        async confirm =>
        //                        {
        //                            if (confirm)
        //                            {
        //                                MessengerInstance.Send(new IsBusyMessage(true));
        //                                await DbService.DeleteCollectionAsync(Parent.Parent.Connection, Collection).ConfigureAwait(false);
        //                                MessengerInstance.Send(new IsBusyMessage(false));
        //                                await DispatcherHelper.RunAsync(() => Parent.Children.Remove(this));
        //                            }
        //                        }).ConfigureAwait(false);
        //                }
        //                ));
        //    }
        //}

        public ContainerNodeViewModel ContainerNode => this;

        public string ContentId => Guid.NewGuid().ToString();

        protected override void NotifyCanExecuteChanged()
        {
            OpenImportDocumentCommand.NotifyCanExecuteChanged();
            NewStoredProcedureCommand.NotifyCanExecuteChanged();
            NewUdfCommand.NotifyCanExecuteChanged();
            NewTriggerCommand.NotifyCanExecuteChanged();
            OpenSqlQueryCommand.NotifyCanExecuteChanged();
        }
    }
}
