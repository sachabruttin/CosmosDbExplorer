using DocumentDbExplorer.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

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

        public string ContentId { get; set; }

        public bool IsSelected { get; set; }

        public bool IsActive { get; set; }

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

    public class ToolViewModel : PaneViewModel
    {
        public ToolViewModel(IMessenger messenger) : base(messenger)
        {
        }

        public string Name { get; set; }

        public bool IsVisible { get; set; }
    }
}
