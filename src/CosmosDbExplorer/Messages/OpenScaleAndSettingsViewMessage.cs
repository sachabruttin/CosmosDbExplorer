using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.Messages
{
    public class OpenScaleAndSettingsViewMessage : OpenTabMessageBase<ScaleSettingsNodeViewModel>
    {
        public OpenScaleAndSettingsViewMessage(ScaleSettingsNodeViewModel node, Connection connection, DocumentCollection collection) 
            : base(node, connection, collection)
        {
        }
    }
}
