using DocumentDbExplorer.ViewModel;

namespace DocumentDbExplorer.Messages
{
    public class OpenImportDocumentViewMessage
    {
        public OpenImportDocumentViewMessage(CollectionNodeViewModel node)
        {
            Node = node;
        }

        public CollectionNodeViewModel Node { get; }
    }
}
