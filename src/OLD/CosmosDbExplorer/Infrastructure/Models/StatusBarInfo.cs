using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CosmosDbExplorer.Infrastructure.Models
{
    public class StatusBarInfo : IStatusBarInfo
    {
        public StatusBarInfo(double? requestCharge, Document resource, NameValueCollection responseHeaders)
        {
            RequestCharge = requestCharge;
            Resource = resource;
            ResponseHeaders = responseHeaders;
        }

        public StatusBarInfo(ResourceResponse<Document> response)
        {
            RequestCharge = response?.RequestCharge;
            Resource = response?.Resource;
            ResponseHeaders = response?.ResponseHeaders;
        }

        public StatusBarInfo(IEnumerable<ResourceResponse<Document>> response)
        {
            RequestCharge = response.Sum(r => r.RequestCharge);
            Resource = null;
            ResponseHeaders = null;
        }

        public double? RequestCharge { get; }

        public Document Resource { get; }

        public NameValueCollection ResponseHeaders { get; }
    }

    public interface IStatusBarInfo
    {
        double? RequestCharge { get; }

        Document Resource { get; }

        NameValueCollection ResponseHeaders { get; }
    }
}
