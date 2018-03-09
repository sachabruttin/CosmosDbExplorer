using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Messages
{
    public class OpenDocumentsViewMessage : OpenTabMessageBase<DocumentNodeViewModel>
    {
        public OpenDocumentsViewMessage(DocumentNodeViewModel node, Connection connection, DocumentCollection collection) 
            : base(node, connection, collection)
        {
        }
    }
}
