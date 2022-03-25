using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Messages
{
    public abstract class OpenTabMessageBase<TNodeViewModel>
    {
        protected OpenTabMessageBase(TNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database, CosmosContainer? container)
        {
            Node = node;
            Connection = connection;
            Database = database;
            Container = container;
        }

        public TNodeViewModel? Node { get; }

        public CosmosConnection? Connection { get; }

        public CosmosDatabase? Database { get; }

        public CosmosContainer? Container { get; }
    }
}
