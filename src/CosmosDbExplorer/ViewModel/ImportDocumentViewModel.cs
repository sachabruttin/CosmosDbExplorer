using System;
using System.IO;
using System.Threading;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Extensions;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Services;
using CosmosDbExplorer.Services.DialogSettings;
using CosmosDbExplorer.ViewModel.Interfaces;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CosmosDbExplorer.ViewModel
{
    public class ImportDocumentViewModel : PaneWithZoomViewModel<CollectionNodeViewModel>, IHaveRequestOptions
    {
        private RelayCommand _executeCommand;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private RelayCommand _openFileCommand;
        private RelayCommand _resetRequestOptionsCommand;
        private readonly StatusBarItem _progessBarStatusBarItem;
        private CancellationTokenSource _cancellationToken;
        private RelayCommand _cancelCommand;

        public ImportDocumentViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService, IUIServices uiServices)
            : base(messenger, uiServices)
        {
            Content = new TextDocument();
            _dialogService = dialogService;
            _dbService = dbService;

            _progessBarStatusBarItem = new StatusBarItem(new StatusBarItemContextCancellableCommand { Value = CancelCommand, IsVisible = IsRunning, IsCancellable = false }, StatusBarItemType.ProgessBar, "Progress", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_progessBarStatusBarItem);
        }

        public bool IsRunning { get; set; }

        public void OnIsRunningChanged()
        {
            _progessBarStatusBarItem.DataContext.IsVisible = IsRunning;

            if (IsRunning)
            {
                _cancellationToken = new CancellationTokenSource();
            }
            else
            {
                _cancellationToken = null;
            }
        }

        public override void Load(string contentId, CollectionNodeViewModel node, Connection connection, DocumentCollection collection)
        {
            ContentId = contentId;
            Node = node;
            Header = "Import";
            Connection = connection;
            Collection = collection;

            var split = Collection.AltLink.Split(new char[] { '/' });
            ToolTip = $"{split[1]}>{split[3]}";
            AccentColor = Connection.AccentColor;
        }

        public CollectionNodeViewModel Node { get; protected set; }

        protected Connection Connection { get; set; }

        protected DocumentCollection Collection { get; set; }

        public TextDocument Content { get; set; }

        public bool IsDirty { get; set; }

        public RelayCommand ExecuteCommand
        {
            get
            {
                return _executeCommand
                    ?? (_executeCommand = new RelayCommand(
                        async () =>
                        {
                            try
                            {
                                IsRunning = true;
                                var response = await _dbService.ImportDocumentAsync(Connection, Collection, Content.Text, this, _cancellationToken.Token).ConfigureAwait(false);
                                await _dialogService.ShowMessageBox($"{response.NumberOfDocumentsImported} document(s) imported!", "Import").ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                await _dialogService.ShowMessage("Operation cancelled by user...", "Cancel").ConfigureAwait(false);
                            }
                            catch (DocumentClientException clientEx)
                            {
                                await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
                            }
                            finally
                            {
                                IsRunning = false;
                            }
                        },
                        () => !IsRunning && !string.IsNullOrEmpty(Content?.Text)));
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(
                    () => _cancellationToken.Cancel(),
                    () => IsRunning));
            }
        }

        public RelayCommand OpenFileCommand
        {
            get
            {
                return _openFileCommand
                    ?? (_openFileCommand = new RelayCommand(
                        async () =>
                        {
                            var settings = new OpenFileDialogSettings
                            {
                                DefaultExt = "json",
                                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                                AddExtension = true,
                                CheckFileExists = true,
                                Multiselect = false,
                                Title = "Open document"
                            };

                            await _dialogService.ShowOpenFileDialog(settings,
                                async (confirm, result) =>
                                {
                                    if (confirm)
                                    {
                                        await DispatcherHelper.RunAsync(async () =>
                                        {
                                            using (var reader = File.OpenText(result.FileName))
                                            {
                                                Content.FileName = result.FileName;
                                                Content.Text = await reader.ReadToEndAsync().ConfigureAwait(true);
                                            }
                                        });
                                    }
                                }).ConfigureAwait(false);
                        }
                        ));
            }
        }

        public IndexingDirective? IndexingDirective { get; set; }
        public ConsistencyLevel? ConsistencyLevel { get; set; }
        public string PartitionKeyValue { get; set; }
        public AccessConditionType? AccessConditionType { get; set; }
        public string AccessCondition { get; set; }
        public string PreTrigger { get; set; }
        public string PostTrigger { get; set; }

        public RelayCommand ResetRequestOptionsCommand
        {
            get
            {
                return _resetRequestOptionsCommand
                    ?? (_resetRequestOptionsCommand = new RelayCommand(
                        () =>
                        {
                            IndexingDirective = null;
                            ConsistencyLevel = null;
                            PartitionKeyValue = null;
                            AccessConditionType = null;
                            AccessCondition = null;
                            PreTrigger = null;
                            PostTrigger = null;
                        }));
            }
        }
    }
}
