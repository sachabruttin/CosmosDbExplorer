using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Services;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModel.Assets
{
    public class UserDefFuncTabViewModel : AssetTabViewModelBase<UserDefFuncNodeViewModel, UserDefinedFunction>
    {
        public UserDefFuncTabViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService, IUIServices uiServices)
            : base(messenger, dialogService, dbService, uiServices)
        {
        }

        protected override string GetDefaultHeader() { return "New User Defined Function"; }
        protected override string GetDefaultTitle() { return "User Defined Function"; }
        protected override string GetDefaultContent() { return Constants.Default.UserDefiniedFunction; }

        protected override void SetInformationImpl(UserDefinedFunction resource)
        {
            SetText(resource.Body);
        }

        protected override Task<UserDefinedFunction> SaveAsyncImpl(IDocumentDbService dbService)
        {
            return dbService.SaveUdfAsync(Connection, Collection, Id, Content.Text, AltLink);
        }

        protected override Task DeleteAsyncImpl(IDocumentDbService dbService)
        {
            return dbService.DeleteUdfAsync(Connection, AltLink);
        }
    }
}
