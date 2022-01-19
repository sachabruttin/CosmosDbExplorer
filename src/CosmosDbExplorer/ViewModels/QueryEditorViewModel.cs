using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Contracts;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using Validar;

namespace CosmosDbExplorer.ViewModels
{
    [InjectValidation]
    public class QueryEditorViewModel : PaneWithZoomViewModel<ContainerNodeViewModel>
        , IHaveSystemProperties
    {
        private RelayCommand _saveLocalCommand;
        private CosmosQueryResult<IReadOnlyCollection<JObject>>? _queryResult;
        //private RelayCommand _goToNextPageCommand;
        //private readonly StatusBarItem _requestChargeStatusBarItem;
        //private readonly StatusBarItem _queryInformationStatusBarItem;
        //private readonly StatusBarItem _progessBarStatusBarItem;
        private CancellationTokenSource _cancellationTokenSource;
        //private RelayCommand _cancelCommand;
        private RelayCommand<string> _saveQueryCommand;
        private RelayCommand _openQueryCommand;

        private ICosmosDocumentService _documentService;

        private ICosmosQuery _query;

        public QueryEditorViewModel(IServiceProvider serviceProvider, IUIServices uiServices)
            : base(uiServices)
        {
            _serviceProvider = serviceProvider;

            EditorViewModel = new JsonViewerViewModel { IsReadOnly = true };
            HeaderViewModel = new HeaderEditorViewModel { IsReadOnly = true };
            IconSource = App.Current.FindResource("SqlQueryIcon");

            _cancellationTokenSource = new CancellationTokenSource();
            _query = new CosmosQuery();
        }


        //public QueryEditorViewModel(IMessenger messenger, IDocumentDbService dbService, IDialogService dialogService, IUIServices uiServices)
        //    : base(messenger, uiServices)
        //{
        //    EditorViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<JsonViewerViewModel>();
        //    EditorViewModel.IsReadOnly = true;

        //    HeaderViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<HeaderEditorViewModel>();
        //    HeaderViewModel.IsReadOnly = true;

        //    _dbService = dbService;
        //    _dialogService = dialogService;

        //    _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge, IsVisible = IsRunning }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
        //    StatusBarItems.Add(_requestChargeStatusBarItem);
        //    _queryInformationStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = QueryInformation, IsVisible = IsRunning }, StatusBarItemType.SimpleText, "Information", System.Windows.Controls.Dock.Left);
        //    StatusBarItems.Add(_queryInformationStatusBarItem);
        //    _progessBarStatusBarItem = new StatusBarItem(new StatusBarItemContextCancellableCommand { Value = CancelCommand, IsVisible = IsRunning, IsCancellable = true }, StatusBarItemType.ProgessBar, "Progress", System.Windows.Controls.Dock.Left);
        //    StatusBarItems.Add(_progessBarStatusBarItem);
        //}

        public override void Load(string contentId, ContainerNodeViewModel node, CosmosConnection connection, CosmosContainer container)
        {
            ContentId = contentId;
            Node = node;
            Header = "SQL Query";
            Connection = connection;
            Container = container;

            _documentService = ActivatorUtilities.CreateInstance<CosmosDocumentService>(_serviceProvider, connection, node.Parent.Database, container);

            Content = $"SELECT * FROM {Container.Id} AS {Container.Id.Substring(0, 1).ToLower()}";

            var split = Container.SelfLink.Split(new char[] { '/' });
            ToolTip = $"{split[1]}>{split[3]}";
            AccentColor = Node.Parent.Parent.Connection.AccentColor;
        }

        public ContainerNodeViewModel Node { get; protected set; }

        protected CosmosConnection Connection { get; set; }

        protected CosmosContainer Container { get; set; }

        public string Content { get; set; }

        public string SelectedText { get; set; }

        public bool IsDirty { get; set; }

        public bool IsRunning { get; set; }

        public void OnIsRunningChanged()
        {
            //_progessBarStatusBarItem.DataContext.IsVisible = IsRunning;
            //_requestChargeStatusBarItem.DataContext.IsVisible = !IsRunning;
            //_queryInformationStatusBarItem.DataContext.IsVisible = !IsRunning;

            //if (IsRunning)
            //{
            //    _cancellationTokenSource = new CancellationTokenSource();
            //}
            //else
            //{
            //    _cancellationTokenSource = null;
            //}
        }

        private readonly IServiceProvider _serviceProvider;

        public JsonViewerViewModel EditorViewModel { get; set; }

        public HeaderEditorViewModel HeaderViewModel { get; set; }

        public string? RequestCharge { get; set; }

        public void OnRequestChargeChanged()
        {
            //_requestChargeStatusBarItem.DataContext.Value = RequestCharge;
        }

        public string? QueryInformation { get; set; }

        public void OnQueryInformationChanged()
        {
            //_queryInformationStatusBarItem.DataContext.Value = QueryInformation;
        }

        public string? ContinuationToken { get; set; }

        public string? QueryMetrics { get; set; }

        //public Dictionary<string, QueryMetrics> QueryMetrics { get; set; }

        public RelayCommand ExecuteCommand => new(async () => await ExecuteQueryAsync(null).ConfigureAwait(false),
                                                         () => !IsRunning && !string.IsNullOrEmpty(Content) && IsValid);

        public RelayCommand CancelCommand => new(() => _cancellationTokenSource?.Cancel(), () => IsRunning);

        private async Task ExecuteQueryAsync(string? token)
        {
            try
            {
                IsRunning = true;

                Clean();

                //((StatusBarItemContextCancellableCommand)_progessBarStatusBarItem.DataContext).IsCancellable = true;

                _query.QueryText= string.IsNullOrEmpty(SelectedText) ? Content : SelectedText;
                _query.ContinuationToken = token;

                _queryResult = await _documentService.ExecuteQueryAsync(_query, _cancellationTokenSource.Token);

                //((StatusBarItemContextCancellableCommand)_progessBarStatusBarItem.DataContext).IsCancellable = false;

                ContinuationToken = _queryResult.ContinuationToken;

                RequestCharge = $"Request Charge: {_queryResult.RequestCharge:N2}";
                QueryInformation = $"Returned {_queryResult.Items?.Count:N0} document(s)." +
                                        (ContinuationToken != null
                                                ? " (more results available)"
                                                : string.Empty);

                QueryMetrics = _queryResult.IndexMetrics;

                //if (_queryResult.QueryMetrics != null)
                //{
                //    QueryMetrics = new Dictionary<string, QueryMetrics>
                //    {
                //        { "All", _queryResult.QueryMetrics.Select(q => q.Value).Aggregate((i, next) => i + next) }
                //    };

                //    foreach (var metric in _queryResult.QueryMetrics.OrderBy(q => int.Parse(q.Key)))
                //    {
                //        QueryMetrics.Add(metric.Key, metric.Value);
                //    }
                //}
                //else
                //{
                //    QueryMetrics = null;
                //}


                EditorViewModel.SetText(_queryResult.Items, HideSystemProperties);
                HeaderViewModel.SetText(_queryResult.Headers, HideSystemProperties);
            }
            catch (OperationCanceledException)
            {
                Clean();
            }
            //catch (DocumentClientException clientEx)
            //{
            //    Clean();
            //    await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null).ConfigureAwait(false);
            //}
            //catch (Exception ex)
            //{
            //    Clean();
            //    await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
            //}
            finally
            {
                IsRunning = false;
            }
        }

        private void Clean()
        {
            ContinuationToken = null;
            RequestCharge = null;
            QueryInformation = null;
            QueryMetrics = null;
            EditorViewModel.SetText(null, HideSystemProperties);
            HeaderViewModel.SetText(null, HideSystemProperties);

            GC.Collect();
        }

        public RelayCommand GoToNextPageCommand => new (async () => await GoToNextPageCommandExecute(), () => GoToNextPageCommandCanExecute());

        private Task GoToNextPageCommandExecute()
        {
            return ExecuteQueryAsync(ContinuationToken);
        }

        private bool GoToNextPageCommandCanExecute()
        {
            return !string.IsNullOrEmpty(ContinuationToken) && !IsRunning && !string.IsNullOrEmpty(Content) && IsValid;
        }


        public RelayCommand SaveLocalCommand
        {
            get
            {
                return _saveLocalCommand ??
                (_saveLocalCommand = new RelayCommand(
                    async () =>
                    {
                        //var settings = new SaveFileDialogSettings
                        //{
                        //    DefaultExt = "json",
                        //    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                        //    AddExtension = true,
                        //    OverwritePrompt = true,
                        //    CheckFileExists = false,
                        //    Title = "Save document locally",
                        //    InitialDirectory = Settings.Default.GetExportFolder()
                        //};

                        //await _dialogService.ShowSaveFileDialog(settings, async (confirm, result) =>
                        //{
                        //    if (confirm)
                        //    {
                        //        try
                        //        {
                        //            IsRunning = true;

                        //            Settings.Default.ExportFolder = (new FileInfo(result.FileName)).DirectoryName;
                        //            Settings.Default.Save();

                        //            await DispatcherHelper.RunAsync(() => File.WriteAllText(result.FileName, EditorViewModel.Content.Text));
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
                        //        }
                        //        finally
                        //        {
                        //            IsRunning = false;
                        //        }
                        //    }
                        //}).ConfigureAwait(false);
                    },
                    () => !IsRunning && !string.IsNullOrEmpty(EditorViewModel.Text)));
            }
        }

        public string FileName { get; set; }

        public RelayCommand<string> SaveQueryCommand
        {
            get
            {
                return _saveQueryCommand ??
                    (_saveQueryCommand = new RelayCommand<string>(
                     async param =>
                     {
                         //if (FileName == null || param == "SaveAs")
                         //{
                         //    var settings = new SaveFileDialogSettings
                         //    {
                         //        DefaultExt = "sql",
                         //        Filter = "SQL Query (*.sql)|*.sql|All files (*.*)|*.*",
                         //        AddExtension = true,
                         //        OverwritePrompt = true,
                         //        CheckFileExists = false,
                         //        Title = "Save Query"
                         //    };

                         //    await _dialogService.ShowSaveFileDialog(settings, async (confirm, result) =>
                         //    {
                         //        if (confirm)
                         //        {
                         //            try
                         //            {
                         //                IsRunning = true;
                         //                FileName = result.FileName;

                         //                await DispatcherHelper.RunAsync(() => File.WriteAllText(result.FileName, Content.Text));
                         //            }
                         //            catch (Exception ex)
                         //            {
                         //                await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
                         //            }
                         //            finally
                         //            {
                         //                IsRunning = false;
                         //            }
                         //        }
                         //    }).ConfigureAwait(false);
                         //}
                         //else
                         //{
                         //    await DispatcherHelper.RunAsync(() => File.WriteAllText(FileName, Content.Text));
                         //}
                     },
                     param => !IsRunning && !string.IsNullOrEmpty(Content)));
            }
        }

        public RelayCommand OpenQueryCommand
        {
            get
            {
                return _openQueryCommand ??
                    (_openQueryCommand = new RelayCommand(
                     async () =>
                     {
                         //var settings = new OpenFileDialogSettings
                         //{
                         //    DefaultExt = "sql",
                         //    Filter = "SQL Query (*.sql)|*.sql|All files (*.*)|*.*",
                         //    AddExtension = true,
                         //    CheckFileExists = false,
                         //    Title = "Save Query"
                         //};

                         //await _dialogService.ShowOpenFileDialog(settings, async (confirm, result) =>
                         //{
                         //    if (confirm)
                         //    {
                         //        try
                         //        {
                         //            IsRunning = true;
                         //            FileName = result.FileName;
                         //            var txt = File.ReadAllText(result.FileName);
                         //            await DispatcherHelper.RunAsync(() => Content.Text = txt);
                         //        }
                         //        catch (Exception ex)
                         //        {
                         //            await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
                         //        }
                         //        finally
                         //        {
                         //            IsRunning = false;
                         //        }
                         //    }
                         //}).ConfigureAwait(false);
                     },
                     () => !IsRunning && !string.IsNullOrEmpty(Content)));
            }
        }

        public bool HideSystemProperties
        {
            get { return _query.HideSystemProperties; }
            set { _query.HideSystemProperties = value; }
        }

        public void OnHideSystemPropertiesChanged()
        {
            if (_queryResult != null)
            {
                EditorViewModel.SetText(_queryResult, HideSystemProperties);
            }
        }

        public bool EnableScanInQuery
        {
            get { return _query.EnableScanInQuery; }
            set { _query.EnableScanInQuery = value; }
        }
        public bool EnableCrossPartitionQuery
        {
            get { return _query.EnableCrossPartitionQuery; }
            set { _query.EnableCrossPartitionQuery = value; }
        }

        public int MaxItemCount
        {
            get { return _query.MaxItemCount; }
            set { _query.MaxItemCount = value; }
        }

        public int MaxDOP
        {
            get { return _query.MaxDOP; }
            set { _query.MaxDOP = value; }
        }

        public int MaxBufferItem
        {
            get { return _query.MaxBufferItem; }
            set { _query.MaxBufferItem = value; }
        }
        
        public string? PartitionKeyValue
        {
            get { return _query.PartitionKeyValue; }
            set { _query.PartitionKeyValue = value; }
        }

        public bool IsValid => !((INotifyDataErrorInfo)this).HasErrors;

        protected override void OnClose()
        {
            Clean();
            base.OnClose();
        }

        //public override void Cleanup()
        //{
        //    SimpleIoc.Default.Unregister(EditorViewModel);
        //    SimpleIoc.Default.Unregister(HeaderViewModel);

        //    base.Cleanup();
        //}

    }

    public class QueryEditorViewModelValidator : AbstractValidator<QueryEditorViewModel>
    {
        public QueryEditorViewModelValidator()
        {
            //When(x => !string.IsNullOrEmpty(x.PartitionKeyValue?.Trim()),
            //    () => RuleFor(x => x.PartitionKeyValue).SetValidator(new PartitionKeyValidator()));
        }
    }
}
