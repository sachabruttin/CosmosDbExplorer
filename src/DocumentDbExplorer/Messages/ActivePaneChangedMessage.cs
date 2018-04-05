using GalaSoft.MvvmLight;

namespace CosmosDbExplorer.Messages
{
    public class ActivePaneChangedMessage
    {
        public ActivePaneChangedMessage(ViewModelBase paneViewModel)
        {
            PaneViewModel = paneViewModel;
        }

        public ViewModelBase PaneViewModel { get; }
    }
}
