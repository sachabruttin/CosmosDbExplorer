using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class OpenDocumentsViewMessage : OpenTabMessageBase<DocumentNodeViewModel>
    {
        public OpenDocumentsViewMessage(DocumentNodeViewModel node, CosmosConnection connection, CosmosContainer container)
            : base(node, connection, container)
        {
        }
    }
}
