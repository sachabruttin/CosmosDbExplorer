using CommunityToolkit.Mvvm.ComponentModel;

namespace CosmosDbExplorer.Messages
{
    public class ActivePaneChangedMessage
    {
        public ActivePaneChangedMessage(ObservableRecipient paneViewModel)
        {
            PaneViewModel = paneViewModel;
        }

        public ObservableRecipient PaneViewModel { get; }
    }
}
