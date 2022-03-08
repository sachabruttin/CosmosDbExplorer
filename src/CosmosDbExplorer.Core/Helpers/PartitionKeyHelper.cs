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

        public static PartitionKey? Parse(string? partitionKey)
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                return null;
            }

            if (bool.TryParse(partitionKey, out var boolResult))
            {
                return new PartitionKey(boolResult);
            }

            if (double.TryParse(partitionKey, out var doubleResult))
            {
                return new PartitionKey(doubleResult);
            }

            if (string.Compare(partitionKey, "null", true) == 0)
            {
                return PartitionKey.Null;
            }

            return new PartitionKey(partitionKey);
        }
    }
}
