using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Messages
{
    public class OpenImportDocumentViewMessage : OpenTabMessageBase<CollectionNodeViewModel>
    {
        public OpenImportDocumentViewMessage(CollectionNodeViewModel node, Connection connection, DocumentCollection collection) 
            : base(node, connection, collection)
        {
        }
    }
}
