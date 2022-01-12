using System.Collections.ObjectModel;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.Core.Models;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using PropertyChanged;

namespace CosmosDbExplorer.ViewModels
{
    public abstract class PaneViewModelBase : UIViewModelBase
    {
        private RelayCommand _closeCommand;
        private readonly StatusBarItem _pathStatusBarItem;

        protected PaneViewModelBase(IUIServices uiServices)
            : base(uiServices)
        {
            _pathStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = ToolTip, IsVisible = true }, StatusBarItemType.SimpleText, "Path", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_pathStatusBarItem);
        }

        [DoNotSetChanged]
        public string Title { get; set; }

        [DoNotSetChanged]
        public string ToolTip { get; set; }

        public virtual void OnToolTipChanged()
        {
            _pathStatusBarItem.DataContext.Value = ToolTip;
        }

        [DoNotSetChanged]
        public string Header { get; set; }

        [DoNotSetChanged]
        public string ContentId { get; protected set; }

        [DoNotSetChanged]
        public bool IsSelected { get; set; }

        //[DoNotSetChanged]
        //public bool IsActive { get; set; }

        [DoNotSetChanged]
        public bool IsClosed { get; set; }

        public virtual void OnIsActiveChanged()
        {
            //DispatcherHelper.RunAsync(() => MessengerInstance.Send(new ActivePaneChangedMessage(this)));
            Messenger.Send(new ActivePaneChangedMessage(this));
        }

        [DoNotSetChanged]
        public ObservableCollection<StatusBarItem> StatusBarItems { get; protected set; } = new ObservableCollection<StatusBarItem>();

        [DoNotSetChanged]
        public object IconSource { get; set; }

        [DoNotSetChanged]
        public System.Drawing.Color? AccentColor { get; set; }

        public RelayCommand CloseCommand
        {
            get
            {
                return _closeCommand ??= new RelayCommand(OnClose, CanClose);
            }
        }

        protected virtual bool CanClose()
        {
            return !IsClosed;
        }

        protected virtual void OnClose()
        {
            Messenger.Send(new CloseDocumentMessage(this));
            OnDeactivated();
            IsClosed = true;
        }
    }

    public abstract class PaneViewModel<TNodeViewModel> : PaneViewModelBase
        where TNodeViewModel : TreeViewItemViewModel
    {
        protected PaneViewModel(IUIServices uiServices)
            : base(uiServices)
        {

        }

        // TODO: Define CosmosDB Connection and DocumentCollection
        public abstract void Load(string contentId, TNodeViewModel node, CosmosConnection connection, CosmosContainer container);
    }

    public abstract class PaneWithZoomViewModel<TNodeViewModel> : PaneViewModel<TNodeViewModel>
        where TNodeViewModel : TreeViewItemViewModel
    {
        protected PaneWithZoomViewModel(IUIServices uiServices)
            : base(uiServices)
        {
            StatusBarItems.Add(new StatusBarItem(new StatusBarItemContext { Value = this, IsVisible = true }, StatusBarItemType.Zoom, "Zoom", System.Windows.Controls.Dock.Right));
        }

        [DoNotSetChanged]
        public double Zoom { get; set; } = 0.5;
    }

    public abstract class ToolViewModel : PaneViewModelBase
    {
        protected ToolViewModel(IUIServices uiServices)
            : base(uiServices)
        {
        }

        [DoNotSetChanged]
        public bool IsVisible { get; set; }
    }
}
