using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public abstract class AssetRootNodeViewModelBase<TResource> : TreeViewItemViewModel, ICanRefreshNode, IHaveCollectionNodeViewModel
        where TResource : Resource
    {
        private RelayCommand _refreshCommand;

        protected AssetRootNodeViewModelBase(CollectionNodeViewModel parent)
            : base(parent, parent.MessengerInstance, true)
        {
            DbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
            MessengerInstance.Register<UpdateOrCreateNodeMessage<TResource>>(this, OnUpdateOrCreateNodeMessage);
        }

        public string Name { get; protected set; }

        public new CollectionNodeViewModel Parent
        {
            get { return base.Parent as CollectionNodeViewModel; }
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

        protected abstract void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<TResource> message);
    }
}
