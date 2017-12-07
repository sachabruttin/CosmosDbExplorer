using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Messages
{
    public class EditStoredProcedureMessage
    {
        public EditStoredProcedureMessage(CollectionNodeViewModel collectionNode, StoredProcedureNodeViewModel node)
        {
            Node = node;
            Connection = collectionNode.Parent.Parent.Connection;
            Collection = collectionNode.Collection;
        }

        public StoredProcedureNodeViewModel Node { get; }

        public Connection Connection { get; }

        public DocumentCollection Collection { get; set; }
    }
}
