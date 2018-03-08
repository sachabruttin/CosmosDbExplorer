using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel.Assets
{
    public class UserDefFuncTabViewModel : AssetTabViewModelBase<UserDefFuncNodeViewModel, UserDefinedFunction>
    {
        public UserDefFuncTabViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService)
            : base(messenger, dialogService, dbService)
        {
        }

        protected override string GetDefaultHeader() { return "New User Definied Function"; }
        protected override string GetDefaultTitle() { return "User Definied Function"; }
        protected override string GetDefaultContent() { return Constants.Default.UserDefiniedFunction; }

        protected override void SetInformationImpl(UserDefinedFunction resource)
        {
            SetText(resource.Body);
        }

        protected override Task<UserDefinedFunction> SaveAsyncImpl(IDocumentDbService dbService)
        {
            return dbService.SaveUdf(Connection, Collection, Id, Content.Text, AltLink);
        }

        protected override Task DeleteAsyncImpl(IDocumentDbService dbService)
        {
            return dbService.DeleteUdf(Connection, AltLink);
        }
    }
}
