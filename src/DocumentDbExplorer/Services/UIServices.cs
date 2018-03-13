using DocumentDbExplorer.Messages;
using GalaSoft.MvvmLight.Messaging;

namespace DocumentDbExplorer.Services
{
    public class UIServices : IUIServices
    {
        private readonly IMessenger _messenger;

        public UIServices(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public void SetBusyState(bool isBusy)
        {
            _messenger.Send(new IsBusyMessage(isBusy));
        }
    }
}
