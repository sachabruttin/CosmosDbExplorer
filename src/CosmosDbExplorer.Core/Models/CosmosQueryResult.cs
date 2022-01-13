using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosQueryResult
    {
        public IReadOnlyList<CosmosDocument> Items { get; internal set; } = Array.Empty<CosmosDocument>();
        public Exception? Error { get; internal set; }
        public TimeSpan TimeElapsed { get; internal set; }
        public double RequestCharge { get; internal set; }
        public string? ContinuationToken { get; internal set; }
        public IEnumerable<string> Warnings { get; internal set; } = Array.Empty<string>();
    }
}
