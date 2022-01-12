using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Messages;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class StoredProcedureRootNodeViewModel : AssetRootNodeViewModelBase<CosmosStoredProcedure>
    {
        public StoredProcedureRootNodeViewModel(ContainerNodeViewModel parent)
            : base(parent)
        {
            Name = "Stored Procedures";
        }

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            //var storedProcedure = await DbService.GetStoredProceduresAsync(Parent.Parent.Parent.Connection, Parent.Collection).ConfigureAwait(false);

            //foreach (var sp in storedProcedure)
            //{
            //    await DispatcherHelper.RunAsync(() => Children.Add(new StoredProcedureNodeViewModel(this, sp)));
            //}

            IsLoading = false;
        }

        protected override void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<CosmosStoredProcedure> message)
        {
            //if (message.IsNewResource)
            //{
            //    var item = new StoredProcedureNodeViewModel(this, message.Resource);
            //    DispatcherHelper.RunAsync(() => Children.Add(item));
            //}
            //else
            //{
            //    var item = Children.Cast<StoredProcedureNodeViewModel>().FirstOrDefault(i => i.Resource.AltLink == message.OldAltLink);

            //    if (item != null)
            //    {
            //        item.Resource = message.Resource;
            //    }
            //}
        }
    }

    public class StoredProcedureNodeViewModel : AssetNodeViewModelBase<CosmosStoredProcedure, StoredProcedureRootNodeViewModel>
    {
        public StoredProcedureNodeViewModel(StoredProcedureRootNodeViewModel parent, CosmosStoredProcedure resource)
            : base(parent, resource)
        {
        }

        protected override Task DeleteCommandImpl()
        {
            throw new System.NotImplementedException();
            //return DialogService.ShowMessage("Are sure you want to delete this Stored Procedure?", "Delete", null, null,
            //    async confirm =>
            //    {
            //        if (confirm)
            //        {
            //            await DbService.DeleteStoredProcedureAsync(Parent.Parent.Parent.Parent.Connection, Resource.AltLink).ConfigureAwait(false);
            //            await DispatcherHelper.RunAsync(() => Parent.Children.Remove(this));
            //            MessengerInstance.Send(new CloseDocumentMessage(ContentId));
            //        }
            //    });
        }

        protected override Task OpenCommandImp()
        {
            throw new System.NotImplementedException();
            //Messenger.Send(new EditStoredProcedureMessage(this, Parent.Parent.Parent.Parent.Connection, Parent.Parent.Collection));
            //return Task.FromResult(0);
        }
    }
}
