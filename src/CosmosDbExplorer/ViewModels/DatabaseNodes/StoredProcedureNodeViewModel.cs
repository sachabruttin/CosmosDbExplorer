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
    public class StoredProcedureRootNodeViewModel : AssetRootNodeViewModelBase<CosmosStoredProcedure>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CosmosScriptService _scriptService;

        public StoredProcedureRootNodeViewModel(ContainerNodeViewModel parent, IServiceProvider serviceProvider)
            : base(parent)
        {
            Name = "Stored Procedures";
            _serviceProvider = serviceProvider;
            _scriptService = ActivatorUtilities.CreateInstance<CosmosScriptService>(_serviceProvider, Parent.Parent.Parent.Connection, Parent.Parent.Database, Parent.Container);
        }

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            var function = await _scriptService.GetStoredProceduresAsync(token);

            foreach (var func in function)
            {
                var vm = ActivatorUtilities.CreateInstance<StoredProcedureNodeViewModel>(_serviceProvider, this, func, _scriptService);
                Children.Add(vm);
            }

            IsLoading = false;
        }

        protected override void OpenNewCommandExecute()
        {
            Parent.NewStoredProcedureCommand.Execute(this);
        }

        protected override void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<CosmosStoredProcedure, CosmosContainer> message)
        {
            if (message.IsNewResource)
            {
                var item = ActivatorUtilities.CreateInstance<StoredProcedureNodeViewModel>(_serviceProvider, this, message.Resource, _scriptService);
                Children.AddSorted(item, i => ((StoredProcedureNodeViewModel)i).Name);
            }
            else
            {
                var item = Children.Cast<StoredProcedureNodeViewModel>().FirstOrDefault(i => i.Resource.SelfLink == message.OldAltLink);

                if (item != null)
                {
                    item.Resource = message.Resource;
                }
            }
        }
    }

    public class StoredProcedureNodeViewModel : AssetNodeViewModelBase<CosmosStoredProcedure, StoredProcedureRootNodeViewModel>
    {
        public StoredProcedureNodeViewModel(StoredProcedureRootNodeViewModel parent, CosmosStoredProcedure resource, ICosmosScriptService cosmosScriptService, IDialogService dialogService)
            : base(parent, resource, cosmosScriptService, dialogService)
        {
        }

        protected override Task DeleteCommandImpl()
        {
            return ScriptService.DeleteStoredProcedureAsync(Resource);
        }

        protected override Task OpenCommandImp()
        {
            Messenger.Send(new EditStoredProcedureMessage(this, Parent.Parent.Parent.Parent.Connection, Parent.Parent.Parent.Database, Parent.Parent.Container));
            return Task.CompletedTask;
        }
    }
}
