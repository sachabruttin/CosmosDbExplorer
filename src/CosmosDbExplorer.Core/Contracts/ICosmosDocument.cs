using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Core.Contracts
{
    public interface ICosmosDocument : ICosmosResource
    {
        JObject Document { get; set; }
    
        string Attachments { get; }

        string TimeStamp { get; }
    }
}
