using CosmosDbExplorer.Infrastructure.Models;

namespace CosmosDbExplorer.Messages
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
