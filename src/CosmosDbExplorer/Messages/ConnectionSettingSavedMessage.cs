using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Messages
{
    public class ConnectionSettingSavedMessage
    {
        public ConnectionSettingSavedMessage(CosmosConnection connection)
        {
            Connection = connection;
        }

        public CosmosConnection Connection { get; }
    }
}
