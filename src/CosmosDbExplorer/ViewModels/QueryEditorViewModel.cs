using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using CosmosDbExplorer.Models;
using CosmosDbExplorer.Properties;
using CosmosDbExplorer.Services.DialogSettings;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using Validar;

namespace CosmosDbExplorer.ViewModels
{
    [InjectValidation]
    public class QueryEditorViewModel : PaneWithZoomViewModel<ContainerNodeViewModel>
        , IHaveSystemProperties
    {
        private CosmosQueryResult<IReadOnlyCollection<JObject>>? _queryResult;
        private readonly StatusBarItem _requestChargeStatusBarItem;
        private readonly StatusBarItem _queryInformationStatusBarItem;
        private readonly StatusBarItem _progessBarStatusBarItem;
        private CancellationTokenSource? _cancellationTokenSource;

        private RelayCommand _saveLocalCommand;
        private RelayCommand<string> _saveQueryCommand;
        private RelayCommand _openQueryCommand;

        private ICosmosDocumentService _documentService;

        private readonly ICosmosQuery _query;
        private AsyncRelayCommand _goToNextPageCommand;
        private AsyncRelayCommand _executeCommand;
        private RelayCommand _cancelCommand;

        public QueryEditorViewModel(IServiceProvider serviceProvider, IUIServices uiServices, IDialogService dialogService)
            : base(uiServices)
        {
            _serviceProvider = serviceProvider;
            _dialogService = dialogService;
            EditorViewModel = new JsonViewerViewModel { IsReadOnly = true };
            HeaderViewModel = new HeaderEditorViewModel { IsReadOnly = true };
            IconSource = App.Current.FindResource("SqlQueryIcon");

            _query = new CosmosQuery();

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge, IsVisible = IsRunning }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
            _queryInformationStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = QueryInformation, IsVisible = IsRunning }, StatusBarItemType.SimpleText, "Information", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_queryInformationStatusBarItem);
            _progessBarStatusBarItem = new StatusBarItem(new StatusBarItemContextCancellableCommand { Value = CancelCommand, IsVisible = IsRunning, IsCancellable = true }, StatusBarItemType.ProgessBar, "Progress", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_progessBarStatusBarItem);
        }

        public override void Load(string contentId, ContainerNodeViewModel node, CosmosConnection connection, CosmosDatabase database, CosmosContainer container)
        {
            ContentId = Guid.NewGuid().ToString();
            Node = node;
            Header = "SQL Query";
            Connection = connection;
            Container = container;

            _documentService = ActivatorUtilities.CreateInstance<CosmosDocumentService>(_serviceProvider, connection, database, container);

            Content = $"SELECT * FROM {Container.Id} AS {Container.Id.Substring(0, 1).ToLower()}";

            //var split = Container.SelfLink.Split(new char[] { '/' });
            ToolTip = $"{Connection.Label}/{database.Id}/{Container.Id}";
            AccentColor = Node.Parent.Parent.Connection.AccentColor;
        }

        public ContainerNodeViewModel Node { get; protected set; }

        protected CosmosConnection Connection { get; set; }

        protected CosmosContainer Container { get; set; }

        [OnChangedMethod(nameof(NotifyCanExecuteChanged))]
        public string Content { get; set; }

        [OnChangedMethod(nameof(NotifyCanExecuteChanged))]
        public string SelectedText { get; set; }

        [OnChangedMethod(nameof(NotifyCanExecuteChanged))]
        public bool IsDirty { get; set; }

        public bool IsRunning { get; set; }

        public void OnIsRunningChanged()
        {
            _progessBarStatusBarItem.DataContext.IsVisible = IsRunning;
            _requestChargeStatusBarItem.DataContext.IsVisible = !IsRunning;
            _queryInformationStatusBarItem.DataContext.IsVisible = !IsRunning;

            if (IsRunning)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }
            else
            {
                _cancellationTokenSource = null;
            }

            NotifyCanExecuteChanged();
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;

        public JsonViewerViewModel EditorViewModel { get; set; }

        public HeaderEditorViewModel HeaderViewModel { get; set; }

        public string? RequestCharge { get; set; }

        public void OnRequestChargeChanged()
        {
            _requestChargeStatusBarItem.DataContext.Value = RequestCharge;
        }

        public string? QueryInformation { get; set; }

        public void OnQueryInformationChanged()
        {
            _queryInformationStatusBarItem.DataContext.Value = QueryInformation;
        }

        public string? ContinuationToken { get; set; }

        public string? QueryMetrics { get; set; }

        //public Dictionary<string, QueryMetrics> QueryMetrics { get; set; }

        public AsyncRelayCommand ExecuteCommand => _executeCommand ??= new(() => ExecuteQueryAsync(null), () => !IsRunning && !string.IsNullOrEmpty(Content) && IsValid);

        public RelayCommand CancelCommand => _cancelCommand ??= new(() => _cancellationTokenSource?.Cancel(), () => IsRunning);

        private async Task ExecuteQueryAsync(string? token)
        {
            try
            {
                IsRunning = true;

                Clean();

                ((StatusBarItemContextCancellableCommand)_progessBarStatusBarItem.DataContext).IsCancellable = true;

                _query.QueryText= string.IsNullOrEmpty(SelectedText) ? Content : SelectedText;
                _query.PartitionKeyValue = GetPartitionKey();
                _query.ContinuationToken = token;

                _queryResult = await _documentService.ExecuteQueryAsync(_query, _cancellationTokenSource.Token);

                ((StatusBarItemContextCancellableCommand)_progessBarStatusBarItem.DataContext).IsCancellable = false;

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

                NotifyCanExecuteChanged();

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
            catch (Exception ex)
            {
                Clean();
                await _dialogService.ShowError(ex, "Error").ConfigureAwait(false);
            }
            finally
            {
                IsRunning = false;
            }
        }

        private Optional<object?> GetPartitionKey()
        { 
            if (string.IsNullOrEmpty(PartitionKeyValue))
            {
                return new Optional<object?>();
            }

            if (double.TryParse(PartitionKeyValue, out var numeric))
            {
                return new Optional<object?>(numeric);
            }

            if (bool.TryParse(PartitionKeyValue, out var boolean))
            {
                return new Optional<object?>(boolean);
            }

            if (PartitionKeyValue.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return new Optional<object?>(null);
            }

            return new Optional<object?>(PartitionKeyValue);
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

        private void NotifyCanExecuteChanged()
        {
            GoToNextPageCommand.NotifyCanExecuteChanged();
            SaveLocalCommand.NotifyCanExecuteChanged();
            SaveQueryCommand.NotifyCanExecuteChanged();
            OpenQueryCommand.NotifyCanExecuteChanged();
        }

        public AsyncRelayCommand GoToNextPageCommand => _goToNextPageCommand ??= new(GoToNextPageCommandExecute, () => GoToNextPageCommandCanExecute());

        private Task GoToNextPageCommandExecute()
        {
            return ExecuteQueryAsync(ContinuationToken);
        }

        private bool GoToNextPageCommandCanExecute()
        {
            return !string.IsNullOrEmpty(ContinuationToken) && !IsRunning && !string.IsNullOrEmpty(Content) && IsValid;
        }


        public RelayCommand SaveLocalCommand => _saveLocalCommand ??= new(SaveLocalCommandExecute, () => !IsRunning && !string.IsNullOrEmpty(EditorViewModel.Text));
        
        private void SaveLocalCommandExecute()
        {
            var settings = new SaveFileDialogSettings
            {
                DefaultExt = "json",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                AddExtension = true,
                OverwritePrompt = true,
                CheckFileExists = false,
                Title = "Save document locally",
                InitialDirectory = Settings.Default.GetExportFolder()
            };

            async void saveFileAsyc(bool confirm, FileDialogResult result) 
            {
                if (!confirm)
                {
                    return;
                }

                IsRunning = true;

                try
                {
                    Settings.Default.ExportFolder = Path.GetDirectoryName(result.FileName);
                    Settings.Default.Save();

                    await File.WriteAllTextAsync(result.FileName, EditorViewModel.Text);
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowError(ex, "An error during saving file");
                }
                finally
                {
                    IsRunning = false;
                }
            };

            _dialogService?.ShowSaveFileDialog(settings, saveFileAsyc);
        }

        public string FileName { get; set; }

        public RelayCommand<string> SaveQueryCommand => _saveQueryCommand ??= new(SaveQueryCommandExecute, param => !IsRunning && !string.IsNullOrEmpty(Content));

        private async void SaveQueryCommandExecute(string param)
        {
            if (FileName == null || param == "SaveAs")
            {
                var settings = new SaveFileDialogSettings
                {
                    DefaultExt = "sql",
                    Filter = "SQL Query (*.sql)|*.sql|All files (*.*)|*.*",
                    AddExtension = true,
                    OverwritePrompt = true,
                    CheckFileExists = false,
                    Title = "Save Query"
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
                        FileName = result.FileName;

                        await File.WriteAllTextAsync(result.FileName, Content);
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
            else
            {
                await File.WriteAllTextAsync(FileName, Content);
            }
        }

        public RelayCommand OpenQueryCommand => _openQueryCommand ??= new(OpenQueryCommandExecute, () => !IsRunning && !string.IsNullOrEmpty(Content));
        
        private void OpenQueryCommandExecute()
        {
            var settings = new OpenFileDialogSettings
            {
                DefaultExt = "sql",
                Filter = "SQL Query (*.sql)|*.sql|All files (*.*)|*.*",
                AddExtension = true,
                CheckFileExists = true,
                Title = "Open Query"
            };

            async void openFile(bool confirm, FileDialogResult result)
            {
                if (!confirm)
                {
                    return;
                }

                IsRunning = true;

                try
                {
                    Content = await File.ReadAllTextAsync(result.FileName);
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowError(ex, "An error occured");
                }
                finally
                {
                    IsRunning = false;
                }
            }

            _dialogService.ShowOpenFileDialog(settings, openFile);
        }

        public bool HideSystemProperties { get; set; }

        public void OnHideSystemPropertiesChanged()
        {
            if (_queryResult != null)
            {
                EditorViewModel.SetText(_queryResult.Items, HideSystemProperties);
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
        
        public string? PartitionKeyValue { get; set; }

        public bool IsValid => string.IsNullOrEmpty(((IDataErrorInfo)this).Error);//!((INotifyDataErrorInfo)this).HasErrors;

        protected override void OnClose()
        {
            Clean();
            base.OnClose();
        }
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
