using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Messages;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class TriggerRootNodeViewModel : AssetRootNodeViewModelBase<CosmosTrigger>
    {
        public TriggerRootNodeViewModel(ContainerNodeViewModel parent)
            : base(parent)
        {
            Name = "Triggers";
        }

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            //var triggers = await DbService.GetTriggersAsync(Parent.Parent.Parent.Connection, Parent.Collection).ConfigureAwait(false);

            //foreach (var trigger in triggers)
            //{
            //    await DispatcherHelper.RunAsync(() => Children.Add(new TriggerNodeViewModel(this, trigger)));
            //}

            IsLoading = false;
        }

        protected override void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<CosmosTrigger> message)
        {
            throw new System.NotImplementedException();
            //if (message.IsNewResource)
            //{
            //    var item = new TriggerNodeViewModel(this, message.Resource);
            //    DispatcherHelper.RunAsync(() => Children.Add(item));
            //}
            //else
            //{
            //    var item = Children.Cast<TriggerNodeViewModel>().FirstOrDefault(i => i.Resource.AltLink == message.OldAltLink);

            //    if (item != null)
            //    {
            //        item.Resource = message.Resource;
            //    }
            //}
        }
    }

    public class TriggerNodeViewModel : AssetNodeViewModelBase<CosmosTrigger, TriggerRootNodeViewModel>
    {
        public TriggerNodeViewModel(TriggerRootNodeViewModel parent, CosmosTrigger resource)
            : base(parent, resource)
        {
        }

        protected override Task DeleteCommandImpl()
        {
            throw new System.NotImplementedException();
            //return DialogService.ShowMessage("Are sure you want to delete this Trigger?", "Delete", null, null,
            //    async confirm =>
            //    {
            //        if (confirm)
            //        {
            //            await DbService.DeleteTriggerAsync(Parent.Parent.Parent.Parent.Connection, Resource.AltLink).ConfigureAwait(false);
            //            await DispatcherHelper.RunAsync(() => Parent.Children.Remove(this));
            //            MessengerInstance.Send(new CloseDocumentMessage(ContentId));
            //        }
            //    });
        }

        protected override Task OpenCommandImp()
        {
            throw new System.NotImplementedException();
            //Messenger.Send(new EditTriggerMessage(this, Parent.Parent.Parent.Parent.Connection, Parent.Parent.Collection));
            //return Task.FromResult(0);
        }
    }
}
