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
    public class UserDefFuncRootNodeViewModel : AssetRootNodeViewModelBase<CosmosUserDefinedFunction>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CosmosScriptService _scriptService;

        public UserDefFuncRootNodeViewModel(ContainerNodeViewModel parent, IServiceProvider serviceProvider)
            : base(parent)
        {
            Name = "User Defined Functions";
            _serviceProvider = serviceProvider;
            _scriptService = ActivatorUtilities.CreateInstance<CosmosScriptService>(_serviceProvider, Parent.Parent.Parent.Connection, Parent.Parent.Database, Parent.Container);
        }

        protected override async Task LoadChildren(CancellationToken token)
        {
            IsLoading = true;

            var function = await _scriptService.GetUserDefinedFunctionsAsync(token);

            foreach (var func in function)
            {
                var vm = ActivatorUtilities.CreateInstance<UserDefFuncNodeViewModel>(_serviceProvider, this, func, _scriptService);
                Children.Add(vm);
            }

            IsLoading = false;
        }

        protected override void OpenNewCommandExecute()
        {
            Parent.NewUdfCommand.Execute(this);
        }

        protected override void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<CosmosUserDefinedFunction, CosmosContainer> message)
        {
            if (message.IsNewResource)
            {
                var item = ActivatorUtilities.CreateInstance<UserDefFuncNodeViewModel>(_serviceProvider, this, message.Resource, _scriptService);
                Children.AddSorted(item, i => ((UserDefFuncNodeViewModel)i).Name);
            }
            else
            {
                var item = Children.Cast<UserDefFuncNodeViewModel>().FirstOrDefault(i => i.Resource.SelfLink == message.OldAltLink);

                if (item != null)
                {
                    item.Resource = message.Resource;
                }
            }
        }
    }

    public class UserDefFuncNodeViewModel : AssetNodeViewModelBase<CosmosUserDefinedFunction, UserDefFuncRootNodeViewModel>
    {
        public UserDefFuncNodeViewModel(UserDefFuncRootNodeViewModel parent, CosmosUserDefinedFunction resource, ICosmosScriptService cosmosScriptService, IDialogService dialogService)
            : base(parent, resource, cosmosScriptService, dialogService)
        {
        }

        protected override Task DeleteCommandImpl()
        {
            return ScriptService.DeleteUserDefinedFunctionAsync(Resource);
        }

        protected override Task OpenCommandImp()
        {
            Messenger.Send(new EditUserDefFuncMessage(this, Parent.Parent.Parent.Parent.Connection, Parent.Parent.Parent.Database, Parent.Parent.Container));
            return Task.CompletedTask;
        }
    }
}
