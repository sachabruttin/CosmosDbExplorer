using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Core.Contracts
{
    public interface IDocumentRequestOptions
    {
        CosmosIndexingDirectives? IndexingDirective { get; set; }
        CosmosConsistencyLevels? ConsistencyLevel { get; set; }
        CosmosAccessConditionType AccessCondition { get; set; }
        string? ETag { get; set; }
        string[] PreTriggers { get; set; } 
        string[] PostTriggers { get; set; }
    }
}
