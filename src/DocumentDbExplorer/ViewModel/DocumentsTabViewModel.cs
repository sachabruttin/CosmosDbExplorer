using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Extensions;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Properties;
using CosmosDbExplorer.Services;
using CosmosDbExplorer.Services.DialogSettings;
using CosmosDbExplorer.ViewModel.Interfaces;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CosmosDbExplorer.ViewModel
{
    public class DocumentsTabViewModel : PaneWithZoomViewModel<DocumentNodeViewModel>, IHaveQuerySettings, IHaveRequestOptions
    {
        private readonly IDocumentDbService _dbService;
        private readonly IDialogService _dialogService;
        private readonly StatusBarItem _requestChargeStatusBarItem;
        private readonly StatusBarItem _progessBarStatusBarItem;
        private RelayCommand _loadMoreCommand;
        private RelayCommand _refreshLoadCommand;
        private RelayCommand _newDocumentCommand;
        private RelayCommand _discardCommand;
        private RelayCommand _saveDocumentCommand;
        private RelayCommand _deleteDocumentCommand;
        private RelayCommand _editFilterCommand;
        private RelayCommand _applyFilterCommand;
        private RelayCommand _closeFilterCommand;
        private RelayCommand _saveLocalCommand;
        private ResourceResponse<Document> _currentDocument;
        private RelayCommand _resetRequestOptionsCommand;

        public DocumentsTabViewModel(IMessenger messenger, IDocumentDbService dbService, IDialogService dialogService, IUIServices uiServices)
            : base(messenger, uiServices)
        {
            Documents = new ObservableCollection<DocumentDescription>();
            _dbService = dbService;
            _dialogService = dialogService;

            EditorViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<DocumentEditorViewModel>();
            HeaderViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<HeaderEditorViewModel>();
            HeaderViewModel.IsReadOnly = true;

            Title = "Documents";
            Header = Title;

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
            _progessBarStatusBarItem = new StatusBarItem(new StatusBarItemContextCancellableCommand { Value = IsRunning, IsVisible = IsRunning, IsCancellable = false }, StatusBarItemType.ProgessBar, "Progress", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_progessBarStatusBarItem);
        }

        public override async void Load(string contentId, DocumentNodeViewModel node, Connection connection, DocumentCollection collection)
        {
            ContentId = contentId;
            Node = node;
            Connection = connection;
            Collection = collection;
            PartitionKey = collection.PartitionKey?.Paths.FirstOrDefault();
            var split = Node.Parent.Collection.AltLink.Split(new char[] { '/' });
            ToolTip = $"{split[1]}>{split[3]}";
            AccentColor = Node.Parent.Parent.Parent.Connection.AccentColor;

            await LoadDocuments(true).ConfigureAwait(false);
        }

        public DocumentNodeViewModel Node { get; protected set; }

        public string PartitionKey { get; set; }

        public ObservableCollection<DocumentDescription> Documents { get; }

        public DocumentDescription SelectedDocument { get; set; }

        public async void OnSelectedDocumentChanged()
        {
            if (SelectedDocument != null)
            {
                IsRunning = true;

                if (_currentDocument?.Resource.Id != SelectedDocument.Id)
                {
                    try
                    {
                        _currentDocument = await _dbService.GetDocumentAsync(Node.Parent.Parent.Parent.Connection, SelectedDocument).ConfigureAwait(false);
                    }
                    catch (DocumentClientException clientEx)
                    {
                        await _dialogService.ShowError(clientEx.Parse(), "Error", null, null).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
                    }
                }

                SetStatusBar(_currentDocument);
                IsRunning = false;
            }
            else
            {
                SetStatusBar(null);
            }
        }

        public string Filter { get; set; }

        public bool IsEditingFilter { get; set; }

        public bool HasMore { get; set; }
        public string ContinuationToken { get; set; }

        public string RequestCharge { get; set; }

        public void OnRequestChargeChanged()
        {
            _requestChargeStatusBarItem.DataContext.Value = RequestCharge;
        }

        public bool IsRunning { get; set; }

        public void OnIsRunningChanged()
        {
            _progessBarStatusBarItem.DataContext.IsVisible = IsRunning;
            _requestChargeStatusBarItem.DataContext.IsVisible = !IsRunning;
        }

        private void SetStatusBar(ResourceResponse<Document> response)
        {
            RequestCharge = response != null
                ? $"Request Charge: {response.RequestCharge:N2}"
                : null;

            EditorViewModel.SetText(response?.Resource, HideSystemProperties);
            HeaderViewModel.SetText(response?.ResponseHeaders, HideSystemProperties);
        }

        private async Task LoadDocuments(bool cleanContent = false)
        {
            try
            {
                IsRunning = true;

                if (cleanContent)
                {
                    Documents.Clear();
                    ContinuationToken = null;
                }

                var list = await _dbService.GetDocumentsAsync(Connection,
                                                               Collection,
                                                               Filter,
                                                               Settings.Default.MaxDocumentToRetrieve,
                                                               ContinuationToken)
                                                               .ConfigureAwait(true);

                HasMore = list.HasMore;
                ContinuationToken = list.ContinuationToken;
                RequestCharge = $"Request Charge: {list.RequestCharge:N2}";

                foreach (var document in list)
                {
                    Documents.Add(document);
                }

                TotalItemsCount = list.CollectionSize;
                RaisePropertyChanged(() => ItemsCount);
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
        }

        public long TotalItemsCount { get; set; }
        public long ItemsCount => Documents.Count;

        public DocumentEditorViewModel EditorViewModel { get; set; }

        public HeaderEditorViewModel HeaderViewModel { get; set; }

        protected Connection Connection { get; set; }

        protected DocumentCollection Collection { get; set; }

        public RelayCommand LoadMoreCommand
        {
            get
            {
                return _loadMoreCommand
                    ?? (_loadMoreCommand = new RelayCommand(
                        async () => await LoadDocuments(false).ConfigureAwait(false)));
            }
        }

        public RelayCommand RefreshLoadCommand
        {
            get
            {
                return _refreshLoadCommand
                    ?? (_refreshLoadCommand = new RelayCommand(
                        async () => await LoadDocuments(true).ConfigureAwait(false),
                        () => !IsRunning));
            }
        }

        public RelayCommand NewDocumentCommand
        {
            get
            {
                return _newDocumentCommand
                    ?? (_newDocumentCommand = new RelayCommand(
                        () =>
                        {
                            SelectedDocument = null;
                            SetStatusBar(null);
                            EditorViewModel.SetText(new Document() { Id = "replace_with_the_new_document_id" }, HideSystemProperties);
                        },
                        () =>
                        {
                            // Can create new document if current document is not a new document
                            return !IsRunning && !EditorViewModel.IsNewDocument && !EditorViewModel.IsDirty;
                        }));
            }
        }

        public RelayCommand DiscardCommand
        {
            get
            {
                return _discardCommand
                    ?? (_discardCommand = new RelayCommand(
                        () => OnSelectedDocumentChanged(),
                        () => !IsRunning && EditorViewModel.IsDirty));
            }
        }

        public RelayCommand SaveDocumentCommand
        {
            get
            {
                return _saveDocumentCommand
                    ?? (_saveDocumentCommand = new RelayCommand(
                        async () =>
                        {
                            IsRunning = true;
                            try
                            {
                                var response = await _dbService.UpdateDocumentAsync(Connection, Collection.AltLink, EditorViewModel.Content.Text, this).ConfigureAwait(false);
                                var document = response.Resource;

                                SetStatusBar(response);

                                var description = new DocumentDescription(document, Collection);

                                if (SelectedDocument == null)
                                {
                                    Documents.Add(description);
                                    SelectedDocument = description;
                                }

                                HeaderViewModel.SetText(response.ResponseHeaders, HideSystemProperties);
                            }
                            catch (DocumentClientException ex)
                            {
                                var message = ex.Parse();
                                await _dialogService.ShowError(message, "Error", null, null).ConfigureAwait(false);
                            }
                            finally
                            {
                                IsRunning = false;
                            }
                        },
                        () => !IsRunning && EditorViewModel.IsDirty));
            }
        }

        public RelayCommand DeleteDocumentCommand
        {
            get
            {
                return _deleteDocumentCommand
                    ?? (_deleteDocumentCommand = new RelayCommand(
                        async () =>
                        {
                            var documentId = SelectedDocument.Id;

                            await _dialogService.ShowMessage($"Are you sure that you want to delete document '{documentId}'?", "Delete Document", null, null, async confirm =>
                            {
                                if (confirm)
                                {
                                    IsRunning = true;
                                    var response = await _dbService.DeleteDocumentAsync(Node.Parent.Parent.Parent.Connection, SelectedDocument).ConfigureAwait(false);
                                    IsRunning = false;
                                    SetStatusBar(response);

                                    await DispatcherHelper.RunAsync(() =>
                                    {
                                        Documents.Remove(SelectedDocument);
                                        SelectedDocument = null;

                                        SetStatusBar(response);
                                        EditorViewModel.SetText(new { result = $"Document '{documentId}' deleted" }, HideSystemProperties);
                                    });
                                }
                            }).ConfigureAwait(false);
                        },
                        () => !IsRunning && SelectedDocument != null && !EditorViewModel.IsNewDocument));
            }
        }

        public RelayCommand EditFilterCommand
        {
            get
            {
                return _editFilterCommand
                    ?? (_editFilterCommand = new RelayCommand(
                        () => IsEditingFilter = true));
            }
        }

        public RelayCommand ApplyFilterCommand
        {
            get
            {
                return _applyFilterCommand
                    ?? (_applyFilterCommand = new RelayCommand(
                        async () =>
                        {
                            IsEditingFilter = false;
                            await LoadDocuments(true).ConfigureAwait(false);
                        }));
            }
        }

        public RelayCommand CloseFilterCommand
        {
            get
            {
                return _closeFilterCommand
                    ?? (_closeFilterCommand = new RelayCommand(
                        () => IsEditingFilter = false));
            }
        }

        public RelayCommand SaveLocalCommand
        {
            get
            {
                return _saveLocalCommand ??
                (_saveLocalCommand = new RelayCommand(
                    async () =>
                    {
                        var settings = new SaveFileDialogSettings
                        {
                            DefaultExt = "json",
                            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                            AddExtension = true,
                            FileName = $"{SelectedDocument.Id}.json",
                            OverwritePrompt = true,
                            CheckFileExists = false,
                            Title = "Save document locally"
                        };

                        await _dialogService.ShowSaveFileDialog(settings, async (confirm, result) =>
                        {
                            if (confirm)
                            {
                                try
                                {
                                    IsRunning = true;
                                    await DispatcherHelper.RunAsync(() => File.WriteAllText(result.FileName, EditorViewModel.Content.Text));
                                    }
                                    catch (Exception ex)
                                    {
                                        await _dialogService.ShowError(ex, "Error", null, null).ConfigureAwait(false);
                                    }
                                    finally
                                    {
                                        IsRunning = false;
                                    }
                            }
                        }).ConfigureAwait(false);
                    },
                    () => !IsRunning && SelectedDocument != null));
            }
        }

        public bool HideSystemProperties { get; set; } = true;

        public void OnHideSystemPropertiesChanged()
        {
            OnSelectedDocumentChanged();
        }

        public bool? EnableScanInQuery { get; set; }
        public bool? EnableCrossPartitionQuery { get; set; }
        public int? MaxItemCount { get; set; }
        public int? MaxDOP { get; set; }
        public int? MaxBufferItem { get; set; }

        public RelayCommand ResetRequestOptionsCommand
        {
            get
            {
                return _resetRequestOptionsCommand
                    ?? (_resetRequestOptionsCommand = new RelayCommand(
                        () =>
                        {
                            var instance = ((IHaveRequestOptions)this);
                            instance.IndexingDirective = null;
                            instance.ConsistencyLevel = null;
                            instance.PartitionKey = null;
                            instance.AccessConditionType = null;
                            instance.AccessCondition = null;
                            instance.PreTrigger = null;
                            instance.PostTrigger = null;
                        }));
            }
        }

        IndexingDirective? IHaveRequestOptions.IndexingDirective { get; set; }
        ConsistencyLevel? IHaveRequestOptions.ConsistencyLevel { get; set; }
        string IHaveRequestOptions.PartitionKey { get; set; }
        AccessConditionType? IHaveRequestOptions.AccessConditionType { get; set; }
        string IHaveRequestOptions.AccessCondition { get; set; }
        string IHaveRequestOptions.PreTrigger { get; set; }
        string IHaveRequestOptions.PostTrigger { get; set; }
    }
}
