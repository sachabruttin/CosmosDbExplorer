using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Helpers
{
    public class PartitionKeyHelper
    {
        public static PartitionKey Get(object? partitionKey)
        {
            return partitionKey switch
            {
                null => PartitionKey.Null,
                string s => new PartitionKey(s),
                double d => new PartitionKey(d),
                float f => new PartitionKey((double)f),
                int i => new PartitionKey((double)i),
                long l => new PartitionKey((double)l),
                bool b => new PartitionKey(b),
                _ => throw new ArgumentException("Partition Key type not supported")
            };
        }
    }
}
