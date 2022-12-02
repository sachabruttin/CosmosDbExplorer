using System;
using System.Threading.Tasks;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

using Microsoft.Extensions.DependencyInjection;

namespace CosmosDbExplorer.ViewModels.Assets
{
    public class TriggerTabViewModel : AssetTabViewModelBase<TriggerNodeViewModel, CosmosTrigger>
    {
        private readonly CosmosScriptService _scriptService;

        public TriggerTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices, IDialogService dialogService, string contentId, NodeContext<TriggerNodeViewModel> nodeContext)
            : base(uiServices, dialogService, contentId, nodeContext)
        {
            IconSource = App.Current.FindResource("TriggerIcon");

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

        public CosmosTriggerType TriggerType { get; set; }

        public CosmosTriggerOperation TriggerOperation { get; set; }

        protected override string GetDefaultHeader() => "New Trigger";
        protected override string GetDefaultTitle() => "Trigger";
        protected override string GetDefaultContent() => "function trigger(){}";

        protected override void SetInformationImpl(CosmosTrigger resource)
        {
            TriggerOperation = resource.Operation;
            TriggerType = resource.Type;
            base.SetInformationImpl(resource);
        }

        protected override Task<CosmosTrigger> SaveAsyncImpl()
        {
            if (Id is null)
            {
                throw new Exception("Asset Id is null!");
            }

            var resource = new CosmosTrigger(Id, Content, AltLink)
            {
                Operation = TriggerOperation,
                Type = TriggerType
            };

            return _scriptService.SaveTriggerAsync(resource);
        }

        protected override Task<CosmosResult> DeleteAsyncImpl()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            return _scriptService.DeleteTriggerAsync(Node.Resource);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
    }
}
