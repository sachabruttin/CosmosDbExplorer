
using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Models
{
    public class NodeContext<TNodeViewModel>
    {
        public NodeContext(TNodeViewModel? node, CosmosConnection? connection, object? data = null)
        {
            Node = node;
            Connection = connection;
            Data = data;
        }

        public NodeContext(TNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database, object? data = null)
            : this(node, connection, data)
        {
            Database = database;
        }
        public NodeContext(TNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database, CosmosContainer? container, object? data = null)
            : this(node, connection, database, data)
        {
            Container = container;
        }

        public TNodeViewModel? Node { get;  }
        public CosmosConnection? Connection { get; }
        public CosmosDatabase? Database { get; }
        public CosmosContainer? Container { get; }
        public object? Data { get; }
    }
}
