using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Extensions;
using CosmosDbExplorer.Messages;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Contracts.Services;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class TriggerRootNodeViewModel : AssetRootNodeViewModelBase<CosmosTrigger>
    {
        private readonly IServiceProvider _serviceProvider;
        private CosmosScriptService _scriptService;

        public TriggerRootNodeViewModel(ContainerNodeViewModel parent, IServiceProvider serviceProvider)
            : base(parent)
        {
            Name = "Triggers";
            _serviceProvider = serviceProvider;
            _scriptService = ActivatorUtilities.CreateInstance<CosmosScriptService>(_serviceProvider, Parent.Parent.Parent.Connection, Parent.Parent.Database, Parent.Container);
        }

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            var service = ActivatorUtilities.CreateInstance<CosmosScriptService>(_serviceProvider, Parent.Parent.Parent.Connection, Parent.Parent.Database, Parent.Container);

            var function = await service.GetTriggersAsync(token);

            foreach (var func in function)
            {
                var vm = ActivatorUtilities.CreateInstance<TriggerNodeViewModel>(_serviceProvider, this, func, _scriptService);
                Children.Add(vm);
            }

            IsLoading = false;
        }

        protected override void OpenNewCommandExecute()
        {
            Parent.NewTriggerCommand.Execute(this);
        }

        protected override void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<CosmosTrigger, CosmosContainer> message)
        {
            if (message.IsNewResource)
            {
                var item = ActivatorUtilities.CreateInstance<TriggerNodeViewModel>(_serviceProvider, this, message.Resource, _scriptService);
                Children.AddSorted(item, i => ((TriggerNodeViewModel)i).Name);
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
        public TriggerNodeViewModel(TriggerRootNodeViewModel parent, CosmosTrigger resource, ICosmosScriptService cosmosScriptService, IDialogService dialogService)
            : base(parent, resource, cosmosScriptService, dialogService)
        {
        }

        protected override Task DeleteCommandImpl()
        {
            return ScriptService.DeleteTriggerAsync(Resource);
        }

        protected override Task OpenCommandImp()
        {
            Messenger.Send(new EditTriggerMessage(this, Parent.Parent.Parent.Parent.Connection, Parent.Parent.Parent.Database, Parent.Parent.Container));
            return Task.CompletedTask;
        }
    }
}
