using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Messages
{
    public class EditTriggerMessage
    {
        public EditTriggerMessage(CollectionNodeViewModel collectionNode, TriggerNodeViewModel node)
        {
            Node = node;
            Connection = collectionNode.Parent.Parent.Connection;
            Collection = collectionNode.Collection;
        }

        public TriggerNodeViewModel Node { get; }

        public Connection Connection { get; }

        public DocumentCollection Collection { get; set; }
    }
}
