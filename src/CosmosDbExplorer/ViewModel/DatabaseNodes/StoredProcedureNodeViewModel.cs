using System.Linq;
using System.Threading.Tasks;
using CosmosDbExplorer.Messages;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModel
{
    public class StoredProcedureRootNodeViewModel : AssetRootNodeViewModelBase<StoredProcedure>
    {
        public StoredProcedureRootNodeViewModel(CollectionNodeViewModel parent)
            : base(parent)
        {
        }

        public override string Name => "Stored Procedures";

        protected override async Task LoadChildren()
        {
            IsLoading = true;

            var storedProcedure = await DbService.GetStoredProceduresAsync(Parent.Parent.Parent.Connection, Parent.Collection).ConfigureAwait(false);

            foreach (var sp in storedProcedure)
            {
                await DispatcherHelper.RunAsync(() => Children.Add(new StoredProcedureNodeViewModel(this, sp)));
            }

            IsLoading = false;
        }

        protected override void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<StoredProcedure> message)
        {
            if (message.IsNewResource)
            {
                var item = new StoredProcedureNodeViewModel(this, message.Resource);
                DispatcherHelper.RunAsync(() => Children.Add(item));
            }
            else
            {
                var item = Children.Cast<StoredProcedureNodeViewModel>().FirstOrDefault(i => i.Resource.AltLink == message.OldAltLink);

                if (item != null)
                {
                    item.Resource = message.Resource;
                }
            }
        }
    }

    public class StoredProcedureNodeViewModel : AssetNodeViewModelBase<StoredProcedure, StoredProcedureRootNodeViewModel>
    {
        public StoredProcedureNodeViewModel(StoredProcedureRootNodeViewModel parent, StoredProcedure resource)
            : base(parent, resource)
        {
        }

        protected override Task DeleteCommandImpl()
        {
            return DialogService.ShowMessage("Are sure you want to delete this Stored Procedure?", "Delete", null, null,
                async confirm =>
                {
                    if (confirm)
                    {
                        await DbService.DeleteStoredProcedureAsync(Parent.Parent.Parent.Parent.Connection, Resource.AltLink).ConfigureAwait(false);
                        await DispatcherHelper.RunAsync(() => Parent.Children.Remove(this));
                        MessengerInstance.Send(new CloseDocumentMessage(ContentId));
                    }
                });
        }

        protected override Task EditCommandImpl()
        {
            MessengerInstance.Send(new EditStoredProcedureMessage(this, Parent.Parent.Parent.Parent.Connection, Parent.Parent.Collection));
            return Task.FromResult(0);
        }
    }
}
