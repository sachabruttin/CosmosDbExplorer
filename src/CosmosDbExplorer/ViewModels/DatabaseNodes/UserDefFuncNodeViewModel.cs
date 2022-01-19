using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class UserDefFuncRootNodeViewModel : AssetRootNodeViewModelBase<CosmosUserDefinedFunction>
    {
        private readonly IServiceProvider _serviceProvider;

        public UserDefFuncRootNodeViewModel(ContainerNodeViewModel parent, IServiceProvider serviceProvider)
            : base(parent)
        {
            Name = "User Defined Functions";
            _serviceProvider = serviceProvider;
        }

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            var service = ActivatorUtilities.CreateInstance<CosmosScriptService>(_serviceProvider, Parent.Parent.Parent.Connection, Parent.Parent.Database, Parent.Container);
            
            var function = await service.GetUserDefinedFunctionsAsync(token);

            foreach (var func in function)
            {
                //await DispatcherHelper.RunAsync(() => Children.Add(new UserDefFuncNodeViewModel(this, func)));
                Children.Add(new UserDefFuncNodeViewModel(this, func));
            }

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
            Messenger.Send(new EditUserDefFuncMessage(this, Parent.Parent.Parent.Parent.Connection, Parent.Parent.Container));
            return Task.CompletedTask;
        }
    }
}
