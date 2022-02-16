using System.Threading.Tasks;
using System.Windows.Input;

using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Contracts;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.Models;

using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public abstract class AssetRootNodeViewModelBase<TResource> : TreeViewItemViewModel<ContainerNodeViewModel>, ICanRefreshNode, IHaveContainerNodeViewModel
        where TResource : ICosmosResource
    {
        private AsyncRelayCommand _refreshCommand;
        private RelayCommand _openNewCommand;

        protected AssetRootNodeViewModelBase(ContainerNodeViewModel parent)
            : base(parent, true)
        {
            Messenger.Register<AssetRootNodeViewModelBase<TResource>, UpdateOrCreateNodeMessage<TResource, ContainerNodeViewModel>>(this, static (r, m) => r.InnerOnUpdateOrCreateNodeMessage(m));
        }

        public string Name { get; protected set; }

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

        private void InnerOnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<TResource, ContainerNodeViewModel> message)
        {
            if (message.Parent.Container == ContainerNode.Container)
            {
                OnUpdateOrCreateNodeMessage(message);
            }
        }

        protected abstract void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<TResource, ContainerNodeViewModel> message);
    }

    public abstract class AssetNodeViewModelBase<TResource, TParent> :
        TreeViewItemViewModel<TParent>, IAssetNode<TResource>, IHaveOpenCommand
        where TResource : ICosmosResource
        where TParent : AssetRootNodeViewModelBase<TResource>
    {
        private RelayCommand _openCommand;
        private RelayCommand _deleteCommand;

        protected AssetNodeViewModelBase(TParent parent, TResource resource)
            : base(parent, false)
        {
            Resource = resource;
        }

        public string Name => Resource.Id;

        public string ContentId => Resource.Id;

        public System.Drawing.Color? AccentColor => Parent.Parent.Parent.Parent.Connection.AccentColor;

        public TResource Resource { get; set; }

        public RelayCommand OpenCommand => _openCommand ??=  new(async () => await OpenCommandImp().ConfigureAwait(false));

        protected abstract Task OpenCommandImp();

        public RelayCommand DeleteCommand => _deleteCommand ??= new(async () => await DeleteCommandImpl().ConfigureAwait(false));

        protected abstract Task DeleteCommandImpl();

        //protected IDialogService DialogService => SimpleIoc.Default.GetInstance<IDialogService>();

        //protected IDocumentDbService DbService => SimpleIoc.Default.GetInstance<IDocumentDbService>();
    }
}
