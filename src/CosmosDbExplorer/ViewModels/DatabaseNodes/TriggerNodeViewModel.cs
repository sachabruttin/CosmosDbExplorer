using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Messages;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class TriggerRootNodeViewModel : AssetRootNodeViewModelBase<CosmosTrigger>
    {
        private readonly IServiceProvider _serviceProvider;

        public TriggerRootNodeViewModel(ContainerNodeViewModel parent, IServiceProvider serviceProvider)
            : base(parent)
        {
            Name = "Triggers";
            _serviceProvider = serviceProvider;
        }

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            var service = ActivatorUtilities.CreateInstance<CosmosScriptService>(_serviceProvider, Parent.Parent.Parent.Connection, Parent.Parent.Database, Parent.Container);

            var function = await service.GetTriggersAsync(token);

            foreach (var func in function)
            {
                //await DispatcherHelper.RunAsync(() => Children.Add(new UserDefFuncNodeViewModel(this, func)));
                Children.Add(new TriggerNodeViewModel(this, func));
            }

            IsLoading = false;
        }

        protected override void OpenNewCommandExecute()
        {
            Parent.NewTriggerCommand.Execute(this);
        }

        protected override void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<CosmosTrigger, ContainerNodeViewModel> message)
        {
            if (message.IsNewResource)
            {
                var item = new TriggerNodeViewModel(this, message.Resource);
                Children.Add(item);
            }
            else
            {
                var item = Children.Cast<TriggerNodeViewModel>().FirstOrDefault(i => i.Resource.SelfLink == message.OldAltLink);

                if (item != null)
                {
                    item.Resource = message.Resource;
                }
            }
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
            Messenger.Send(new EditTriggerMessage(this, Parent.Parent.Parent.Parent.Connection, Parent.Parent.Container));
            return Task.CompletedTask;
        }
    }
}
