namespace CosmosDbExplorer.Core.Contracts
{
    public interface ICosmosResource
    {
        string Id { get; }
        string ETag { get; }
        string SelfLink { get; }
    }
}
