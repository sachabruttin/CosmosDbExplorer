using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class EditPermissionMessage : OpenTabMessageBase<PermissionNodeViewModel>
    {
        public EditPermissionMessage(PermissionNodeViewModel node, CosmosConnection connection, CosmosDatabase database)
            : base(node, connection, database, null)
        {
        }
    }
}
