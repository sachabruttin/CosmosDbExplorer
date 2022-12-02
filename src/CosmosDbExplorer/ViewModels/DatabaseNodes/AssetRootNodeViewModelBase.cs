using System.Threading.Tasks;
using System.Windows.Input;

using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Contracts;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.Models;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Contracts.Services;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public abstract class AssetRootNodeViewModelBase<TResource> : TreeViewItemViewModel<ContainerNodeViewModel>, ICanRefreshNode, IHaveContainerNodeViewModel
        where TResource : ICosmosResource
    {
        private AsyncRelayCommand? _refreshCommand;
        private RelayCommand? _openNewCommand;

        protected AssetRootNodeViewModelBase(ContainerNodeViewModel parent)
            : base(parent, true)
        {
            Messenger.Register<AssetRootNodeViewModelBase<TResource>, UpdateOrCreateNodeMessage<TResource, CosmosContainer>>(this, static (r, m) => r.InnerOnUpdateOrCreateNodeMessage(m));
        }

        public string Name { get; protected set; } = string.Empty;

        public new ContainerNodeViewModel Parent
        {
            get { return base.Parent; }
        }

        public ICommand RefreshCommand => _refreshCommand ??= new(RefreshCommandExecute);

        private Task RefreshCommandExecute()
        {
            Children.Clear();
            return LoadChildren(new System.Threading.CancellationToken());
        }

        public RelayCommand OpenNewCommand => _openNewCommand ??= new(OpenNewCommandExecute);

        protected abstract void OpenNewCommandExecute();

        public ContainerNodeViewModel ContainerNode => Parent;

        private void InnerOnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<TResource, CosmosContainer> message)
        {
            if (message.Parent == ContainerNode.Container)
            {
                OnUpdateOrCreateNodeMessage(message);
            }
        }

        protected abstract void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<TResource, CosmosContainer> message);
    }

    public abstract class AssetNodeViewModelBase<TResource, TParent> :
        TreeViewItemViewModel<TParent>, IAssetNode<TResource>, IHaveOpenCommand
        where TResource : ICosmosResource
        where TParent : AssetRootNodeViewModelBase<TResource>
    {
        private RelayCommand? _openCommand;
        private RelayCommand? _deleteCommand;
        private readonly IDialogService _dialogService;

        protected AssetNodeViewModelBase(TParent parent, TResource resource, ICosmosScriptService cosmosScriptService, IDialogService dialogService)
            : base(parent, false)
        {
            Resource = resource;
            ScriptService = cosmosScriptService;
            _dialogService = dialogService;
        }

        public string Name => Resource.Id ?? "New";

        public string? ContentId => Resource.SelfLink;

        public System.Drawing.Color? AccentColor => Parent.Parent.Parent.Parent.Connection.AccentColor;

        public TResource Resource { get; set; }
        public ICosmosScriptService ScriptService { get; init; }

        public RelayCommand OpenCommand => _openCommand ??= new(async () => await OpenCommandImp());

        protected abstract Task OpenCommandImp();

        public RelayCommand DeleteCommand => _deleteCommand ??= new(async () => await DeleteCommandExecute());

        protected virtual async Task DeleteCommandExecute()
        {
            await _dialogService.ShowQuestion("Are you sure...", "Delete", async confirm =>
            {
                if (confirm)
                {
                    await DeleteCommandImpl();
                    if (!string.IsNullOrEmpty(ContentId))
                    {
                        Messenger.Send(new RemoveNodeMessage(ContentId));
                        Messenger.Send(new CloseDocumentMessage(ContentId));
                    }

                }
            });
        }

        protected abstract Task DeleteCommandImpl();

        //protected IDialogService DialogService => SimpleIoc.Default.GetInstance<IDialogService>();

        //protected IDocumentDbService DbService => SimpleIoc.Default.GetInstance<IDocumentDbService>();
    }
}
