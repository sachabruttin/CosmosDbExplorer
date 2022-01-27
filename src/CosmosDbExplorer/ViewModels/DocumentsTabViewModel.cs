using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.Validar;
using CosmosDbExplorer.ViewModels;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using CosmosDbExplorer.Core.Models;
using FluentValidation;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Validar;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Core.Contracts;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.ViewModels
{
    [InjectValidation]
    public class DocumentsTabViewModel : PaneWithZoomViewModel<DocumentNodeViewModel>
        , IHaveRequestOptions
        , IHaveSystemProperties
    {
        //private readonly IDialogService _dialogService;
        private readonly StatusBarItem _requestChargeStatusBarItem;
        private readonly StatusBarItem _progessBarStatusBarItem;
        private readonly IServiceProvider _serviceProvider;
        
        private JObject _currentDocument;

        private ICosmosDocumentService _cosmosDocumentService;
        private AsyncRelayCommand _loadMoreCommand;
        private AsyncRelayCommand _refreshLoadCommand;
        private RelayCommand _newDocumentCommand;
        private RelayCommand _resetRequestOptionsCommand;
        private AsyncRelayCommand _saveLocalCommand;
        private RelayCommand _closeFilterCommand;
        private AsyncRelayCommand _applyFilterCommand;
        private RelayCommand _editFilterCommand;
        private AsyncRelayCommand _deleteDocumentCommand;
        private AsyncRelayCommand _saveDocumentCommand;
        private RelayCommand _discardCommand;

        public DocumentsTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices)
            : base(uiServices)
        {
            _serviceProvider = serviceProvider;

            Title = "Documents";
            Header = Title;
            IconSource = App.Current.FindResource("DocumentIcon");

            EditorViewModel = new DocumentEditorViewModel();
            HeaderViewModel = new HeaderEditorViewModel { IsReadOnly = true };

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
            _progessBarStatusBarItem = new StatusBarItem(new StatusBarItemContextCancellableCommand { Value = IsRunning, IsVisible = IsRunning, IsCancellable = false }, StatusBarItemType.ProgessBar, "Progress", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_progessBarStatusBarItem);
        }

        public override async void Load(string contentId, DocumentNodeViewModel node, CosmosConnection connection, CosmosContainer container)
        {
            ContentId = contentId;
            Node = node;
            Connection = connection;
            Collection = container;
            PartitionKey = container.PartitionKeyPath;
            var split = Node.Parent.Container.SelfLink.Split(new char[] { '/' });
            ToolTip = $"{split[1]}>{split[3]}";
            AccentColor = Node.Parent.Parent.Parent.Connection.AccentColor;

            _cosmosDocumentService = ActivatorUtilities.CreateInstance<CosmosDocumentService>(_serviceProvider, connection, node.Parent.Parent.Database, container);

            await LoadDocuments(true, new CancellationToken()).ConfigureAwait(false);
        }

        protected override void OnDeactivated()
        {
            //    SimpleIoc.Default.Unregister(EditorViewModel);
            //    SimpleIoc.Default.Unregister(HeaderViewModel);

            //    base.Cleanup();
        }

        public DocumentNodeViewModel Node { get; protected set; }

        public string PartitionKey { get; set; }

        public ObservableCollection<ICosmosDocument> Documents { get; } = new();

        public ICosmosDocument? SelectedDocument { get; set; }

        public async void OnSelectedDocumentChanged()
        {
            if (SelectedDocument != null)
            {
                IsRunning = true;

                if (_currentDocument == null || (_currentDocument.SelectToken("_self").Value<string>() != SelectedDocument.SelfLink))
                {

                    //try
                    //{
                    var response = await _cosmosDocumentService.GetDocumentAsync(SelectedDocument, new CancellationToken());
                    _currentDocument = response.Items;
                    //}
                    //catch (DocumentClientException clientEx)
                    //{
                    //    await _dialogService.ShowError(clientEx.Parse(), "Error", null, null).ConfigureAwait(false);
                    //}
                    //catch (Exception ex)
                    //{
                    //    await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
                    //}

                    SetStatusBar(new StatusBarInfo(response));

                    EditorViewModel.SetText(_currentDocument, HideSystemProperties);
                    HeaderViewModel.SetText(response?.Headers, HideSystemProperties);

                    IsRunning = false;
                }

            }
            else
            {
                SetStatusBar(null);
            }

            NotifyCanExecuteChanged();
        }

        private void NotifyCanExecuteChanged()
        {
            NewDocumentCommand.NotifyCanExecuteChanged();
            LoadMoreCommand.NotifyCanExecuteChanged();
            RefreshLoadCommand.NotifyCanExecuteChanged();
            DiscardCommand.NotifyCanExecuteChanged();
            SaveDocumentCommand.NotifyCanExecuteChanged();
            DeleteDocumentCommand.NotifyCanExecuteChanged();
            EditFilterCommand.NotifyCanExecuteChanged();
            ApplyFilterCommand.NotifyCanExecuteChanged();
            SaveLocalCommand.NotifyCanExecuteChanged();
        }

        public string Filter { get; set; }

        public bool IsEditingFilter { get; set; }

        public bool HasMore { get; set; }
        public string? ContinuationToken { get; set; }

        public string? RequestCharge { get; set; }

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

        private void SetStatusBar(IStatusBarInfo? response)
        {
            RequestCharge = response == null
                ? null
                : $"Request Charge: {response.RequestCharge:N2}";
        }

        private async Task LoadDocuments(bool cleanContent, CancellationToken cancellationToken)
        {
            try
            {
                IsRunning = true;

                if (cleanContent)
                {
                    Documents.Clear();
                    ContinuationToken = null;
                }

                var result = await _cosmosDocumentService.GetDocumentsAsync(
                    Filter,
                    50, // MaxDocumentToRetrieve
                    ContinuationToken, cancellationToken);

                HasMore = result.HasMore;
                ContinuationToken = result.ContinuationToken;
                RequestCharge = $"Request Charge: {result.RequestCharge:N2}";

                foreach (var document in result.Items)
                {
                    Documents.Add(document);
                }

                //    RaisePropertyChanged(() => ItemsCount);
                //}
                //catch (DocumentClientException clientEx)
                //{
                //    await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null).ConfigureAwait(false);
                //}
                //catch (Exception ex)
                //{
                //    await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
                //}
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                IsRunning = false;
                NotifyCanExecuteChanged();
            }
        }

        public long TotalItemsCount { get; set; }
        public long ItemsCount => Documents.Count;

        public DocumentEditorViewModel EditorViewModel { get; }

        public HeaderEditorViewModel HeaderViewModel { get; }

        protected CosmosConnection Connection { get; set; }

        protected CosmosContainer Collection { get; set; }

        public AsyncRelayCommand LoadMoreCommand => _loadMoreCommand ??= new(async () => await LoadDocuments(false, new CancellationToken()).ConfigureAwait(false));

        public AsyncRelayCommand RefreshLoadCommand => _refreshLoadCommand ??= new(async () => await LoadDocuments(true, new CancellationToken()).ConfigureAwait(false),
                                                      () => !IsRunning && IsValid);


        public RelayCommand NewDocumentCommand => _newDocumentCommand ??= new(NewDocumentExecute, NewDocumentCommandCanExecute);

        private void NewDocumentExecute()
        {
            SelectedDocument = null;
            SetStatusBar(null);
            EditorViewModel.SetText(new CosmosDocument { Id = "replace_with_the_new_document_id" }, HideSystemProperties);
        }

        private bool NewDocumentCommandCanExecute()
        {
            // Can create new document if current document is not a new document
            return true;//!IsRunning && !EditorViewModel.IsNewDocument && !EditorViewModel.IsDirty;
        }

        public RelayCommand DiscardCommand => _discardCommand ??= new(() => OnSelectedDocumentChanged(), () => !IsRunning && EditorViewModel.IsDirty);

        public AsyncRelayCommand SaveDocumentCommand => _saveDocumentCommand ??= new(SaveDocumentCommandExecute, () => !IsRunning && EditorViewModel.IsDirty && IsValid);

        private async Task SaveDocumentCommandExecute()
        {
            IsRunning = true;
            try
            {
                //    var response = await _dbService.UpdateDocumentAsync(Connection, Collection.AltLink, EditorViewModel.Content.Text, this).ConfigureAwait(true);
                //    var document = response.Resource;

                //    SetStatusBar(new StatusBarInfo(response));

                //    var description = new DocumentDescription(document, Collection);

                //    if (SelectedDocument == null)
                //    {
                //        Documents.Add(description);
                //        SelectedDocument = description;
                //    }

                //    HeaderViewModel.SetText(response.ResponseHeaders, HideSystemProperties);
            }
            //catch (DocumentClientException ex)
            //{
            //    var message = ex.Parse();
            //    await _dialogService.ShowError(message, "Error", null, null).ConfigureAwait(false);
            //}
            //catch (Exception ex)
            //{
            //    await _dialogService.ShowError(ex.Message, "Error", null, null).ConfigureAwait(false);
            //}
            finally
            {
                IsRunning = false;
            }

        }

        public AsyncRelayCommand DeleteDocumentCommand => _deleteDocumentCommand ??= new(DeleteDocumentCommandExecute, () => true/*!IsRunning && SelectedDocument != null && !EditorViewModel.IsNewDocument && IsValid*/);

        private async Task DeleteDocumentCommandExecute()
        {
            //var selectedDocuments = Documents.Where(doc => doc.IsSelected).ToList();
            //var message = selectedDocuments.Count == 1
            //            ? $"Are you sure that you want to delete document '{selectedDocuments[0].Id}'?"
            //            : $"Are you sure that you want to delete these {selectedDocuments.Count} documents?";

            //await _dialogService.ShowMessage(message, "Delete Document(s)", null, null, async confirm =>
            //{
            //    if (confirm)
            //    {
            //        IsRunning = true;
            //        var response = await _dbService.DeleteDocumentsAsync(Node.Parent.Parent.Parent.Connection, selectedDocuments).ConfigureAwait(false);
            //        IsRunning = false;
            //        SetStatusBar(new StatusBarInfo(response));

            //        await DispatcherHelper.RunAsync(() =>
            //        {
            //            SelectedDocument = null;
            //            foreach (var item in selectedDocuments)
            //            {
            //                Documents.Remove(item);
            //            }

            //            EditorViewModel.SetText(new { result = "Delete operation succeeded!" }, HideSystemProperties);
            //        });
            //    }
            //}).ConfigureAwait(false);
        }

        public RelayCommand EditFilterCommand => _editFilterCommand ??= new(() => IsEditingFilter = true);

        public AsyncRelayCommand ApplyFilterCommand => _applyFilterCommand ??= new(async () => { IsEditingFilter = false; await LoadDocuments(true, new CancellationToken()).ConfigureAwait(false); });

        public RelayCommand CloseFilterCommand => _closeFilterCommand ??= new(() => IsEditingFilter = false);

        public AsyncRelayCommand SaveLocalCommand => _saveLocalCommand ??= new(SaveDocumentCommandExecute, () => !IsRunning && SelectedDocument != null && IsValid);

        private async Task SaveLocalCommandExecute()
        {
            //var selectedDocuments = Documents.Where(doc => doc.IsSelected).ToList();

            //if (selectedDocuments.Count == 1)
            //{
            //    await SaveLocalSingleDocumentAsync().ConfigureAwait(false);
            //}
            //else
            //{
            //    await SaveLocalMultipleDocumentsAsync(selectedDocuments).ConfigureAwait(false);
            //}
        }

        //private Task SaveLocalMultipleDocumentsAsync(List<DocumentDescription> selectedDocuments)
        //{
        //    var settings = new FolderBrowserDialogSettings
        //    {
        //        ShowNewFolderButton = true,
        //        Description = "Select output folder...",
        //        SelectedPath = Settings.Default.GetExportFolder()
        //    };

        //    return _dialogService.ShowFolderBrowserDialog(settings, async (confirm, result) =>
        //    {
        //        if (confirm)
        //        {
        //            try
        //            {
        //                IsRunning = true;

        //                // Save path for future use
        //                Settings.Default.ExportFolder = result.Path;
        //                Settings.Default.Save();

        //                var tasks = selectedDocuments.Select(doc => _dbService.GetDocumentAsync(Connection, doc));
        //                await Task.WhenAll(tasks).ConfigureAwait(false);

        //                foreach (var item in tasks)
        //                {
        //                    var document = item.Result;
        //                    File.WriteAllText(Path.Combine(result.Path, $"{document.Resource.Id}.json"), document.Resource.ToString());
        //                }
        //           }
        //            catch (Exception ex)
        //            {
        //                await _dialogService.ShowError(ex, "Error", null, null).ConfigureAwait(false);
        //            }
        //            finally
        //            {
        //                IsRunning = false;
        //            }
        //        }
        //    });
        //}

        //private Task SaveLocalSingleDocumentAsync()
        //{
        //    var settings = new SaveFileDialogSettings
        //    {
        //        DefaultExt = "json",
        //        Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
        //        AddExtension = true,
        //        FileName = $"{SelectedDocument.Id}.json",
        //        OverwritePrompt = true,
        //        CheckFileExists = false,
        //        Title = "Save document locally"
        //    };

        //    return _dialogService.ShowSaveFileDialog(settings, async (confirm, result) =>
        //    {
        //        if (confirm)
        //        {
        //            try
        //            {
        //                IsRunning = true;

        //                Settings.Default.ExportFolder = (new FileInfo(result.FileName)).DirectoryName;
        //                Settings.Default.Save();

        //                await DispatcherHelper.RunAsync(() => File.WriteAllText(result.FileName, EditorViewModel.Content.Text));
        //            }
        //            catch (Exception ex)
        //            {
        //                await _dialogService.ShowError(ex, "Error", null, null).ConfigureAwait(false);
        //            }
        //            finally
        //            {
        //                IsRunning = false;
        //            }
        //        }
        //    });
        //}

        public bool IsValid => string.IsNullOrEmpty(((IDataErrorInfo)this).Error);//!((INotifyDataErrorInfo)this).HasErrors;

        public bool HideSystemProperties { get; set; }

        public void OnHideSystemPropertiesChanged()
        {
            EditorViewModel.SetText(_currentDocument, HideSystemProperties);
            NotifyCanExecuteChanged();
        }

        public bool? EnableScanInQuery { get; set; }
        public bool? EnableCrossPartitionQuery { get; set; }
        public int? MaxItemCount { get; set; }
        public int? MaxDOP { get; set; }
        public int? MaxBufferItem { get; set; }

        public RelayCommand ResetRequestOptionsCommand => _resetRequestOptionsCommand ??= new(ResetRequestOptionCommandExecute);

        private void ResetRequestOptionCommandExecute()
        {
            //IndexingDirective = null;
            //ConsistencyLevel = null;
            PartitionKeyValue = null;
            //AccessConditionType = null;
            AccessCondition = null;
            PreTrigger = null;
            PostTrigger = null;
        }

        //public IndexingDirective? IndexingDirective { get; set; }
        //public ConsistencyLevel? ConsistencyLevel { get; set; }
        public string? PartitionKeyValue { get; set; }
        //public AccessConditionType? AccessConditionType { get; set; }
        public string? AccessCondition { get; set; }
        public string? PreTrigger { get; set; }
        public string? PostTrigger { get; set; }
    }

    public class DocumentsTabViewModelValidator : AbstractValidator<DocumentsTabViewModel>
    {
        public DocumentsTabViewModelValidator()
        {
            //When(x => !string.IsNullOrEmpty(x.PartitionKeyValue?.Trim()),
            //    () => RuleFor(x => x.PartitionKeyValue).SetValidator(new PartitionKeyValidator()));
        }
    }
}
