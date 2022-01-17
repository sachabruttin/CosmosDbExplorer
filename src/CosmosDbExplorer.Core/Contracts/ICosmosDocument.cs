using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Core.Contracts
{
    public interface ICosmosDocument : ICosmosResource
    {  
        string Attachments { get; }
        long TimeStamp { get; }
        string? PartitionKey { get; }
        bool HasPartitionKey { get; }
    }

    public interface ICosmosQuery
    {
        string QueryText { get; }
        int MaxItemCount { get; }
        string? ContinuationToken { get; }
    }
}
