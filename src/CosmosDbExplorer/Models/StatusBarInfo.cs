using System.Collections.Generic;
using System.Linq;
using CosmosDbExplorer.Core.Contracts;
using CosmosDbExplorer.Core.Models;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Models
{
    public class StatusBarInfo : IStatusBarInfo
    {
        //public StatusBarInfo(double? requestCharge, CosmosDocument resource, Dictionary<string, string> responseHeaders)
        //{
        //    RequestCharge = requestCharge;
        //    Resource = long.Parse(resource.ite);
        //    ResponseHeaders = responseHeaders;
        //}

        public StatusBarInfo(CosmosQueryResult<JObject> response)
        {
            RequestCharge = response.RequestCharge;
            Resource = response.Items;
            ResponseHeaders = response.Headers;
        }

        public StatusBarInfo(IEnumerable<CosmosQueryResult<IReadOnlyCollection<JObject>>> response)
        {
            RequestCharge = response.Sum(r => r.RequestCharge);
            Resource = null;
            ResponseHeaders = null;
        }

        public double RequestCharge { get; }

        public JObject? Resource { get; }

        public Dictionary<string, string>? ResponseHeaders { get; }
    }

    public interface IStatusBarInfo
    {
        double RequestCharge { get; }

        JObject? Resource { get; }

        Dictionary<string, string>? ResponseHeaders { get; }
    }
}
