using DocumentDbExplorer.Infrastructure.Models;

namespace DocumentDbExplorer.Messages
{
    public class CloseDocumentMessage
    {
        public CloseDocumentMessage(PaneViewModel paneViewModel)
        {
            Paneviewmodel = paneViewModel;
        }

        public PaneViewModel Paneviewmodel { get; }
    }
}
