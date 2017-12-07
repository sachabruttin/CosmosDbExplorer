using DocumentDbExplorer.Infrastructure.Models;

namespace DocumentDbExplorer.Messages
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
