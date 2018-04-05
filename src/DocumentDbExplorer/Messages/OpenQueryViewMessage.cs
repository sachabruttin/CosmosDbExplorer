using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.Messages
{
    public class OpenQueryViewMessage : OpenTabMessageBase<CollectionNodeViewModel>
    {
        public OpenQueryViewMessage(CollectionNodeViewModel node, Connection connection, DocumentCollection collection) 
            : base(node, connection, collection)
        {
        }
    }
}
