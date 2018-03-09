using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Messages
{
    public class OpenScaleAndSettingsViewMessage : OpenTabMessageBase<ScaleSettingsNodeViewModel>
    {
        public OpenScaleAndSettingsViewMessage(ScaleSettingsNodeViewModel node, Connection connection, DocumentCollection collection) 
            : base(node, connection, collection)
        {
        }
    }
}
