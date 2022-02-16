using CosmosDbExplorer.Core.Contracts;

namespace CosmosDbExplorer.Messages
{
    public class UpdateOrCreateNodeMessage<TResource, TParent>
        where TResource : ICosmosResource
    {
        public UpdateOrCreateNodeMessage(TResource resource, TParent container, string? oldAltLink)
        {
            Resource = resource;
            OldAltLink = oldAltLink;
            Parent = container;
        }

        public TResource Resource { get; }

        public bool IsNewResource => string.IsNullOrEmpty(OldAltLink);

        public string? OldAltLink { get; }

        public TParent Parent { get; }
    }
}
