using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class EditUserMessage : OpenTabMessageBase<UserNodeViewModel>
    {
        public EditUserMessage(UserNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database)
            : base(node, connection, database, null)
        {
        }
    }
}
