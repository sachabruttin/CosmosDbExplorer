using System;
using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Messages;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModel
{
    public class CollectionNodeViewModel : ResourceNodeViewModelBase<DatabaseNodeViewModel>, IHaveCollectionNodeViewModel, IContent
    {
        private RelayCommand _openSqlQueryCommand;
        private RelayCommand _openImportDocumentCommand;
        private RelayCommand _clearAllDocumentsCommand;
        private RelayCommand _newStoredProcedureCommand;
        private RelayCommand _newUdfCommand;
        private RelayCommand _newTriggerCommand;
        private RelayCommand _deleteCollectionCommand;
        private RelayCommand _recreateAsEmptyCommand;

        public CollectionNodeViewModel(DocumentCollection collection, DatabaseNodeViewModel parent)
            : base(collection, parent, true)
        {
            Collection = collection;
        }

        protected override async Task LoadChildren()
        {
            await DispatcherHelper.RunAsync(() =>
            {
                Children.Add(new DocumentNodeViewModel(this));
                Children.Add(new ScaleSettingsNodeViewModel(this));
                Children.Add(new StoredProcedureRootNodeViewModel(this));
                Children.Add(new UserDefFuncRootNodeViewModel(this));
                Children.Add(new TriggerRootNodeViewModel(this));
                Children.Add(new CollectionMetricsNodeViewModel(this));
            });
        }

        public DocumentCollection Collection { get; }

        public RelayCommand OpenSqlQueryCommand
        {
            get
            {
                return _openSqlQueryCommand
                    ?? (_openSqlQueryCommand = new RelayCommand(() => MessengerInstance.Send(new OpenQueryViewMessage(this, Parent.Parent.Connection, Collection))));
            }
        }

        public RelayCommand ClearAllDocumentsCommand
        {
            get
            {
                return _clearAllDocumentsCommand
                    ?? (_clearAllDocumentsCommand = new RelayCommand(
                        async () =>
                        {
                            await DialogService.ShowMessage($"All documents will be removed from the collection {Parent.Name}.\n\nIf you have a lot of documents, this could take a while and be costly and it is perhaps preferable to use the 'Recreate' option.\n\nAre you sure you want to continue?",
                                "Cleanup collection", null, null,
                                async confirm =>
                                {
                                    if (confirm)
                                    {
                                        MessengerInstance.Send(new IsBusyMessage(true));
                                        await DbService.CleanCollectionAsync(Parent.Parent.Connection, Collection).ConfigureAwait(false);
                                        MessengerInstance.Send(new IsBusyMessage(false));
                                        await DispatcherHelper.RunAsync(async () => await DialogService.ShowMessageBox($"Collection {Parent.Name} is now empty.", "Cleanup collection").ConfigureAwait(false));
                                    }
                                }).ConfigureAwait(false);
                        }));
            }
        }

        public RelayCommand RecreateAsEmptyCommand
        {
            get
            {
                return _recreateAsEmptyCommand
                    ?? (_recreateAsEmptyCommand = new RelayCommand(
                        async () =>
                        {
                            await DialogService.ShowMessage($"Collection will be deleted and recreated with the same parameters and assets:\n\t- Stored Procedures\n\t- Triggers\n\t- User Defined Functions\n\nThis is fast and cost efficient but could affect your application(s) availability.\n\nAre you sure you want to continue?",
                                "Recreate empty collection", null, null,
                                async confirm =>
                                {
                                    if (confirm)
                                    {
                                        MessengerInstance.Send(new IsBusyMessage(true));
                                        await DbService.RecreateCollectionAsync(Parent.Parent.Connection, Parent.Database, Collection).ConfigureAwait(false);
                                        MessengerInstance.Send(new IsBusyMessage(false));
                                        Parent.RefreshCommand.Execute(null);
                                        await DispatcherHelper.RunAsync(async () => await DialogService.ShowMessageBox($"Collection {Parent.Name} is now empty.", "Cleanup collection").ConfigureAwait(false));
                                    }
                                }).ConfigureAwait(false);
                        }));
            }
        }

        public RelayCommand OpenImportDocumentCommand
        {
            get
            {
                return _openImportDocumentCommand
                    ?? (_openImportDocumentCommand = new RelayCommand(
                       () => MessengerInstance.Send(new OpenImportDocumentViewMessage(this, Parent.Parent.Connection, Collection))));
            }
        }

        public RelayCommand NewStoredProcedureCommand
        {
            get
            {
                return _newStoredProcedureCommand
                    ?? (_newStoredProcedureCommand = new RelayCommand(
                        () => MessengerInstance.Send(new EditStoredProcedureMessage(null, Parent.Parent.Connection, Collection))
                        ));
            }
        }

        public RelayCommand NewUdfCommand
        {
            get
            {
                return _newUdfCommand
                    ?? (_newUdfCommand = new RelayCommand(
                        () => MessengerInstance.Send(new EditUserDefFuncMessage(null, Parent.Parent.Connection, Collection))
                        ));
            }
        }

        public RelayCommand NewTriggerCommand
        {
            get
            {
                return _newTriggerCommand
                    ?? (_newTriggerCommand = new RelayCommand(
                        () => MessengerInstance.Send(new EditTriggerMessage(null, Parent.Parent.Connection, Collection))
                        ));
            }
        }

        public RelayCommand DeleteCollectionCommand
        {
            get
            {
                return _deleteCollectionCommand
                    ?? (_deleteCollectionCommand = new RelayCommand(
                        async () =>
                        {
                            await DialogService.ShowMessage("Are you sure you want to delete this collection?", "Delete", null, null,
                                async confirm =>
                                {
                                    if (confirm)
                                    {
                                        MessengerInstance.Send(new IsBusyMessage(true));
                                        await DbService.DeleteCollectionAsync(Parent.Parent.Connection, Collection).ConfigureAwait(false);
                                        MessengerInstance.Send(new IsBusyMessage(false));
                                        await DispatcherHelper.RunAsync(() => Parent.Children.Remove(this));
                                    }
                                }).ConfigureAwait(false);
                        }
                        ));
            }
        }

        public CollectionNodeViewModel CollectionNode => this;

        public string ContentId => Guid.NewGuid().ToString();
    }
}
