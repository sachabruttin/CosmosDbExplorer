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

    public class EditUserMessage
    {
        public EditUserMessage(UserNodeViewModel node)
        {
            Node = node;
            Connection = node.Parent.Parent.Parent.Connection;
        }

        public UserNodeViewModel Node { get; }

        public Connection Connection { get; }
    }

    public class EditPermissionMessage
    {
        public EditPermissionMessage(PermissionNodeViewModel node)
        {
            Node = node;
        }

        public PermissionNodeViewModel Node { get; }
    }
}
