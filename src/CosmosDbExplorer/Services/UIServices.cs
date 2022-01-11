
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Messages;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.Services
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
