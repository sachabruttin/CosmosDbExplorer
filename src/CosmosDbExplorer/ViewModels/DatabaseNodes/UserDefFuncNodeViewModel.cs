using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Messages;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class UserDefFuncRootNodeViewModel : AssetRootNodeViewModelBase<CosmosUserDefinedFunction>
    {
        public UserDefFuncRootNodeViewModel(ContainerNodeViewModel parent)
            : base(parent)
        {
            Name = "User Defined Functions";
        }

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            //var function = await DbService.GetUdfsAsync(Parent.Parent.Parent.Connection, Parent.Collection).ConfigureAwait(false);

            //foreach (var func in function)
            //{
            //    await DispatcherHelper.RunAsync(() => Children.Add(new UserDefFuncNodeViewModel(this, func)));
            //}

            IsLoading = false;
        }

        protected override void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<CosmosUserDefinedFunction> message)
        {
            throw new System.NotImplementedException();
            //if (message.IsNewResource)
            //{
            //    var item = new UserDefFuncNodeViewModel(this, message.Resource);
            //    DispatcherHelper.RunAsync(() => Children.Add(item));
            //}
            //else
            //{
            //    var item = Children.Cast<UserDefFuncNodeViewModel>().FirstOrDefault(i => i.Resource.AltLink == message.OldAltLink);

            //    if (item != null)
            //    {
            //        item.Resource = message.Resource;
            //    }
            //}
        }
    }

    public class UserDefFuncNodeViewModel : AssetNodeViewModelBase<CosmosUserDefinedFunction, UserDefFuncRootNodeViewModel>
    {
        public UserDefFuncNodeViewModel(UserDefFuncRootNodeViewModel parent, CosmosUserDefinedFunction resource)
            : base(parent, resource)
        {
        }

        protected override Task DeleteCommandImpl()
        {
            throw new System.NotImplementedException();
            //return DialogService.ShowMessage("Are sure you want to delete this User Defined Function?", "Delete", null, null,
            //    async confirm =>
            //    {
            //        if (confirm)
            //        {
            //            await DbService.DeleteUdfAsync(Parent.Parent.Parent.Parent.Connection, Resource.AltLink).ConfigureAwait(false);
            //            await DispatcherHelper.RunAsync(() => Parent.Children.Remove(this));
            //            MessengerInstance.Send(new CloseDocumentMessage(ContentId));
            //        }
            //    });
        }

        protected override Task OpenCommandImp()
        {
            throw new System.NotImplementedException();
            //MessengerInstance.Send(new EditUserDefFuncMessage(this, Parent.Parent.Parent.Parent.Connection, null));
            //return Task.FromResult(0);
        }
    }
}
