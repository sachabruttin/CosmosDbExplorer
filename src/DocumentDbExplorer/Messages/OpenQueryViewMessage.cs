using DocumentDbExplorer.ViewModel;

namespace DocumentDbExplorer.Messages
{
    public class OpenQueryViewMessage
    {
        public OpenQueryViewMessage(CollectionNodeViewModel node)
        {
            Node = node;
        }

        public CollectionNodeViewModel Node { get; }
    }
}
