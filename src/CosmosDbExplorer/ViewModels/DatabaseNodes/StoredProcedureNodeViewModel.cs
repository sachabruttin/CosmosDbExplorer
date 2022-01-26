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
    public class StoredProcedureRootNodeViewModel : AssetRootNodeViewModelBase<CosmosStoredProcedure>
    {
        private readonly IServiceProvider _serviceProvider;

        public StoredProcedureRootNodeViewModel(ContainerNodeViewModel parent, IServiceProvider serviceProvider)
            : base(parent)
        {
            Name = "Stored Procedures";
            _serviceProvider = serviceProvider;
        }

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            var service = ActivatorUtilities.CreateInstance<CosmosScriptService>(_serviceProvider, Parent.Parent.Parent.Connection, Parent.Parent.Database, Parent.Container);

            var function = await service.GetStoredProceduresAsync(token);

            foreach (var func in function)
            {
                //await DispatcherHelper.RunAsync(() => Children.Add(new UserDefFuncNodeViewModel(this, func)));
                Children.Add(new StoredProcedureNodeViewModel(this, func));
            }

            IsLoading = false;
        }

        protected override void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<CosmosStoredProcedure, ContainerNodeViewModel> message)
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
            Messenger.Send(new EditStoredProcedureMessage(this, Parent.Parent.Parent.Parent.Connection, Parent.Parent.Container));
            return Task.CompletedTask;
        }
    }
}
