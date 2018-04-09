using CosmosDbExplorer.Infrastructure.Models;

namespace CosmosDbExplorer.Messages
{
    class ConnectionSettingSavedMessage
    {
        public ConnectionSettingSavedMessage(Connection connection)
        {
            Connection = connection;
        }

        public Connection Connection { get; }
    }
}
