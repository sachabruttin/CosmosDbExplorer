using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class OpenImportDocumentViewMessage : OpenTabMessageBase<ContainerNodeViewModel>
    {
        public OpenImportDocumentViewMessage(ContainerNodeViewModel node, CosmosConnection connection, CosmosContainer container)
            : base(node, connection, container)
        {
        }
    }
}
