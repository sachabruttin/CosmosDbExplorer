using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.Messages
{
    public class EditStoredProcedureMessage : OpenTabMessageBase<StoredProcedureNodeViewModel>
    {
        public EditStoredProcedureMessage(StoredProcedureNodeViewModel node, Connection connection, DocumentCollection collection) 
            : base(node, connection, collection)
        {
        }
    }
}
