using DocumentDbExplorer.ViewModel;

namespace DocumentDbExplorer.Messages
{
    public class OpenDocumentsViewMessage
    {
        public OpenDocumentsViewMessage(DocumentNodeViewModel node)
        {
            Node = node;
        }

        public DocumentNodeViewModel Node { get; }
    }
}
