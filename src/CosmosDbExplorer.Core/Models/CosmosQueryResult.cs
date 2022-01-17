﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosQueryResult<T>
    {
        public T? Items { get; internal set; } = default;
        public Exception? Error { get; internal set; }
        public TimeSpan TimeElapsed { get; internal set; }
        public double RequestCharge { get; internal set; }
        public string? ContinuationToken { get; internal set; }
        public IEnumerable<string> Warnings { get; internal set; } = Array.Empty<string>();
        public Dictionary<string, string> Headers { get; internal set; } = new Dictionary<string, string>();
        public bool HasMore => !string.IsNullOrEmpty(ContinuationToken);
        public JObject? Diagnostics { get; internal set; }
    }
}
