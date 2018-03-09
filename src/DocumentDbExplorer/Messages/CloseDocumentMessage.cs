using DocumentDbExplorer.Infrastructure.Models;

namespace DocumentDbExplorer.Messages
{
    public class CloseDocumentMessage
    {
        public CloseDocumentMessage(PaneViewModelBase paneViewModel)
        {
            PaneViewModel = paneViewModel;
        }

        public PaneViewModelBase PaneViewModel { get; }
    }
}
