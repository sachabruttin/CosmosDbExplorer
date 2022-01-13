using System.Text;
using CosmosDbExplorer.Core.Contracts;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosDocument : ICosmosDocument
    {
        public JObject Document { get; set; }

        public string Id => (string)Document.GetValue("id");

        public string ETag => (string)Document.GetValue("_etag");

        public string SelfLink => (string)Document.GetValue("_self");

        public string Attachments => (string)Document.GetValue("_attachments");

        public string TimeStamp => (string)Document.GetValue("_");
    }
}
