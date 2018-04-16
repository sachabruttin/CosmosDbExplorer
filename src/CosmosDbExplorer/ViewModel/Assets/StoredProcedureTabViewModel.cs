using System;
using System.Net;
using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Extensions;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Services;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModel.Assets
{
    public class StoredProcedureTabViewModel : AssetTabViewModelBase<StoredProcedureNodeViewModel, StoredProcedure>
    {
        private RelayCommand _executeCommand;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private readonly StatusBarItem _requestChargeStatusBarItem;

        public StoredProcedureTabViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService, IUIServices uiServices)
            : base(messenger, dialogService, dbService, uiServices)
        {
            _dialogService = dialogService;
            _dbService = dbService;

            ResultViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<JsonViewerViewModel>();
            ResultViewModel.IsReadOnly = true;

            HeaderViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<HeaderEditorViewModel>();
            HeaderViewModel.IsReadOnly = true;

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge, IsVisible = IsBusy }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
        }

        protected override string GetDefaultHeader() { return "New Stored Procedure"; }
        protected override string GetDefaultTitle() { return "Stored Procedure"; }
        protected override string GetDefaultContent() { return Constants.Default.StoredProcedure; }

        protected override void SetInformationImpl(StoredProcedure resource)
        {
            SetText(resource.Body);
        }

        protected override Task<StoredProcedure> SaveAsyncImpl(IDocumentDbService dbService)
        {
            return dbService.SaveStoredProcedureAsync(Connection, Collection, Id, Content.Text, AltLink);
        }

        protected override Task DeleteAsyncImpl(IDocumentDbService dbService)
        {
            return dbService.DeleteStoredProcedureAsync(Connection, AltLink);
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

        public RelayCommand ExecuteCommand
        {
            get
            {
                return _executeCommand ?? (_executeCommand = new RelayCommand(
                    async () =>
                    {
                        try
                        {
                            IsBusy = true;
                            var result = await _dbService.ExecuteStoreProcedureAsync(Connection, AltLink, null).ConfigureAwait(false);
                            RequestCharge = $"Request Charge: {result.RequestCharge:N2}";

                            Log += $"{DateTime.Now:T} : {WebUtility.UrlDecode(result.ScriptLog)}{Environment.NewLine}";

                            ResultViewModel.SetText(result.Response, false);
                            HeaderViewModel.SetText(result.ResponseHeaders, false);
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
                            IsBusy = false;
                        }
                    },
                    () => !IsBusy && !IsDirty));

            }
        }
    }
}
