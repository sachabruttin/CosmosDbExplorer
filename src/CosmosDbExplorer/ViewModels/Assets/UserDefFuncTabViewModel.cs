using System;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Services;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModels.Assets
{
    public class UserDefFuncTabViewModel : AssetTabViewModelBase<UserDefFuncNodeViewModel, CosmosUserDefinedFunction>
    {
        public UserDefFuncTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices) 
            : base(serviceProvider, uiServices)
        {
        }

        //public UserDefFuncTabViewModel(IDialogService dialogService, IDocumentDbService dbService, IUIServices uiServices)
        //    : base(dialogService, dbService, uiServices)
        //{
        //}

        protected override string GetDefaultHeader() => "New User Defined Function";
        protected override string GetDefaultTitle() => "User Defined Function";
        protected override string GetDefaultContent() => "function userDefinedFunction(){}";

        //protected override Task<CosmosUserDefinedFunction> SaveAsyncImpl(IDocumentDbService dbService)
        //{
        //    return dbService.SaveUdfAsync(Connection, Collection, Id, Content.Text, AltLink);
        //}

        //protected override Task DeleteAsyncImpl(IDocumentDbService dbService)
        //{
        //    return dbService.DeleteUdfAsync(Connection, AltLink);
        //}

        protected override void DiscardCommandExecute()
        {
            throw new System.NotImplementedException();
        }
    }
}
