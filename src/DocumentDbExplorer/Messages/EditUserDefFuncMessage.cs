using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Messages
{
    public class EditUserDefFuncMessage
    {
        public EditUserDefFuncMessage(CollectionNodeViewModel collectionNode, UserDefFuncNodeViewModel node)
        {
            Node = node;
            Connection = collectionNode.Parent.Parent.Connection;
            Collection = collectionNode.Collection;
        }

        public UserDefFuncNodeViewModel Node { get; }

        public Connection Connection { get; }

        public DocumentCollection Collection { get; set; }
    }
}
