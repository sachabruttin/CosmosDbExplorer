using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class EditUserMessage : OpenTabMessageBase<UserNodeViewModel>
    {
        public EditUserMessage(UserNodeViewModel node, CosmosConnection connection, CosmosContainer container)
            : base(node, connection, container)
        {
        }
    }
}
