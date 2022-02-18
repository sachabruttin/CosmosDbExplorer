using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using FluentValidation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Validar;

namespace CosmosDbExplorer.ViewModels.Assets
{
    [InjectValidation]
    public class StoredProcedureTabViewModel : AssetTabViewModelBase<StoredProcedureNodeViewModel, CosmosStoredProcedure>
    {
        private RelayCommand _executeCommand;
        private RelayCommand<StoredProcParameterViewModel> _removeParameterCommand;
        private RelayCommand _addParameterCommand;
        private RelayCommand<object> _browseParameterCommand;
        private RelayCommand _saveLocalCommand;
        private RelayCommand _goToNextPageCommand;
        private readonly StatusBarItem _requestChargeStatusBarItem;
        private readonly IServiceProvider _serviceProvider;

        public StoredProcedureTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices, IDialogService dialogService)
            : base(uiServices, dialogService)
        {
            ResultViewModel = new JsonViewerViewModel { IsReadOnly = true };
            HeaderViewModel = new HeaderEditorViewModel { IsReadOnly = true };
            IconSource = App.Current.FindResource("StoredProcedureIcon");

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge, IsVisible = IsBusy }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
            _serviceProvider = serviceProvider;
        }


        //public StoredProcedureTabViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService, IUIServices uiServices)
        //    : base(messenger, dialogService, dbService, uiServices)
        //{
        //    _dialogService = dialogService;
        //    _dbService = dbService;

        //    ResultViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<JsonViewerViewModel>();
        //    ResultViewModel.IsReadOnly = true;

        //    HeaderViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<HeaderEditorViewModel>();
        //    HeaderViewModel.IsReadOnly = true;

        //    _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge, IsVisible = IsBusy }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
        //    StatusBarItems.Add(_requestChargeStatusBarItem);
        //}

        protected override string GetDefaultHeader() => "New Stored Procedure"; 
        protected override string GetDefaultTitle() => "Stored Procedure"; 
        protected override string GetDefaultContent() => "function storedProcedure(){}";

        public override void Load(string contentId, StoredProcedureNodeViewModel node, CosmosConnection connection, CosmosDatabase database, CosmosContainer container)
        {
            _scriptService = ActivatorUtilities.CreateInstance<CosmosScriptService>(_serviceProvider, connection, database, container);

            IsCollectionPartitioned = !string.IsNullOrEmpty(container.PartitionKeyPath);  // collection.PartitionKey.Paths.Count > 0;
            base.Load(contentId, node, connection, database, container);
        }

        protected override Task<CosmosStoredProcedure> SaveAsyncImpl()
        {
            if (Id is null)
            {
                throw new Exception("Asset Id is null!");
            }

            var resource = new CosmosStoredProcedure(Id, Content, AltLink);
            return _scriptService.SaveStoredProcedureAsync(resource);
        }

        protected override Task<CosmosResult> DeleteAsyncImpl()
        {
            return _scriptService.DeleteStoredProcedureAsync(Node.Resource);
        }

        public string Log { get; protected set; }

        public JsonViewerViewModel ResultViewModel { get; set; }

        public HeaderEditorViewModel HeaderViewModel { get; set; }

        public string RequestCharge { get; set; }

        public void OnRequestChargeChanged()
        {
            _requestChargeStatusBarItem.DataContext.Value = RequestCharge;
        }

        protected override void OnIsBusyChanged()
        {
            _requestChargeStatusBarItem.DataContext.IsVisible = !IsBusy;

            base.OnIsBusyChanged();
        }
        public string PartitionKey { get; set; }

        private CosmosScriptService _scriptService;

        public bool IsCollectionPartitioned { get; protected set; }

        public ObservableCollection<StoredProcParameterViewModel> Parameters { get; } = new ObservableCollection<StoredProcParameterViewModel>();

        public RelayCommand AddParameterCommand => _addParameterCommand ??= new(() => Parameters.Add(new StoredProcParameterViewModel()), () => !IsBusy && !IsDirty);

        public RelayCommand<StoredProcParameterViewModel> RemoveParameterCommand => _removeParameterCommand ??= new(RemoveParameterCommandExecute, RemoveParameterCommandCanExecute);

        private void RemoveParameterCommandExecute(StoredProcParameterViewModel? item)
        {
            if (item == null)
            {
                return;
            }

            Parameters.Remove(item);
            item.Dispose();
        }

        private bool RemoveParameterCommandCanExecute(StoredProcParameterViewModel? item) => !IsBusy & !IsDirty;

        public RelayCommand<object> BrowseParameterCommand => _browseParameterCommand ??= new(BrowseParameterCommandExecute, BrowseParameterCommandCanExecute);

        private void BrowseParameterCommandExecute(object? item)
        {
            throw new NotImplementedException();
            //var options = new OpenFileDialogSettings
            //{
            //    Title = "Select file...",
            //    DefaultExt = "json",
            //    Multiselect = false,
            //    Filter = "JSON|*.json"
            //};

            //await _dialogService.ShowOpenFileDialog(options, (confirm, result) =>
            //{
            //    if (confirm && item is StoredProcParameterViewModel vm)
            //    {
            //        vm.FileName = result.FileName;
            //    }
            //}).ConfigureAwait(false);
        }

        private bool BrowseParameterCommandCanExecute(object? item) => !IsBusy & !IsDirty;


        public RelayCommand ExecuteCommand => _executeCommand ??= new(ExecuteCommandExecute, ExecuteCommandCanExecute);

        private void ExecuteCommandExecute()
        {
            throw new NotImplementedException();
            //try
            //{
            //    IsBusy = true;
            //    var result = await _dbService.ExecuteStoreProcedureAsync(Connection, AltLink, Parameters.Select(p => p.GetValue()).ToArray(), PartitionKey).ConfigureAwait(false);
            //    RequestCharge = $"Request Charge: {result.RequestCharge:N2}";

            //    Log += $"{DateTime.Now:T} : {WebUtility.UrlDecode(result.ScriptLog)}{Environment.NewLine}";

            //    ResultViewModel.SetText(result.Response, false);
            //    HeaderViewModel.SetText(result.ResponseHeaders, false);
            //}
            //catch (DocumentClientException clientEx)
            //{
            //    await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null).ConfigureAwait(false);
            //}
            //catch (Exception ex)
            //{
            //    await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
            //}
            //finally
            //{
            //    IsBusy = false;
            //}
        }

        private bool ExecuteCommandCanExecute() => !IsBusy && !IsDirty && IsValid;

        public RelayCommand SaveLocalCommand => _saveLocalCommand ??= new(SaveLocalCommandExecute, SaveLocalCommandCanExecute);

        private void SaveLocalCommandExecute()
        {
            throw new NotImplementedException();
            //var settings = new SaveFileDialogSettings
            //{
            //    DefaultExt = "json",
            //    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            //    AddExtension = true,
            //    OverwritePrompt = true,
            //    CheckFileExists = false,
            //    Title = "Save document locally"
            //};

            //await _dialogService.ShowSaveFileDialog(settings, async (confirm, result) =>
            //{
            //    if (confirm)
            //    {
            //        try
            //        {
            //            IsBusy = true;

            //            await DispatcherHelper.RunAsync(() => File.WriteAllText(result.FileName, ResultViewModel.Content.Text));
            //        }
            //        catch (Exception ex)
            //        {
            //            await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
            //        }
            //        finally
            //        {
            //            IsBusy = false;
            //        }
            //    }
            //}).ConfigureAwait(false);
        }

        private bool SaveLocalCommandCanExecute() => !IsBusy && !string.IsNullOrEmpty(ResultViewModel.Text);

        public RelayCommand GoToNextPageCommand => _goToNextPageCommand ??= new(() => throw new NotImplementedException(), () => false);

        public bool IsValid => string.IsNullOrEmpty(((IDataErrorInfo)this).Error); //!((INotifyDataErrorInfo)this).HasErrors;
    }

    public class StoredProcedureTabViewModelValidator : AbstractValidator<StoredProcedureTabViewModel>
    {
        public StoredProcedureTabViewModelValidator()
        {
            //When(x => x.IsCollectionPartitioned, 
            //    () => RuleFor(x => x.PartitionKey).NotEmpty().SetValidator(new PartitionKeyValidator()));
        }
    }
}
