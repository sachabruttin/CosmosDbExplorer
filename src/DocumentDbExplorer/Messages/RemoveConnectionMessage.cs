using DocumentDbExplorer.Infrastructure.Models;

namespace DocumentDbExplorer.Messages
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
