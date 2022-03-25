using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class OpenDocumentsViewMessage : OpenTabMessageBase<DocumentNodeViewModel>
    {
        public OpenDocumentsViewMessage(DocumentNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database, CosmosContainer container)
            : base(node, connection, database, container)
        {
        }
    }
}
