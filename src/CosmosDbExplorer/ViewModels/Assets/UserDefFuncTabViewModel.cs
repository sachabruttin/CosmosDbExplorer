﻿using System;
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
        private ICosmosScriptService _scriptService;
        private readonly IServiceProvider _serviceProvider;

        public UserDefFuncTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices, IDialogService dialogService)
            : base(uiServices, dialogService)
        {
            IconSource = App.Current.FindResource("UdfIcon");
            _serviceProvider = serviceProvider;
        }

        public override void Load(string contentId, NodeContext<UserDefFuncNodeViewModel> nodeContext)
        {
            _scriptService = ActivatorUtilities.CreateInstance<CosmosScriptService>(_serviceProvider, nodeContext.Connection, nodeContext.Database, nodeContext.Container);

            base.Load(contentId, nodeContext);
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
            return _scriptService.DeleteUserDefinedFunctionAsync(Node.Resource);
        }
    }
}
