using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel.Assets
{
    public class StoredProcedureTabViewModel : AssetTabViewModelBase<StoredProcedureNodeViewModel, StoredProcedure>
    {

        public StoredProcedureTabViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService)
            : base(messenger, dialogService, dbService)
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
