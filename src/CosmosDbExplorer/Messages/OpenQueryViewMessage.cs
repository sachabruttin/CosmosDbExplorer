using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class OpenQueryViewMessage : OpenTabMessageBase<ContainerNodeViewModel>
    {
        public OpenQueryViewMessage(ContainerNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database, CosmosContainer container, GenericQueryTypes queryType)
            : base(node, connection, database, container, queryType)
        {
        }
    }
}
