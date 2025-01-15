using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Contracts;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Extensions;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.Properties;
using CosmosDbExplorer.Services.DialogSettings;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

using Newtonsoft.Json.Linq;

using Validar;

namespace CosmosDbExplorer.ViewModels
{
    [InjectValidation]
    public class DocumentsTabViewModel : PaneWithZoomViewModel<DocumentNodeViewModel>
        , IHaveRequestOptions
        , IHaveSystemProperties
    {
        private readonly StatusBarItem _requestChargeStatusBarItem;
        private readonly StatusBarItem _progessBarStatusBarItem;
        private readonly IDialogService _dialogService;
        private JObject? _currentDocument;
        private ICosmosDocument? _currentCosmosDocument;

        private readonly ICosmosDocumentService _cosmosDocumentService;
        private AsyncRelayCommand? _loadMoreCommand;
        private AsyncRelayCommand? _refreshLoadCommand;
        private RelayCommand? _newDocumentCommand;
        private RelayCommand? _resetRequestOptionsCommand;
        private RelayCommand? _saveLocalCommand;
        private RelayCommand? _closeFilterCommand;
        private AsyncRelayCommand? _applyFilterCommand;
        private RelayCommand? _editFilterCommand;
        private AsyncRelayCommand? _deleteDocumentCommand;
        private AsyncRelayCommand? _saveDocumentCommand;
        private RelayCommand? _discardCommand;
        private readonly IDocumentRequestOptions _documentRequestOptions = new DocumentRequestOptions();

        public DocumentsTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices, IDialogService dialogService, string contentId, NodeContext<DocumentNodeViewModel> nodeContext)
            : base(uiServices, contentId, nodeContext)
        {
            _dialogService = dialogService;
            Title = "Documents";
            Header = Title;
            IconSource = App.Current.FindResource("DocumentIcon");

            EditorViewModel = new DocumentEditorViewModel();
            EditorViewModel.PropertyChanged += (s, e) => NotifyCanExecuteChanged();

            HeaderViewModel = new HeaderEditorViewModel { IsReadOnly = true };

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
            _progessBarStatusBarItem = new StatusBarItem(new StatusBarItemContextCancellableCommand { Value = IsRunning, IsVisible = IsRunning, IsCancellable = false }, StatusBarItemType.ProgessBar, "Progress", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_progessBarStatusBarItem);

            if (nodeContext.Node is null || nodeContext.Connection is null || nodeContext.Container is null || nodeContext.Database is null)
            {
                throw new NullReferenceException("Node context is not correctly initialized!");
            }

            Node = nodeContext.Node;
            Connection = nodeContext.Connection;
            Container = nodeContext.Container;
            PartitionKey = Container.PartitionKeyPath;

            //var split = Node.Parent.Container.SelfLink.Split(new char[] { '/' });
            ToolTip = $"{Connection.Label}/{nodeContext.Database.Id}/{Container.Id}";

            AccentColor = Connection.AccentColor;

            _cosmosDocumentService = ActivatorUtilities.CreateInstance<CosmosDocumentService>(serviceProvider, Connection, nodeContext.Database, Container);
        }

        public override Task InitializeAsync()
        {
            return LoadDocuments(true, new CancellationToken());
        }

        public DocumentNodeViewModel Node { get; protected set; }

        public IList<string> PartitionKey { get; set; }

        public ObservableCollection<CheckedItem<ICosmosDocument>> Documents { get; } = new();

        public ICosmosDocument? SelectedDocument { get; set; }

