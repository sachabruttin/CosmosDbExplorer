using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel.Assets
{
    public class TriggerTabViewModel : AssetTabViewModelBase<TriggerNodeViewModel, Trigger>
    {
        private TriggerType _triggerType;
        private TriggerOperation _triggerOperation;

        public TriggerTabViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService)
            : base(messenger, dialogService, dbService)
        {
        }

        protected override string GetDefaultHeader() { return "New Trigger"; }
        protected override string GetDefaultTitle() { return "Trigger"; }
        protected override string GetDefaultContent() { return Constants.Default.Trigger; }

        protected override void SetInformationImpl(Trigger resource)
        {
            TriggerOperation = resource.TriggerOperation;
            TriggerType = resource.TriggerType;
            SetText(resource.Body);
        }

        public TriggerType TriggerType
        {
            get { return _triggerType; }
            set
            {
                if (value != _triggerType)
                {
                    _triggerType = value;
                    IsDirty = true;
                    RaisePropertyChanged(() => TriggerType);
                }
            }
        }

        public TriggerOperation TriggerOperation
        {
            get { return _triggerOperation; }
            set
            {
                if (value != _triggerOperation)
                {
                    _triggerOperation = value;
                    IsDirty = true;
                    RaisePropertyChanged(() => TriggerOperation);
                }
            }
        }

        protected override Task<Trigger> SaveAsyncImpl(IDocumentDbService dbService)
        {
            return dbService.SaveTrigger(Connection, Collection, Id, Content.Text, TriggerType, TriggerOperation, AltLink);
        }

        protected override Task DeleteAsyncImpl(IDocumentDbService dbService)
        {
            return dbService.DeleteTrigger(Connection, AltLink);
        }
    }
}
