using System;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.ViewModels.Assets
{
    public class UserDefFuncTabViewModel : AssetTabViewModelBase<UserDefFuncNodeViewModel, CosmosUserDefinedFunction>
    {
        public UserDefFuncTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices) 
            : base(serviceProvider, uiServices)
        {
            IconSource = App.Current.FindResource("UdfIcon");
        }

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
