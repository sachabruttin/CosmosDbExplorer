using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class OpenMetricsViewMessage : OpenTabMessageBase<MetricsNodeViewModel>
    {
        public OpenMetricsViewMessage(MetricsNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database, CosmosContainer? container)
            : base(node, connection, database, container)
        {
        }
    }
}
