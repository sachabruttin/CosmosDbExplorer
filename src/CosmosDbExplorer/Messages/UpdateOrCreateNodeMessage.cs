using CosmosDbExplorer.Core.Contracts;
using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Messages
{

    public class UpdateOrCreateNodeMessage<T>
        where T: ICosmosResource
    {
        public UpdateOrCreateNodeMessage(T resource, CosmosContainer container, string oldAltLink)
        {
            Resource = resource;
            OldAltLink = oldAltLink;
            Container = container;
        }

        public T Resource { get; }

        public bool IsNewResource => string.IsNullOrEmpty(OldAltLink);

        public string OldAltLink { get; }

        public CosmosContainer Container { get; }
    }
}
