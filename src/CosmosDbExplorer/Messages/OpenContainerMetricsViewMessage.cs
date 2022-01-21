using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class OpenContainerMetricsViewMessage : OpenTabMessageBase<MetricsNodeViewModel>
    {
        public OpenContainerMetricsViewMessage(MetricsNodeViewModel node, CosmosConnection connection, CosmosContainer container)
            : base(node, connection, container)
        {
        }
    }
}
