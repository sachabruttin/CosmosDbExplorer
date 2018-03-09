using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Messages
{
    public abstract class OpenTabMessageBase<TNodeViewModel>
    {
        protected OpenTabMessageBase(TNodeViewModel node, Connection connection, DocumentCollection collection)
        {
            Node = node;
            Connection = connection;
            Collection = collection;
        }

        public TNodeViewModel Node { get; }

        public Connection Connection { get; }

        public DocumentCollection Collection { get; }
    }

    public class EditTriggerMessage : OpenTabMessageBase<TriggerNodeViewModel>
    {
        public EditTriggerMessage(TriggerNodeViewModel node, Connection connection, DocumentCollection collection)
            : base(node, connection, collection)
        {
        }
    }

    public class EditUserMessage : OpenTabMessageBase<UserNodeViewModel>
    {
        public EditUserMessage(UserNodeViewModel node, Connection connection, DocumentCollection collection)
            : base(node, connection, collection)
        {
        }
    }

    public class EditPermissionMessage : OpenTabMessageBase<PermissionNodeViewModel>
    {
        public EditPermissionMessage(PermissionNodeViewModel node, Connection connection, DocumentCollection collection)
            : base(node, connection, collection)
        {
        }
    }
}
