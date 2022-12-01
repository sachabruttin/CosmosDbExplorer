using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Models;

using Microsoft.Extensions.DependencyInjection;

namespace CosmosDbExplorer.Messages
{
    public abstract class OpenTabMessageBase<TNodeViewModel>
    {
        protected OpenTabMessageBase(TNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database, CosmosContainer? container, object? data = null)
        {
            Context = new NodeContext<TNodeViewModel>(node, connection, database, container, data);
        }

        public NodeContext<TNodeViewModel> Context { get; }
    }
}
