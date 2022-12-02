using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.Services.DialogSettings;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

using Validar;

namespace CosmosDbExplorer.ViewModels.Assets
{
    [InjectValidation]
    public class StoredProcedureTabViewModel : AssetTabViewModelBase<StoredProcedureNodeViewModel, CosmosStoredProcedure>
    {
        private AsyncRelayCommand? _executeCommand;
        private RelayCommand<StoredProcParameterViewModel>? _removeParameterCommand;
        private RelayCommand? _addParameterCommand;
        private RelayCommand<object>? _browseParameterCommand;
        private RelayCommand? _saveLocalCommand;
        private RelayCommand? _goToNextPageCommand;
        private readonly StatusBarItem _requestChargeStatusBarItem;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;
        private readonly CosmosScriptService _scriptService;

        public StoredProcedureTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices, IDialogService dialogService, string contentId, NodeContext<StoredProcedureNodeViewModel> nodeContext)
            : base(uiServices, dialogService, contentId, nodeContext)
        {
            HeaderViewModel = new HeaderEditorViewModel { IsReadOnly = true };
            IconSource = App.Current.FindResource("StoredProcedureIcon");

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge, IsVisible = IsBusy }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
            _serviceProvider = serviceProvider;
            _dialogService = dialogService;

            if (nodeContext.Connection is null || nodeContext.Database is null || nodeContext.Container is null)
            {
                throw new NullReferenceException("Node context is not correctly initialized!");
            }

            _scriptService = ActivatorUtilities.CreateInstance<CosmosScriptService>(_serviceProvider, nodeContext.Connection, nodeContext.Database, nodeContext.Container);
        }

        protected override string GetDefaultHeader() => "New Stored Procedure";
        protected override string GetDefaultTitle() => "Stored Procedure";
        protected override string GetDefaultContent() => "function storedProcedure(){}";

        public override Task InitializeAsync()
        {
            IsCollectionPartitioned = !string.IsNullOrEmpty(Container.PartitionKeyPath);  // collection.PartitionKey.Paths.Count > 0;

            UpdateCommandStatus();

            return Task.CompletedTask;
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            return _scriptService.DeleteStoredProcedureAsync(Node.Resource);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public string Log { get; protected set; } = string.Empty;

        public string? QueryResult { get; set; }

        public HeaderEditorViewModel HeaderViewModel { get; set; }

        public string RequestCharge { get; set; } = string.Empty;

        public void OnRequestChargeChanged()
        {
            _requestChargeStatusBarItem.DataContext.Value = RequestCharge;
        }

        protected override void OnIsBusyChanged()
        {
            _requestChargeStatusBarItem.DataContext.IsVisible = !IsBusy;

            base.OnIsBusyChanged();
        }
        public string? PartitionKey { get; set; }

        public bool IsCollectionPartitioned { get; protected set; }

        public ObservableCollection<StoredProcParameterViewModel> Parameters { get; } = new ObservableCollection<StoredProcParameterViewModel>();

        public RelayCommand AddParameterCommand => _addParameterCommand ??= new(() => Parameters.Add(new StoredProcParameterViewModel())/*, () => !IsBusy && !IsDirty*/);

        public RelayCommand<StoredProcParameterViewModel> RemoveParameterCommand => _removeParameterCommand ??= new(RemoveParameterCommandExecute/*, RemoveParameterCommandCanExecute*/);

        private void RemoveParameterCommandExecute(StoredProcParameterViewModel? item)
        {
            if (item == null)
            {
                return;
            }

            Parameters.Remove(item);
            item.Dispose();
        }

        //private bool RemoveParameterCommandCanExecute(StoredProcParameterViewModel? item) => !IsBusy & !IsDirty;

        public RelayCommand<object> BrowseParameterCommand => _browseParameterCommand ??= new(BrowseParameterCommandExecute/*, BrowseParameterCommandCanExecute*/);

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

        //private bool BrowseParameterCommandCanExecute(object? item) => !IsBusy & !IsDirty;


        public AsyncRelayCommand ExecuteCommand => _executeCommand ??= new AsyncRelayCommand(ExecuteCommandExecute, ExecuteCommandCanExecute);

        private async Task ExecuteCommandExecute()
        {
            if (Id is null)
            {
                return;
            }

            QueryResult = null;
            Log = string.Empty;
            HeaderViewModel.SetText(null, false);

            try
            {
                IsBusy = true;

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                var result = await _scriptService.ExecuteStoredProcedureAsync(Id, PartitionKey, Parameters.Select(p => p.GetValue()).ToArray());
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                RequestCharge = $"Request Charge: {result.RequestCharge:N2}";

                Log = result.ScriptLog;
                QueryResult = result.Result;
                HeaderViewModel.SetText(result.Headers, false);
            }
            //catch (DocumentClientException clientEx)
            //{
            //    await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null).ConfigureAwait(false);
            //}
            catch (Exception ex)
            {
                await _dialogService.ShowError(ex, "Error");
            }
            finally
            {
                IsBusy = false;
                UpdateCommandStatus();
            }
        }

        protected override void UpdateCommandStatus()
        {
            base.UpdateCommandStatus();
            ExecuteCommand.NotifyCanExecuteChanged();
            SaveLocalCommand.NotifyCanExecuteChanged();
            AddParameterCommand.NotifyCanExecuteChanged();
        }

        private bool ExecuteCommandCanExecute() => !IsBusy && !IsDirty && IsValid;

        public RelayCommand SaveLocalCommand => _saveLocalCommand ??= new(SaveLocalCommandExecute/*, SaveLocalCommandCanExecute*/);

        private void SaveLocalCommandExecute()
        {
            var settings = new SaveFileDialogSettings
            {
                //DefaultExt = "json",
                //Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                //AddExtension = true,
                OverwritePrompt = true,
                CheckFileExists = false,
                Title = "Save document locally"
            };

            _dialogService.ShowSaveFileDialog(settings, async (confirm, result) =>
            {
                if (confirm)
                {
                    try
                    {
                        IsBusy = true;
                        System.IO.File.WriteAllText(result.FileName, QueryResult);
                    }
                    catch (Exception ex)
                    {
                        await _dialogService.ShowError(ex, "Error");
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }
            });
        }

        //private bool SaveLocalCommandCanExecute() => !IsBusy && QueryResult is not null;

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
