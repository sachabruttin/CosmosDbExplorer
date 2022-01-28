using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Messages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels
{
    /// <summary>
    /// Base class for all ViewModel classes displayed by TreeViewItems.  
    /// This acts as an adapter between a raw data object and a TreeViewItem.
    /// </summary>
    public class TreeViewItemViewModel : ObservableRecipient
    {
        private static readonly TreeViewItemViewModel DummyChild = new();

        protected TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren)
        {
            Parent = parent;
            Children = new ObservableCollection<TreeViewItemViewModel>();

            Messenger.Register<TreeViewItemViewModel, RemoveNodeMessage>(this, static (r, m) => r.OnRemoveNodeMessage(m));

            if (lazyLoadChildren)
            {
                Children.Add(DummyChild);
            }
        }

        private void OnRemoveNodeMessage(RemoveNodeMessage msg)
        {
            if (Parent != null)
            {
                if (this is IContent assetNode && assetNode.ContentId == msg.AltLink)
                {
                    Parent.Children.Remove(this);
                }
            }
        }

        // This is used to create the DummyChild instance.
        private TreeViewItemViewModel()
        {
        }

        /// <summary>
        /// Returns the logical child items of this object.
        /// </summary>
        public ObservableCollection<TreeViewItemViewModel> Children { get; }

        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        public bool HasDummyChild
        {
            get { return Children.Count == 1 && Children[0] == DummyChild; }
        }

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded { get; set; }

        public async void OnIsExpandedChanged()
        {
            // Expand all the way up to the root.
            if (IsExpanded && Parent != null)
            {
                Parent.IsExpanded = true;
            }

            // Lazy load the child items, if necessary.
            if (HasDummyChild)
            {
                Children.Remove(DummyChild);
                var token = new CancellationToken();
                await LoadChildren(token);
            }
        }

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected { get; set; }

        public void OnIsSelectedChanged()
        {
            Messenger.Send(new TreeNodeSelectedMessage(this));
            NotifyCanExecuteChanged();
        }

        protected virtual void NotifyCanExecuteChanged()
        {

        }

        public bool IsLoading { get; set; }

        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// Subclasses can override this to populate the Children collection.
        /// </summary>
        protected virtual Task LoadChildren(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public TreeViewItemViewModel Parent { get; }
    }

    public class TreeViewItemViewModel<TParent> : TreeViewItemViewModel
        where TParent : TreeViewItemViewModel
    {
        public TreeViewItemViewModel(TParent parent, bool lazyLoadChildren)
            : base(parent, lazyLoadChildren)
        {
        }

        public new TParent Parent => base.Parent as TParent;
    }
}
