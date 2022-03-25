using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class EditUserDefFuncMessage : OpenTabMessageBase<UserDefFuncNodeViewModel>
    {
        public EditUserDefFuncMessage(UserDefFuncNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database, CosmosContainer container)
            : base(node, connection, database, container)
        {
        }
    }
}
