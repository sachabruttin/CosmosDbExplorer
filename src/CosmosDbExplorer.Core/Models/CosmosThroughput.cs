using CosmosDbExplorer.Core.Contracts;

using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosThroughput : ICosmosResource
    {
        public CosmosThroughput(ThroughputResponse response)
        {
            ETag = response.ETag;
            SelfLink = response.Resource.SelfLink;
            IsReplacingPending = response.IsReplacePending.GetValueOrDefault();
            MinThroughtput = response.MinThroughput;
            AutoscaleMaxThroughput = response.Resource.AutoscaleMaxThroughput;
            Throughput = response.Resource.Throughput;
            RequestCharge = response.RequestCharge;
        }

        public string? Id { get; }
        public string? ETag { get; }
        public string? SelfLink { get; }
        public bool IsReplacingPending { get; }
        public int? MinThroughtput { get; }
        public int? AutoscaleMaxThroughput { get; }
        public int? Throughput { get; }
        public double RequestCharge { get; }
    }
}
