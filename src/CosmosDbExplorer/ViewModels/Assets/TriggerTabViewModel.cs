using System;
using System.Threading.Tasks;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

using Microsoft.Extensions.DependencyInjection;

using PropertyChanged;

namespace CosmosDbExplorer.ViewModels.Assets
{
    public class TriggerTabViewModel : AssetTabViewModelBase<TriggerNodeViewModel, CosmosTrigger>
    {
        private readonly IServiceProvider _serviceProvider;
        private CosmosScriptService _scriptService;

        public TriggerTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices, IDialogService dialogService)
            : base(uiServices, dialogService)
        {
            IconSource = App.Current.FindResource("TriggerIcon");
            _serviceProvider = serviceProvider;
        }

        public override void Load(string contentId, NodeContext<TriggerNodeViewModel> nodeContext)
        {
            _scriptService = ActivatorUtilities.CreateInstance<CosmosScriptService>(_serviceProvider, nodeContext.Connection, nodeContext.Database, nodeContext.Container);

            base.Load(contentId, nodeContext);
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
            return _scriptService.DeleteTriggerAsync(Node.Resource);
        }
    }
}
