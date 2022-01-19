namespace CosmosDbExplorer.Core.Contracts
{
    public interface ICosmosScript : ICosmosResource
    {
        string Body { get; }
    }
}
