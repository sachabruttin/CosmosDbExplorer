using System;
using System.Threading.Tasks;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

using Microsoft.Extensions.DependencyInjection;

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

        public override void Load(string contentId, TriggerNodeViewModel node, CosmosConnection connection, CosmosDatabase database, CosmosContainer container)
        {
            _scriptService = ActivatorUtilities.CreateInstance<CosmosScriptService>(_serviceProvider, connection, database, container);

            base.Load(contentId, node, connection, database, container);
        }

        //private TriggerType _triggerType;
        //private TriggerOperation _triggerOperation;

        protected override string GetDefaultHeader() => "New Trigger";
        protected override string GetDefaultTitle() => "Trigger";
        protected override string GetDefaultContent() => "function trigger(){}";


        protected override void SetInformationImpl(CosmosTrigger resource)
        {
            //TriggerOperation = resource.TriggerOperation;
            //TriggerType = resource.TriggerType;
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
                //Operation = Microsoft.Azure.Cosmos.Scripts.TriggerOperation.All
                //Type = Microsoft.Azure.Cosmos.Scripts.TriggerType.Pre
            };

            return _scriptService.SaveTriggerAsync(resource);
        }

        protected override Task<CosmosResult> DeleteAsyncImpl()
        {
            return _scriptService.DeleteTriggerAsync(Node.Resource);
        }

        //public TriggerType TriggerType
        //{
        //    get { return _triggerType; }
        //    set
        //    {
        //        if (value != _triggerType)
        //        {
        //            _triggerType = value;
        //            IsDirty = true;
        //            RaisePropertyChanged(() => TriggerType);
        //        }
        //    }
        //}

        //public TriggerOperation TriggerOperation
        //{
        //    get { return _triggerOperation; }
        //    set
        //    {
        //        if (value != _triggerOperation)
        //        {
        //            _triggerOperation = value;
        //            IsDirty = true;
        //            RaisePropertyChanged(() => TriggerOperation);
        //        }
        //    }
        //}

    }
}
