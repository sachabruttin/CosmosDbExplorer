using Newtonsoft.Json;

namespace DocumentDbExplorer.Infrastructure.Models
{
    public class ResponseContinuation
    { 
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("range")]
        public Range Range { get; set; }
    }

    public class Range
    {

        [JsonProperty("min")]
        public string Minimum { get; set; }

        [JsonProperty("max")]
        public string Maximum { get; set; }
    }

}
