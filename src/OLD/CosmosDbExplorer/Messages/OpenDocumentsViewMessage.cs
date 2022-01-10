using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.Messages
{
    public class OpenDocumentsViewMessage : OpenTabMessageBase<DocumentNodeViewModel>
    {
        public OpenDocumentsViewMessage(DocumentNodeViewModel node, Connection connection, DocumentCollection collection) 
            : base(node, connection, collection)
        {
        }
    }
}