        public async void OnSelectedDocumentChanged()
        {
            if (SelectedDocument != null && SelectedDocument != _currentCosmosDocument)
            {
                IsRunning = true;

                try
                {
                    var response = await _cosmosDocumentService.GetDocumentAsync(SelectedDocument, _documentRequestOptions, new CancellationToken());
                    _currentDocument = response.Items;
#pragma warning disable CS8604 // Possible null reference argument.
                    _currentCosmosDocument = CosmosDocument.CreateFrom(response.Items, Container.PartitionKeyJsonPath);
#pragma warning restore CS8604 // Possible null reference argument.
                    SetStatusBar(new StatusBarInfo(response));

                    EditorViewModel.SetText(_currentDocument, HideSystemProperties);
                    HeaderViewModel.SetText(response?.Headers, HideSystemProperties);
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowError(ex, "Error");
                }
                finally
                {
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

        public string? Filter { get; set; }

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

        public CosmosIndexingDirectives? IndexingDirective
        {
            get { return _documentRequestOptions.IndexingDirective; }
            set
            {
                _documentRequestOptions.IndexingDirective = value;
                OnPropertyChanged();
            }
        }
        public CosmosConsistencyLevels? ConsistencyLevel
        {
            get { return _documentRequestOptions.ConsistencyLevel; }
            set
            {
                _documentRequestOptions.ConsistencyLevel = value;
                OnPropertyChanged();
            }
        }

        public CosmosAccessConditionType AccessConditionType
        {
            get { return _documentRequestOptions.AccessCondition; }
            set
            {
                _documentRequestOptions.AccessCondition = value;
                OnPropertyChanged();
            }
        }

        public string? AccessCondition
        {
            get { return _documentRequestOptions.ETag; }
            set
            {
                _documentRequestOptions.ETag = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                OnPropertyChanged();
            }
        }

        public string? PreTrigger
        {
            get { return string.Join("; ", _documentRequestOptions.PreTriggers); }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _documentRequestOptions.PreTriggers = Array.Empty<string>();
                }
                else
                {
                    _documentRequestOptions.PreTriggers = value.Split(';', StringSplitOptions.TrimEntries);
                }
            }
        }

        public string? PostTrigger
        {
            get { return string.Join("; ", _documentRequestOptions.PostTriggers); }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _documentRequestOptions.PostTriggers = Array.Empty<string>();
                }
                else
                {
                    _documentRequestOptions.PostTriggers = value.Split(';', StringSplitOptions.TrimEntries);
                }
            }
        }

        public bool IsValid => string.IsNullOrEmpty(((IDataErrorInfo)this).Error);

        public bool HideSystemProperties { get; set; }

        protected void OnHideSystemPropertiesChanged()
        {
            EditorViewModel.SetText(_currentDocument, HideSystemProperties);
            NotifyCanExecuteChanged();
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
                    Settings.Default.MaxDocumentToRetrieve,
                    ContinuationToken, cancellationToken);

                HasMore = result.HasMore;
                ContinuationToken = result.ContinuationToken;
                RequestCharge = $"Request Charge: {result.RequestCharge:N2}";

                if (result.Items is not null)
                {
                    foreach (var document in result.Items)
                    {
                        Documents.Add(new CheckedItem<ICosmosDocument>(document));
                    }
                }
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

        protected CosmosContainer Container { get; set; }

        public AsyncRelayCommand LoadMoreCommand => _loadMoreCommand ??= new(async () => await LoadDocuments(false, new CancellationToken()));

        public AsyncRelayCommand RefreshLoadCommand => _refreshLoadCommand ??= new(async () => await LoadDocuments(true, new CancellationToken()),
                                                      () => !IsRunning && IsValid);


        public RelayCommand NewDocumentCommand => _newDocumentCommand ??= new(NewDocumentExecute, NewDocumentCommandCanExecute);

        private void NewDocumentExecute()
        {
            SelectedDocument = null;
            SetStatusBar(null);

            var json = new JObject(new JProperty("id", "replace_with_new_document_id"));

            foreach (var item in Container.PartitionKeyPath)
            {
                var parts = item.Split('/', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0)
                {
                    var current = json;
                    for (var i = 0; i < parts.Length; i++)
                    {
                        if (i == parts.Length - 1)
                        {
                            current[parts[i]] = "replace_with_new_partition_key_value";
                        }
                        else
                        {
                            if (current[parts[i]] is null)
                            {
                                current[parts[i]] = new JObject();
                            }
                            current = (JObject)current[parts[i]];
                        }
                    }
                }
            }

            EditorViewModel.SetText(json, HideSystemProperties);
        }

        private bool NewDocumentCommandCanExecute()
        {
            // Can create new document if current document is not a new document
            return !IsRunning && !EditorViewModel.IsNewDocument && !EditorViewModel.IsDirty;
        }

        public RelayCommand DiscardCommand => _discardCommand ??= new(() => OnSelectedDocumentChanged(), () => !IsRunning && EditorViewModel.IsDirty);

        public AsyncRelayCommand SaveDocumentCommand => _saveDocumentCommand ??= new(SaveDocumentCommandExecute, () => !IsRunning && EditorViewModel.IsDirty && IsValid);

        private async Task SaveDocumentCommandExecute()
        {
            if (EditorViewModel?.Text is null)
            {
                return;
            }

            IsRunning = true;
            try
            {
                var response = await _cosmosDocumentService.SaveDocumentAsync(EditorViewModel.Text, _documentRequestOptions, new CancellationToken());

                SetStatusBar(new StatusBarInfo(response));

#pragma warning disable CS8604 // Possible null reference argument.
                _currentCosmosDocument = CosmosDocument.CreateFrom(response.Items, Container?.PartitionKeyJsonPath);
#pragma warning restore CS8604 // Possible null reference argument.

                if (SelectedDocument == null)
                {
                    Documents.Add(new CheckedItem<ICosmosDocument>(_currentCosmosDocument));
                }
                else
                {
                    Documents.Select(d => d.Item).Replace(SelectedDocument, _currentCosmosDocument);
                }

                SelectedDocument = _currentCosmosDocument;

                EditorViewModel.SetText(response.Items, HideSystemProperties);
                HeaderViewModel.SetText(response.Headers, HideSystemProperties);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowError(ex.Message, "Error saving document");
            }
            finally
            {
                IsRunning = false;
            }
        }

        public AsyncRelayCommand DeleteDocumentCommand => _deleteDocumentCommand ??= new(DeleteDocumentCommandExecute, () => !IsRunning && SelectedDocument != null && !EditorViewModel.IsNewDocument && IsValid);

        private async Task DeleteDocumentCommandExecute()
        {
            var selectedDocuments = Documents.Where(doc => doc.IsChecked).Select(doc => doc.Item).ToList();
            var message = selectedDocuments.Count == 1
                        ? $"Are you sure that you want to delete document '{selectedDocuments[0].Id}'?"
                        : $"Are you sure that you want to delete these {selectedDocuments.Count} documents?";

            async void deleteDocument(bool confirm)
            {
                if (!confirm)
                {
                    return;
                }

                IsRunning = true;

                try
                {
                    var response = await _cosmosDocumentService.DeleteDocumentsAsync(selectedDocuments, new CancellationToken());
                    SetStatusBar(new StatusBarInfo(response));

                    var toRemove = Documents.Where(doc => doc.IsChecked).ToList();
                    SelectedDocument = null;

                    foreach (var item in toRemove)
                    {
                        Documents.Remove(item);
                    }

                    EditorViewModel.Clear();
                    HeaderViewModel.Clear();
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowError(ex, "Unable to delete document");
                }
                finally
                {
                    IsRunning = false;
                }
            }

            await _dialogService.ShowQuestion(message, "Delete Document(s)", deleteDocument);
        }

        public RelayCommand EditFilterCommand => _editFilterCommand ??= new(() => IsEditingFilter = true);

        public AsyncRelayCommand ApplyFilterCommand => _applyFilterCommand ??= new(async () => { IsEditingFilter = false; await LoadDocuments(true, new CancellationToken()).ConfigureAwait(false); });

        public RelayCommand CloseFilterCommand => _closeFilterCommand ??= new(() => IsEditingFilter = false);

        public RelayCommand SaveLocalCommand => _saveLocalCommand ??= new(SaveLocalCommandExecute, () => !IsRunning && SelectedDocument != null && IsValid);

        private void SaveLocalCommandExecute()
        {
            var selectedDocuments = Documents.Where(doc => doc.IsChecked).ToList();

            if (selectedDocuments.Count == 1)
            {
                SaveLocalSingleDocument();
            }
            else
            {
                SaveLocalMultipleDocuments(selectedDocuments.Select(doc => doc.Item).ToList());
            }
        }

        private void SaveLocalMultipleDocuments(List<ICosmosDocument> selectedDocuments)
        {
            var settings = new FolderBrowserDialogSettings
            {
                ShowNewFolderButton = true,
                Description = "Select output folder...",
                SelectedPath = Settings.Default.GetExportFolder()
            };

            async void saveFile(bool confirm, FolderDialogResult result)
            {
                if (!confirm)
                {
                    return;
                }
                try
                {
                    IsRunning = true;

                    // Save path for future use
                    Settings.Default.ExportFolder = result.Path;
                    Settings.Default.Save();

                    var tasks = new List<Task<CosmosQueryResult<JObject>>>(selectedDocuments.Count);

                    foreach (var document in selectedDocuments)
                    {
                        tasks.Add(_cosmosDocumentService.GetDocumentAsync(document, _documentRequestOptions, new CancellationToken())
                            .ContinueWith<CosmosQueryResult<JObject>>(requestResult =>
                            {
                                if (requestResult.IsCompletedSuccessfully)
                                {
                                    // TODO: Implement this
                                    throw new NotImplementedException();
                                    //var path = document.HasPartitionKey
                                    //    ? Path.Combine(result.Path, $"{document.PartitionKey}-{document.Id}.json".SafeForFilename())
                                    //    : Path.Combine(result.Path, $"{document.Id}.json".SafeForFilename());

                                    //File.WriteAllTextAsync(path, requestResult.Result.Items.ToString());
                                }

                                return requestResult.Result;
                            }));
                    }

                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowError(ex, "Error");
                }
                finally
                {
                    IsRunning = false;
                }
            }

            _dialogService.ShowFolderBrowserDialog(settings, saveFile);
        }

        private void SaveLocalSingleDocument()
        {
            var settings = new SaveFileDialogSettings
            {
                DefaultExt = "json",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                AddExtension = true,
                FileName = $"{SelectedDocument?.Id}.json",
                OverwritePrompt = true,
                CheckFileExists = false,
                Title = "Save document locally"
            };

            async void saveFile(bool confirm, FileDialogResult result)
            {
                if (!confirm)
                {
                    return;
                }

                try
                {
                    IsRunning = true;

                    Settings.Default.ExportFolder = (new FileInfo(result.FileName)).DirectoryName;
                    Settings.Default.Save();

                    await File.WriteAllTextAsync(result.FileName, EditorViewModel.Text);
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowError(ex, "Error");
                }
                finally
                {
                    IsRunning = false;
                }
            }

            _dialogService.ShowSaveFileDialog(settings, saveFile);
        }

        public RelayCommand ResetRequestOptionsCommand => _resetRequestOptionsCommand ??= new(ResetRequestOptionCommandExecute);

        private void ResetRequestOptionCommandExecute()
        {
            IndexingDirective = null;
            ConsistencyLevel = null;
            //PartitionKeyValue = null;
            AccessConditionType = CosmosAccessConditionType.None;
            AccessCondition = null;
            PreTrigger = null;
            PostTrigger = null;
        }


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
