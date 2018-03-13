using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Messages
{
    public class OpenQueryViewMessage : OpenTabMessageBase<CollectionNodeViewModel>
    {
        public OpenQueryViewMessage(CollectionNodeViewModel node, Connection connection, DocumentCollection collection) 
            : base(node, connection, collection)
        {
        }
    }
}
