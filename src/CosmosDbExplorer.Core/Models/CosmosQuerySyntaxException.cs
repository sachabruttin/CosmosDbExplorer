using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosQuerySyntaxException
    {
        [JsonProperty("errors")]
        public List<Error>? Errors { get; set; }
    }

    public partial class Error
    {
        [JsonProperty("severity")]
        public string? Severity { get; set; }

        [JsonProperty("location")]
        public Location? Location { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }
    }

    public partial class Location
    {
        [JsonProperty("start")]
        public long? Start { get; set; }

        [JsonProperty("end")]
        public long? End { get; set; }
    }
}
