using System;
using System.Threading.Tasks;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

using Microsoft.Extensions.DependencyInjection;

namespace CosmosDbExplorer.ViewModels.Assets
{
    public class UserDefFuncTabViewModel : AssetTabViewModelBase<UserDefFuncNodeViewModel, CosmosUserDefinedFunction>
    {
        private readonly ICosmosScriptService _scriptService;

        public UserDefFuncTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices, IDialogService dialogService, string contentId, NodeContext<UserDefFuncNodeViewModel> nodeContext)
            : base(uiServices, dialogService, contentId, nodeContext)
        {
            IconSource = App.Current.FindResource("UdfIcon");

            if (nodeContext.Connection is null || nodeContext.Container is null || nodeContext.Database is null)
            {
                throw new NullReferenceException("Node context is not correctly initialized!");
            }

            _scriptService = ActivatorUtilities.CreateInstance<CosmosScriptService>(serviceProvider, nodeContext.Connection, nodeContext.Database, nodeContext.Container);
        }

        public override Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        protected override string GetDefaultHeader() => "New User Defined Function";
        protected override string GetDefaultTitle() => "User Defined Function";
        protected override string GetDefaultContent() => "function userDefinedFunction(){}";

        protected override Task<CosmosUserDefinedFunction> SaveAsyncImpl()
        {
            if (Id is null)
            {
                throw new Exception("Asset Id is null!");
            }

            var resource = new CosmosUserDefinedFunction(Id, Content, AltLink);
            return _scriptService.SaveUserDefinedFunctionAsync(resource);
        }

        protected override Task<CosmosResult> DeleteAsyncImpl()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            return _scriptService.DeleteUserDefinedFunctionAsync(Node.Resource);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
    }
}
