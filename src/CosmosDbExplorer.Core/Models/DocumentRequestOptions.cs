using System;
using System.ComponentModel;
using CosmosDbExplorer.Core.Contracts;

namespace CosmosDbExplorer.Core.Models
{
    public class DocumentRequestOptions : IDocumentRequestOptions
    {
        public CosmosIndexingDirectives? IndexingDirective { get; set; }
        public CosmosConsistencyLevels? ConsistencyLevel { get; set; }
        public CosmosAccessConditionType AccessCondition { get; set; }
        public string? ETag { get; set; }
        public string[] PreTriggers { get; set; } = Array.Empty<string>();
        public string[] PostTriggers { get; set; } = Array.Empty<string>();
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CosmosIndexingDirectives
    {
        Default = 0,
        Include = 1,
        Exclude = 2
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CosmosConsistencyLevels
    {
        Strong = 0,
        [Description("Bounded Staleness")]
        BoundedStaleness = 1,
        Session = 2,
        Eventual = 3,
        [Description("Consistent Prefix")]
        ConsistentPrefix = 4
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CosmosAccessConditionType
    {
        None = 0,
        [Description("If Match")]
        IfMatch = 1,
        [Description("If Not Match")]
        IfNotMatch = 2
    }
}
