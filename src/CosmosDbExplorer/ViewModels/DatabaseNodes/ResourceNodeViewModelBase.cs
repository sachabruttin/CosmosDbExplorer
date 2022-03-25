using System.Threading;
using System.Windows;
using System.Windows.Input;

using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Contracts;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public abstract class ResourceNodeViewModelBase<TParent> : TreeViewItemViewModel<TParent>, ICanRefreshNode, IContent
        where TParent : TreeViewItemViewModel
    {
        private RelayCommand _refreshCommand;
        private RelayCommand _copySelfLinkToClipboardCommand;

        protected ResourceNodeViewModelBase(ICosmosResource resource, TParent parent, bool lazyLoadChildren)
            : base(parent, lazyLoadChildren)
        {
            Resource = resource;
        }

        public string Name => Resource?.Id ?? "Unknown";
        public string? ContentId => Resource?.SelfLink;

        public ICommand RefreshCommand => _refreshCommand ??= new(RefreshCommandExecute);

        private async void RefreshCommandExecute()
        {
            Children.Clear();
            await LoadChildren(new CancellationToken()).ConfigureAwait(false);
        }

        public RelayCommand CopySelfLinkToClipboardCommand => _copySelfLinkToClipboardCommand ??= new(() => Clipboard.SetText(Resource.SelfLink));

        protected ICosmosResource Resource { get; set; }

    }
}
