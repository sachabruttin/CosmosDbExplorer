using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosResult
    {
        public double RequestCharge { get; internal set; }
        public TimeSpan TimeElapsed { get; internal set; }
    }

    public class CosmosQueryResult<T> : CosmosResult
    {
        public T Items { get; internal set; }
        public Exception? Error { get; internal set; }
        public string? ContinuationToken { get; internal set; }
        public IEnumerable<string> Warnings { get; internal set; } = Array.Empty<string>();
        public Dictionary<string, string> Headers { get; internal set; } = new Dictionary<string, string>();
        public bool HasMore => !string.IsNullOrEmpty(ContinuationToken);
        public JObject? Diagnostics { get; internal set; }
        public string? IndexMetrics { get; internal set; }
    }
}
