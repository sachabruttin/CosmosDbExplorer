using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Helpers
{
    public class PartitionKeyHelper
    {
        public static PartitionKey Build(IList<string> partitionKeyPath, object? pk0, object? pk1, object? pk2)
        {
            var builder = new PartitionKeyBuilder();

            // bool, double, string
            switch (partitionKeyPath.Count)
            {
                case 0: 
                    builder.AddNoneType();
                    break;
                case 1:
                    builder.AddObject(pk0);
                    break;
                case 2:
                    builder.AddObject(pk0);
                    builder.AddObject(pk1);
                    break;
                case 3:
                    builder.AddObject(pk0);
                    builder.AddObject(pk1);
                    builder.AddObject(pk2);
                    break;
            }
           
            return builder.Build();
        }

        public static PartitionKey Get(object? partitionKey)
        {
            // Replace this method when supporting Hierachical PartitionKey on Query parameters
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

    public static class PartitionKeyBuilderExtensions
    {
        public static PartitionKeyBuilder AddObject(this PartitionKeyBuilder builder, object? partitionKey)
        {
            return partitionKey switch
            {
                string s => builder.Add(s),
                double d => builder.Add(d),
                float f => builder.Add((double)f),
                int i => builder.Add(i),
                long l => builder.Add(l),
                bool b => builder.Add(b),
                null => builder.AddNullValue(),
                _ => throw new ArgumentException("Partition Key type not supported"),
            };
        }
    }

}
