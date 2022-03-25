
using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Models
{
    public class NodeContext
    {
        public NodeContext(CosmosConnection connection)
        {
            Connection = connection;
        }

        public NodeContext(NodeContext context, CosmosDatabase database)
            : this(context.Connection)
        {
            Database = database;
        }

        public NodeContext(NodeContext context, CosmosContainer container)
            : this(context, context.Database)
        {
            Container = container;
        }

        public CosmosConnection Connection { get; }
        public CosmosDatabase? Database { get; }
        public CosmosContainer? Container { get; }
        public object? Data { get; set; }
    }
}
