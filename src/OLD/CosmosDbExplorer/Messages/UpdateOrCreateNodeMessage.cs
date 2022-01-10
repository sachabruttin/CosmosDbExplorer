using CosmosDbExplorer.Infrastructure.Models;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.Messages
{

    public class UpdateOrCreateNodeMessage<T>
        where T: Resource
    {
        public UpdateOrCreateNodeMessage(T resource, DocumentCollection collection, string oldAltLink)
        {
            Resource = resource;
            OldAltLink = oldAltLink;
            Collection = collection;
        }

        public T Resource { get; }

        public bool IsNewResource => string.IsNullOrEmpty(OldAltLink);

        public string OldAltLink { get; }

        public DocumentCollection Collection { get; }
    }
}
