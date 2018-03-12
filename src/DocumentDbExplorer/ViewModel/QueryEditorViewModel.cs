using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Extensions;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Services;
using DocumentDbExplorer.Services.DialogSettings;
using DocumentDbExplorer.ViewModel.Interfaces;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace DocumentDbExplorer.ViewModel
{
    public class QueryEditorViewModel : PaneWithZoomViewModel<CollectionNodeViewModel>, IHaveQuerySettings
    {
        private RelayCommand _executeCommand;
        private readonly IDocumentDbService _dbService;
        private readonly IDialogService _dialogService;
        private RelayCommand _saveLocalCommand;
        private FeedResponse<dynamic> _queryResult;
        private RelayCommand _goToNextPageCommand;
        private readonly StatusBarItem _requestChargeStatusBarItem;
        private readonly StatusBarItem _queryInformationStatusBarItem;
        private readonly StatusBarItem _progessBarStatusBarItem;
        private CancellationTokenSource _cancellationToken;
        private RelayCommand _cancelCommand;

        public QueryEditorViewModel(IMessenger messenger, IDocumentDbService dbService, IDialogService dialogService) : base(messenger)
        {
            EditorViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<JsonViewerViewModel>();
            EditorViewModel.IsReadOnly = true;

            HeaderViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<HeaderEditorViewModel>();
            HeaderViewModel.IsReadOnly = true;

            _dbService = dbService;
            _dialogService = dialogService;

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge, IsVisible = IsRunning }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
            _queryInformationStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = QueryInformation, IsVisible = IsRunning }, StatusBarItemType.SimpleText, "Information", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_queryInformationStatusBarItem);
            _progessBarStatusBarItem = new StatusBarItem(new StatusBarItemContextCancellableCommand { Value = CancelCommand, IsVisible = IsRunning, IsCancellable = true }, StatusBarItemType.ProgessBar, "Progress", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_progessBarStatusBarItem);
        }

        public override void Load(string contentId, CollectionNodeViewModel node, Connection connection, DocumentCollection collection)
        {
            ContentId = contentId;
            Node = node;
            Header = "SQL Query";
            Connection = connection;
            Collection = collection;

            Content = new TextDocument($"SELECT * FROM {Collection.Id} AS {Collection.Id.Substring(0, 1).ToLower()}");

            var split = Collection.AltLink.Split(new char[] { '/' });
            ToolTip = $"{split[1]}>{split[3]}";
            AccentColor = Node.Parent.Parent.Connection.AccentColor;
        }

        public CollectionNodeViewModel Node { get; protected set; }

        protected Connection Connection { get; set; }

        protected DocumentCollection Collection { get; set; }

        public TextDocument Content { get; set; }

        public string SelectedText { get; set; }

        public bool IsDirty { get; set; }

        public bool IsRunning { get; set; }

        public void OnIsRunningChanged()
        {
            _progessBarStatusBarItem.DataContext.IsVisible = IsRunning;
            _requestChargeStatusBarItem.DataContext.IsVisible = !IsRunning;
            _queryInformationStatusBarItem.DataContext.IsVisible = !IsRunning;

            if (IsRunning)
            {
                _cancellationToken = new CancellationTokenSource();
            }
            else
            {
                _cancellationToken = null;
            }
        }

        public JsonViewerViewModel EditorViewModel { get; set; }

        public HeaderEditorViewModel HeaderViewModel { get; set; }

        public string RequestCharge { get; set; }

        public void OnRequestChargeChanged()
        {
            _requestChargeStatusBarItem.DataContext.Value = RequestCharge;
        }

        public string QueryInformation { get; set; }

        public void OnQueryInformationChanged()
        {
            _queryInformationStatusBarItem.DataContext.Value = QueryInformation;
        }

        public string ContinuationToken { get; set; }

        public Dictionary<string, string> QueryMetrics { get; set; }

        public RelayCommand ExecuteCommand
        {
            get
            {
                return _executeCommand
                    ?? (_executeCommand = new RelayCommand(
                        async () => await ExecuteQueryAsync(null).ConfigureAwait(false),
                        () => !IsRunning && !string.IsNullOrEmpty(Content.Text)));
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

        private async Task ExecuteQueryAsync(string token)
        {
            try
            {
                IsRunning = true;

                ((StatusBarItemContextCancellableCommand)_progessBarStatusBarItem.DataContext).IsCancellable = true;

                var query = string.IsNullOrEmpty(SelectedText) ? Content.Text : SelectedText;
                _queryResult = await _dbService.ExecuteQueryAsync(Connection, Collection, query, this, token, _cancellationToken.Token).ConfigureAwait(false);

                ((StatusBarItemContextCancellableCommand)_progessBarStatusBarItem.DataContext).IsCancellable = false;

                ContinuationToken = _queryResult.ResponseContinuation;

                RequestCharge = $"Request Charge: {_queryResult.RequestCharge:N2}";
                QueryInformation = $"Returned {_queryResult.Count:N0} document(s)." +
                                        (ContinuationToken != null
                                                ? " (more results available)"
                                                : string.Empty);

                if (_queryResult.ResponseHeaders.AllKeys.Contains("x-ms-documentdb-query-metrics"))
                {
                    QueryMetrics = _queryResult.ResponseHeaders.GetValues("x-ms-documentdb-query-metrics")
                                                             .First()
                                                             .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                             .Select(part => part.Split('='))
                                                             .ToDictionary(split => split[0], split => split[1]);
                }
                else
                {
                    QueryMetrics = new Dictionary<string, string>();
                }

                EditorViewModel.SetText(_queryResult, HideSystemProperties);
                HeaderViewModel.SetText(_queryResult.ResponseHeaders, HideSystemProperties);
            }
            catch (OperationCanceledException)
            {
                ContinuationToken = null;
                RequestCharge = null;
                QueryInformation = null;
                QueryMetrics = new Dictionary<string, string>();
                EditorViewModel.SetText(null, HideSystemProperties);
                HeaderViewModel.SetText(null, HideSystemProperties);
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

        public RelayCommand GoToNextPageCommand
        {
            get
            {
                return _goToNextPageCommand
                    ?? (_goToNextPageCommand = new RelayCommand(
                        async () => await ExecuteQueryAsync(ContinuationToken).ConfigureAwait(false),
                        () => ContinuationToken != null
                            && !IsRunning
                            && !string.IsNullOrEmpty(Content.Text)));
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
                                    await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
                                }
                                finally
                                {
                                    IsRunning = false;
                                }
                            }
                        }).ConfigureAwait(false);
                    },
                    () => !IsRunning && !string.IsNullOrEmpty(EditorViewModel.Content?.Text)));
            }
        }

        public bool HideSystemProperties { get; set; } = true;

        public void OnHideSystemPropertiesChanged()
        {
            if (_queryResult != null)
            {
                EditorViewModel.SetText(_queryResult, HideSystemProperties);
            }
        }

        public bool? EnableScanInQuery { get; set; } = false;
        public bool? EnableCrossPartitionQuery { get; set; } = false;
        public int? MaxItemCount { get; set; } = 100;
        public int? MaxDOP { get; set; } = -1;
        public int? MaxBufferItem { get; set; } = -1;
    }
}
