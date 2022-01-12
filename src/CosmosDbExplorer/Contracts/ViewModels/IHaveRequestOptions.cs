namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface IHaveRequestOptions
    {
        //IndexingDirective? IndexingDirective { get; set; }
        //ConsistencyLevel? ConsistencyLevel { get; set; }
        string PartitionKeyValue { get; set; }
        //AccessConditionType? AccessConditionType { get; set; }
        string AccessCondition { get; set; }
        string PreTrigger { get; set; }
        string PostTrigger { get; set; }
    }
}
