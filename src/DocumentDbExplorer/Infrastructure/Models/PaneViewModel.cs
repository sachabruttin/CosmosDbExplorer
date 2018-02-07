﻿using System.Collections.ObjectModel;
using System.Windows.Media;
using DocumentDbExplorer.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;

namespace DocumentDbExplorer.Infrastructure.Models
{
    public class PaneViewModel : ViewModelBase
    {
        private RelayCommand _closeCommand;

        public PaneViewModel(IMessenger messenger) : base(messenger)
        {
        }

        public string Title { get; set; }

        public string ToolTip { get; set; }

        public string Header { get; set; }

        public string ContentId { get; set; }

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
                    ?? (_closeCommand = new RelayCommand(x => OnClose(), x => CanClose()));
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

    public class PaneWithZoomViewModel : PaneViewModel
    {
        public PaneWithZoomViewModel(IMessenger messenger) : base(messenger)
        {
            StatusBarItems.Add(new StatusBarItem(new StatusBarItemContext { Value = this, IsVisible = true }, StatusBarItemType.Zoom, null, System.Windows.Controls.Dock.Right));
        }

        public double Zoom { get; set; } = 0.5;
    }

    public class ToolViewModel : PaneViewModel
    {
        public ToolViewModel(IMessenger messenger) : base(messenger)
        {
        }

        public bool IsVisible { get; set; }
    }
}
