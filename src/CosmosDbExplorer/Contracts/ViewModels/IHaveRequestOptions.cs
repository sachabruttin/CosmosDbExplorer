using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface IHaveRequestOptions
    {
        CosmosIndexingDirectives? IndexingDirective { get; set; }
        CosmosConsistencyLevels? ConsistencyLevel { get; set; }
        //string? PartitionKeyValue { get; set; }
        CosmosAccessConditionType AccessConditionType { get; set; }
        string? AccessCondition { get; set; }
        string? PreTrigger { get; set; }
        string? PostTrigger { get; set; }
    }
}
