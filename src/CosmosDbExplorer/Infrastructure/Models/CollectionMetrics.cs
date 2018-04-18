using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CosmosDbExplorer.Infrastructure.Models
{
    public class CollectionMetric
    {
        public CollectionMetric(ResourceResponse<DocumentCollection> documentCollection)
        {
            var quotaUsage = documentCollection.CurrentResourceQuotaUsage
                        .Split(';')
                        .Select(item => new { Key = item.Split('=')[0], Value = item.Split('=')[1] })
                        .ToDictionary(d => d.Key, d => d.Value);

            PartitionCount = documentCollection.Resource.PartitionKeyRangeStatistics.Count;
            DocumentSize = long.Parse(quotaUsage["documentsSize"]);
            DocumentCount = long.Parse(quotaUsage["documentsCount"]);
            PartitionMetrics = documentCollection.Resource.PartitionKeyRangeStatistics.ToList();
            CollectionSizeQuota = documentCollection.CollectionSizeQuota;
            CollectionSizeUsage = documentCollection.CollectionSizeUsage;
            StoredProceduresQuota = documentCollection.StoredProceduresQuota;
            StoredProceduresUsage = documentCollection.StoredProceduresUsage;
            TriggersQuota = documentCollection.TriggersQuota;
            TriggersUsage = documentCollection.TriggersUsage;
            UserDefinedFunctionsQuota = documentCollection.UserDefinedFunctionsQuota;
            UserDefinedFunctionsUsage = documentCollection.UserDefinedFunctionsUsage;
        }

        public int PartitionCount { get; }
        public long DocumentCount { get; }
        public long DocumentSize { get; }
        public List<PartitionKeyRangeStatistics> PartitionMetrics { get; }
        public long CollectionSizeQuota { get; }
        public long CollectionSizeUsage { get; }
        public long StoredProceduresQuota { get; }
        public long StoredProceduresUsage { get; }
        public long TriggersQuota { get; }
        public long TriggersUsage { get; }
        public long UserDefinedFunctionsQuota { get; }
        public long UserDefinedFunctionsUsage { get; }
        public bool HasPartitionKey { get; }
    }
}
