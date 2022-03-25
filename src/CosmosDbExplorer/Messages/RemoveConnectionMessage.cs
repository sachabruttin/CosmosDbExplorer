using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Messages
{
    public class RemoveConnectionMessage
    {
        public RemoveConnectionMessage(CosmosConnection connection)
        {
            Connection = connection;
        }

        public CosmosConnection Connection { get; }
    }
}
