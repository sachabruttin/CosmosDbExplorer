using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.Messages
{
    public class OpenDatabaseScaleViewMessage : OpenTabMessageBase<DatabaseScaleNodeViewModel>
    {
        public OpenDatabaseScaleViewMessage(DatabaseScaleNodeViewModel node, Connection connection, DocumentCollection collection) 
            : base(node, connection, collection)
        {
        }
    }
}
