using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.Messages
{

    public class OpenCollectionMetricsViewMessage : OpenTabMessageBase<CollectionMetricsNodeViewModel>
    {
        public OpenCollectionMetricsViewMessage(CollectionMetricsNodeViewModel node, Connection connection, DocumentCollection collection) 
            : base(node, connection, collection)
        {
        }
    }
}
