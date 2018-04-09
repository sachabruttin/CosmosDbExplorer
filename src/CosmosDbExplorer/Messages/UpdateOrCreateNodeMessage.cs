using CosmosDbExplorer.Infrastructure.Models;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.Messages
{

    public class UpdateOrCreateNodeMessage<T>
        where T: Resource
    {
        public UpdateOrCreateNodeMessage(T resource, string oldAltLink)
        {
            Resource = resource;
            OldAltLink = oldAltLink;
        }

        public T Resource { get; }

        public bool IsNewResource => string.IsNullOrEmpty(OldAltLink);

        public string OldAltLink { get; }
    }
}
