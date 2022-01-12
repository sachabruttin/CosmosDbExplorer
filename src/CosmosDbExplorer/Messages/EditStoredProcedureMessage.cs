using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class EditStoredProcedureMessage : OpenTabMessageBase<StoredProcedureNodeViewModel>
    {
        public EditStoredProcedureMessage(StoredProcedureNodeViewModel node, CosmosConnection connection, CosmosContainer container) 
            : base(node, connection, container)
        {
        }
    }
}
