using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class OpenContainerMetricsViewMessage : OpenTabMessageBase<MetricsNodeViewModel>
    {
        public OpenContainerMetricsViewMessage(MetricsNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database, CosmosContainer container)
            : base(node, connection, database, container)
        {
        }
    }
}
