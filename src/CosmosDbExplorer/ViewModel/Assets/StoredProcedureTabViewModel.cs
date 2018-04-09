using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Services;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModel.Assets
{
    public class StoredProcedureTabViewModel : AssetTabViewModelBase<StoredProcedureNodeViewModel, StoredProcedure>
    {
        public StoredProcedureTabViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService, IUIServices uiServices)
            : base(messenger, dialogService, dbService, uiServices)
        {
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
    }
}
