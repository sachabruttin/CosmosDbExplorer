using System;
using System.Collections.Generic;
using System.Linq;

using CosmosDbExplorer.Core.Helpers;

using Microsoft.Azure.Cosmos.Scripts;

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
        public T? Items { get; internal set; }
        public Exception? Error { get; internal set; }
        public string? ContinuationToken { get; internal set; }
        public IEnumerable<string> Warnings { get; internal set; } = Array.Empty<string>();
        public Dictionary<string, string> Headers { get; internal set; } = new Dictionary<string, string>();
        public bool HasMore => !string.IsNullOrEmpty(ContinuationToken);
        public JObject? Diagnostics { get; internal set; }
        public string? IndexMetrics { get; internal set; }
    }

    public class CosmosStoredProcedureResult : CosmosResult
    {

        public CosmosStoredProcedureResult(StoredProcedureExecuteResponse<string> response)
        {
            RequestCharge = response.RequestCharge;
            ScriptLog = response.ScriptLog;
            Headers = response.Headers.ToDictionary();
            Result = response.Resource;
        }

        public string Result { get; }
        public string ScriptLog { get; }
        public Dictionary<string, string> Headers { get; internal set; } = new Dictionary<string, string>();
    }
}
