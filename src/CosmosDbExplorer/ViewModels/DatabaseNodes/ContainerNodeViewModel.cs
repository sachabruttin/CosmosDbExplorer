using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Messages;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class ContainerNodeViewModel : ResourceNodeViewModelBase<DatabaseNodeViewModel>, IHaveContainerNodeViewModel, IContent
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CosmosContainerService _containerService;
        private readonly IDialogService _dialogService;
        private RelayCommand _openImportDocumentCommand;
        private RelayCommand _newStoredProcedureCommand;
        private RelayCommand _newUdfCommand;
        private RelayCommand _newTriggerCommand;
        private RelayCommand _openSqlQueryCommand;
        private AsyncRelayCommand _deleteContainerCommand;

        private readonly StoredProcedureRootNodeViewModel _storedProcedureNode;
        private readonly UserDefFuncRootNodeViewModel _userDefFuncNode;
        private readonly TriggerRootNodeViewModel _triggerNode;

        public ContainerNodeViewModel(IServiceProvider serviceProvider, CosmosContainer container, DatabaseNodeViewModel parent)
            : base(container, parent, true)
        {
            _serviceProvider = serviceProvider;
            Container = container;

            _dialogService = _serviceProvider.GetRequiredService<IDialogService>();
            _containerService = ActivatorUtilities.CreateInstance<CosmosContainerService>(_serviceProvider, Parent.Parent.Connection, Parent.Database);

            _storedProcedureNode = new StoredProcedureRootNodeViewModel(this, _serviceProvider);
            _userDefFuncNode = new UserDefFuncRootNodeViewModel(this, _serviceProvider);
            _triggerNode = new TriggerRootNodeViewModel(this, _serviceProvider);
        }

        protected override Task LoadChildren(CancellationToken token)
        {
            Children.Add(new DocumentNodeViewModel(this));
            Children.Add(new ScaleSettingsNodeViewModel(this));
            Children.Add(_storedProcedureNode);
            Children.Add(_userDefFuncNode);
            Children.Add(_triggerNode);
            Children.Add(new MetricsNodeViewModel(this));

            return Task.CompletedTask;
        }

        public CosmosContainer Container { get; }

        public RelayCommand OpenSqlQueryCommand => _openSqlQueryCommand ??= new(() => Messenger.Send(new OpenQueryViewMessage(this, Parent.Parent.Connection, Parent.Database, Container)));

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

        public RelayCommand OpenImportDocumentCommand => _openImportDocumentCommand ??= new(() => Messenger.Send(new OpenImportDocumentViewMessage(this, Parent.Parent.Connection, Parent.Database, Container)));

        public RelayCommand NewStoredProcedureCommand => _newStoredProcedureCommand ??= new(() => Messenger.Send(new EditStoredProcedureMessage(null, Parent.Parent.Connection, Parent.Database, Container)));

        public RelayCommand NewUdfCommand => _newUdfCommand ??= new(() => Messenger.Send(new EditUserDefFuncMessage(null, Parent.Parent.Connection, Parent.Database, Container)));

        public RelayCommand NewTriggerCommand => _newTriggerCommand ??= new(() => Messenger.Send(new EditTriggerMessage(null, Parent.Parent.Connection, Parent.Database, Container)));

        public AsyncRelayCommand DeleteContainerCommand => _deleteContainerCommand ??= new(DeleteContainerCommandExecute);

        private async Task DeleteContainerCommandExecute()
        {
            async void OnDialogClose(bool confirm)
            {
                if (!confirm)
                {
                    return;
                }

                try
                {
                    await _containerService.DeleteContainserAsync(Container, new CancellationToken());
                    Messenger.Send(new RemoveNodeMessage(Container.SelfLink));
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowError(ex, $"Error during container {Container.Id} deletion");
                }
            }

            var msg = $"Are you sure you want to delete the container '{Container.Id}' and all his content?";
            await _dialogService.ShowQuestion(msg, "Delete Container", OnDialogClose);
        }

        public ContainerNodeViewModel ContainerNode => this;

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
