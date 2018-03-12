using System.Collections.ObjectModel;
using System.Windows.Media;
using DocumentDbExplorer.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Infrastructure.Models
{
    public abstract class PaneViewModelBase: ViewModelBase
    {
        private RelayCommand _closeCommand;
        private readonly StatusBarItem _pathStatusBarItem;

        protected PaneViewModelBase(IMessenger messenger) : base(messenger)
        {
            _pathStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = ToolTip, IsVisible = true }, StatusBarItemType.SimpleText, "Path", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_pathStatusBarItem);
        }

        public string Title { get; set; }

        public string ToolTip { get; set; }

        public virtual void OnToolTipChanged()
        {
            _pathStatusBarItem.DataContext.Value = ToolTip;
        }

        public string Header { get; set; }

        public string ContentId { get; protected set; }

        public bool IsSelected { get; set; }

        public bool IsActive { get; set; }

        public virtual void OnIsActiveChanged()
        {
            DispatcherHelper.RunAsync(() => MessengerInstance.Send(new ActivePaneChangedMessage(this)));
        }

        public ObservableCollection<StatusBarItem> StatusBarItems { get; protected set; } = new ObservableCollection<StatusBarItem>();

        public object IconSource { get; set; }

        public Color? AccentColor { get; set; }

        public RelayCommand CloseCommand
        {
            get
            {
                return _closeCommand
                    ?? (_closeCommand = new RelayCommand(OnClose, CanClose));
            }
        }

        protected virtual bool CanClose()
        {
            return true;
        }

        protected virtual void OnClose()
        {
            MessengerInstance.Send(new CloseDocumentMessage(this));
        }
    }

    public abstract class PaneViewModel<TNodeViewModel> : PaneViewModelBase
        where TNodeViewModel : TreeViewItemViewModel
    {
        protected PaneViewModel(IMessenger messenger)
            : base(messenger)
        {

        }

        public abstract void Load(string contentId, TNodeViewModel node, Connection connection, DocumentCollection collection);
    }

    public abstract class PaneWithZoomViewModel<TNodeViewModel> : PaneViewModel<TNodeViewModel>
        where TNodeViewModel : TreeViewItemViewModel
    {
        protected PaneWithZoomViewModel(IMessenger messenger) : base(messenger)
        {
            StatusBarItems.Add(new StatusBarItem(new StatusBarItemContext { Value = this, IsVisible = true }, StatusBarItemType.Zoom, "Zoom", System.Windows.Controls.Dock.Right));
        }

        public double Zoom { get; set; } = 0.5;
    }

    public abstract class ToolViewModel : PaneViewModelBase
    {
        protected ToolViewModel(IMessenger messenger) : base(messenger)
        {
        }

        public bool IsVisible { get; set; }
    }
}
