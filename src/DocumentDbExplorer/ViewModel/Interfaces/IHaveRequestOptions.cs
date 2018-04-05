using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CosmosDbExplorer.ViewModel.Interfaces
{
    public interface IHaveRequestOptions
    {
        IndexingDirective? IndexingDirective { get; set; }
        ConsistencyLevel? ConsistencyLevel { get; set; }
        string PartitionKey { get; set; }
        AccessConditionType? AccessConditionType { get; set; }
        string AccessCondition { get; set; }
        string PreTrigger { get; set; }
        string PostTrigger { get; set; }
    }
}
