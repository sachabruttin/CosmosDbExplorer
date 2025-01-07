using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Core.Contracts
{
    public interface ICosmosDocument : ICosmosResource, IEquatable<ICosmosDocument?>
    {
        string? Attachments { get; }
        long TimeStamp { get; }
        object? PartitionKey0 { get; }
        object? PartitionKey1 { get; }
        object? PartitionKey2 { get; }
        bool HasPartitionKey { get; }
    }
}
