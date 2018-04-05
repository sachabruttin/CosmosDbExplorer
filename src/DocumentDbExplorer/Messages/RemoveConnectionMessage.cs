using CosmosDbExplorer.Infrastructure.Models;

namespace CosmosDbExplorer.Messages
{
    public class RemoveConnectionMessage
    {
        public RemoveConnectionMessage(Connection connection)
        {
            Connection = connection;
        }

        public Connection Connection { get; }
    }
}
