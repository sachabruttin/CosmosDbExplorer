using System;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;


namespace CosmosDbExplorer.ViewModels.Assets
{
    public class TriggerTabViewModel : AssetTabViewModelBase<TriggerNodeViewModel, CosmosTrigger>
    {
        public TriggerTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices) : base(serviceProvider, uiServices)
        {
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

        protected override void DiscardCommandExecute()
        {
            throw new NotImplementedException();
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

        //protected override Task<Trigger> SaveAsyncImpl(IDocumentDbService dbService)
        //{
        //    return dbService.SaveTriggerAsync(Connection, Collection, Id, Content.Text, TriggerType, TriggerOperation, AltLink);
        //}

        //protected override Task DeleteAsyncImpl(IDocumentDbService dbService)
        //{
        //    return dbService.DeleteTriggerAsync(Connection, AltLink);
        //}
    }
}
