using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class OpenScaleAndSettingsViewMessage : OpenTabMessageBase<ScaleSettingsNodeViewModel>
    {
        public OpenScaleAndSettingsViewMessage(ScaleSettingsNodeViewModel node, CosmosConnection connection, CosmosContainer container)
        : base(node, connection, container)
        {
        }
    }
}
