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
        object? PartitionKey { get; }
        bool HasPartitionKey { get; }
    }
}
