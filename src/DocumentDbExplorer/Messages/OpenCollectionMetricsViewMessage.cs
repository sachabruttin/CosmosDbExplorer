using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Messages
{

    public class OpenCollectionMetricsViewMessage : OpenTabMessageBase<CollectionMetricsNodeViewModel>
    {
        public OpenCollectionMetricsViewMessage(CollectionMetricsNodeViewModel node, Connection connection, DocumentCollection collection) 
            : base(node, connection, collection)
        {
        }
    }
}
