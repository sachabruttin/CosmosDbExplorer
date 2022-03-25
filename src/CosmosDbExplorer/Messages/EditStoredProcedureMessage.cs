using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class EditStoredProcedureMessage : OpenTabMessageBase<StoredProcedureNodeViewModel>
    {
        public EditStoredProcedureMessage(StoredProcedureNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database, CosmosContainer container)
            : base(node, connection, database, container)
        {
        }
    }
}
