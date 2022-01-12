using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Messages
{
    public abstract class OpenTabMessageBase<TNodeViewModel>
    {
        protected OpenTabMessageBase(TNodeViewModel node, CosmosConnection connection, CosmosContainer container)
        {
            Node = node;
            Connection = connection;
            Container = container;
        }

        public TNodeViewModel Node { get; }

        public CosmosConnection Connection { get; }

        public CosmosContainer Container { get; }
    }
}
