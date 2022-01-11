using CosmosDbExplorer.ViewModels;

namespace CosmosDbExplorer.Messages
{
    public class CloseDocumentMessage
    {
        public CloseDocumentMessage(string contentId)
        {
            ContentId = contentId;
        }

        public CloseDocumentMessage(PaneViewModelBase paneViewModel)
            : this(paneViewModel.ContentId)
        {
        }

        public string ContentId { get; set; }
    }
}
