using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class CollectionNodeViewModel : TreeViewItemViewModel
    {
        private readonly DocumentCollection _collection;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private RelayCommand _openSqlQueryCommand;
        private RelayCommand _openImportDocumentCommand;
        private RelayCommand _clearAllDocumentsCommand;
        private RelayCommand _newStoredProcedureCommand;
        private RelayCommand _newUdfCommand;
        private RelayCommand _newTriggerCommand;
        private RelayCommand _deleteCollectionCommand;

        public CollectionNodeViewModel(DocumentCollection collection, DatabaseNodeViewModel parent) 
            : base(parent, parent.MessengerInstance, true)
        {
            Name = collection.Id;
            _collection = collection;
            _dialogService = SimpleIoc.Default.GetInstance<IDialogService>();
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
        }

        public string Name { get; set; }

        public new DatabaseNodeViewModel Parent
        {
            get { return base.Parent as DatabaseNodeViewModel; }
        }

        protected override async Task LoadChildren()
        {
            await DispatcherHelper.RunAsync(() =>
            {
                Children.Add(new DocumentNodeViewModel(this));
                Children.Add(new ScaleSettingsNodeViewModel(this) { Name = "Scale & Settings" });
                Children.Add(new StoredProcedureRootNodeViewModel(this));
                Children.Add(new UserDefFuncRootNodeViewModel(this));
                Children.Add(new TriggerRootNodeViewModel(this));
            });
        }

        public DocumentCollection Collection { get { return _collection; } }

        public RelayCommand OpenSqlQueryCommand
        {
            get
            {
                return _openSqlQueryCommand
                    ?? (_openSqlQueryCommand = new RelayCommand(
                        x =>
                        {
                            MessengerInstance.Send(new OpenQueryViewMessage(this));
                        }));
            }
        }

        public RelayCommand ClearAllDocumentsCommand
        {
            get
            {
                return _clearAllDocumentsCommand
                    ?? (_clearAllDocumentsCommand = new RelayCommand(
                        async x =>
                        {
                            await _dialogService.ShowMessage($"All documents will be removed from the collection {Parent.Name} .\n\nAre you sure you want to continue?",
                                "Cleanup collection", null, null,
                                async confirm =>
                                {
                                    if (confirm)
                                    {
                                        // TODO: Show Please wait...
                                        await _dbService.CleanCollection(Parent.Parent.Connection, Collection);
                                        await _dialogService.ShowMessageBox($"Collection {Parent.Name} is now empty.", "Cleanup collection");
                                    }
                                });
                        }));
            }
        }

        public RelayCommand OpenImportDocumentCommand
        {
            get
            {
                return _openImportDocumentCommand
                    ?? (_openImportDocumentCommand = new RelayCommand(
                       x => MessengerInstance.Send(new OpenImportDocumentViewMessage(this))));
            }
        }

        public RelayCommand NewStoredProcedureCommand
        {
            get
            {
                return _newStoredProcedureCommand
                    ?? (_newStoredProcedureCommand = new RelayCommand(
                        x => MessengerInstance.Send(new EditStoredProcedureMessage(this, null))
                        ));
            }
        }

        public RelayCommand NewUdfCommand
        {
            get
            {
                return _newUdfCommand
                    ?? (_newUdfCommand = new RelayCommand(
                        x => MessengerInstance.Send(new EditUserDefFuncMessage(this, null))
                        ));
            }
        }

        public RelayCommand NewTriggerCommand
        {
            get
            {
                return _newTriggerCommand
                    ?? (_newTriggerCommand = new RelayCommand(
                        x => MessengerInstance.Send(new EditTriggerMessage(this, null))
                        ));
            }
        }

        public RelayCommand DeleteCollectionCommand
        {
            get
            {
                return _deleteCollectionCommand
                    ?? (_deleteCollectionCommand = new RelayCommand(
                        async x =>
                        {
                            await _dialogService.ShowMessage("Are you sure you want to delete this collection?", "Delete", null, null,
                                async confirm =>
                                {
                                    await _dbService.DeleteCollection(Parent.Parent.Connection, Collection);
                                    await DispatcherHelper.RunAsync(() => Parent.Children.Remove(this));
                                });
                        }
                        ));
            }
        }
    }
}
