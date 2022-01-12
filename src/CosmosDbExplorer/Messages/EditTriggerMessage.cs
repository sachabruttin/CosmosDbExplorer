using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class EditTriggerMessage : OpenTabMessageBase<TriggerNodeViewModel>
    {
        public EditTriggerMessage(TriggerNodeViewModel node, CosmosConnection connection, CosmosContainer container)
            : base(node, connection, container)
        {
        }
    }
}
