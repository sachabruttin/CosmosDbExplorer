using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class EditTriggerMessage : OpenTabMessageBase<TriggerNodeViewModel>
    {
        public EditTriggerMessage(TriggerNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database, CosmosContainer container)
            : base(node, connection, database, container)
        {
        }
    }
}
