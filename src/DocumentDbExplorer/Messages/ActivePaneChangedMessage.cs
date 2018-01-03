using DocumentDbExplorer.Infrastructure.Models;

namespace DocumentDbExplorer.Messages
{
    public class ActivePaneChangedMessage
    {
        public ActivePaneChangedMessage(PaneViewModel paneViewModel)
        {
            PaneViewModel = paneViewModel;
        }

        public PaneViewModel PaneViewModel { get; }
    }
}
