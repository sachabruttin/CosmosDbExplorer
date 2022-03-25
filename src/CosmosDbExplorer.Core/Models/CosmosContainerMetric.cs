using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosContainerMetric
    {
        public CosmosContainerMetric(ContainerResponse containerResponse)
        {
            var quota = Parse(containerResponse.Headers, "x-ms-resource-quota");
            var usage = Parse(containerResponse.Headers, "x-ms-resource-usage");

            RequestCharge = containerResponse.RequestCharge;
            //PartitionCount = containerResponse..Resource.PartitionKeyRangeStatistics.Count;
            DocumentsSizeQuota = quota["documentsSize"];
            DocumentsSizeUsage = usage["documentsSize"];
            DocumentsCountQuota = quota["documentsCount"];
            DocumentsCountUsage = usage["documentsCount"];
            CollectionSizeQuota = quota["collectionSize"];
            CollectionSizeUsage = usage["collectionSize"];
            StoredProceduresQuota = quota["storedProcedures"];
            StoredProceduresUsage = usage["storedProcedures"];
            TriggersQuota = quota["triggers"];
            TriggersUsage = usage["triggers"];
            UserDefinedFunctionsQuota = quota["functions"];
            UserDefinedFunctionsUsage = usage["functions"];
        }

        public double RequestCharge { get; }
        public int PartitionCount { get; }
        public long DocumentsCountQuota { get; }
        public long DocumentsCountUsage { get; }
        public long DocumentsSizeQuota { get; }
        public long DocumentsSizeUsage { get; }
        //public List<PartitionKeyRangeStatistics> PartitionMetrics { get; }
        public long CollectionSizeQuota { get; }
        public long CollectionSizeUsage { get; }
        public long StoredProceduresQuota { get; }
        public long StoredProceduresUsage { get; }
        public long TriggersQuota { get; }
        public long TriggersUsage { get; }
        public long UserDefinedFunctionsQuota { get; }
        public long UserDefinedFunctionsUsage { get; }
        public bool HasPartitionKey { get; }

        private Dictionary<string, long> Parse(Headers headers, string headerName)
        {
            return headers[headerName]
                .Split(';')
                .Select(item => new { Key = item.Split('=')[0], Value = item.Split('=')[1] })
                .ToDictionary(d => d.Key, d => long.Parse(d.Value));
        }
    }
}
