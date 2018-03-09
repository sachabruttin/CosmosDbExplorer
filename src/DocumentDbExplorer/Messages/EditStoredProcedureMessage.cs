using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Messages
{
    public class EditStoredProcedureMessage : OpenTabMessageBase<StoredProcedureNodeViewModel>
    {
        public EditStoredProcedureMessage(StoredProcedureNodeViewModel node, Connection connection, DocumentCollection collection) 
            : base(node, connection, collection)
        {
        }
    }
}
