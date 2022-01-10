using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.Services;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModel
{
    public abstract class AssetRootNodeViewModelBase<TResource> : TreeViewItemViewModel<CollectionNodeViewModel>, ICanRefreshNode, IHaveCollectionNodeViewModel
        where TResource : Resource
    {
        private RelayCommand _refreshCommand;

        protected AssetRootNodeViewModelBase(CollectionNodeViewModel parent)
            : base(parent, parent.MessengerInstance, true)
        {
            DbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
            MessengerInstance.Register<UpdateOrCreateNodeMessage<TResource>>(this, InnerOnUpdateOrCreateNodeMessage);
        }

        public string Name { get; protected set; }

        public new CollectionNodeViewModel Parent
        {
            get { return base.Parent; }
        }

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                        async () =>
                        {
                            Children.Clear();
                            await LoadChildren().ConfigureAwait(false);
                        }));
            }
        }

        public CollectionNodeViewModel CollectionNode => Parent;

        protected IDocumentDbService DbService { get; }

        private void InnerOnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<TResource> message)
        {
            if (message.Collection == CollectionNode.Collection)
            {
                OnUpdateOrCreateNodeMessage(message);
            }
        }

        protected abstract void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<TResource> message);
    }

    public abstract class AssetNodeViewModelBase<TResource, TParent> :
        TreeViewItemViewModel<TParent>, ICanEditDelete, IAssetNode<TResource>
        where TResource : Resource
        where TParent : AssetRootNodeViewModelBase<TResource>
    {
        protected AssetNodeViewModelBase(TParent parent, TResource resource)
            : base(parent, parent.MessengerInstance, false)
        {
            Resource = resource;
        }

        public string Name => Resource.Id;

        public string ContentId => Resource.AltLink;

        public Color? AccentColor => Parent.Parent.Parent.Parent.Connection.AccentColor;

        public TResource Resource { get; set; }

        public RelayCommand EditCommand => new RelayCommand(async () => await EditCommandImpl().ConfigureAwait(false));

        protected abstract Task EditCommandImpl();

        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(async () => await DeleteCommandImpl().ConfigureAwait(false));
            }
        }

        protected abstract Task DeleteCommandImpl();

        protected IDialogService DialogService => SimpleIoc.Default.GetInstance<IDialogService>();

        protected IDocumentDbService DbService => SimpleIoc.Default.GetInstance<IDocumentDbService>();
    }
}
