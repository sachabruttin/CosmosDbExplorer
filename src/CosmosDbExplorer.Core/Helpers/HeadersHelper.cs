using System.Collections.Generic;
using System.Linq;

using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Helpers
{
    public static class HeadersHelper
    {
        public static Dictionary<string, string> ToDictionary(this Headers headers)
        {
            return headers.AllKeys().ToDictionary(key => key, key => headers.GetValueOrDefault(key));
        }
    }
}
