using DocumentDbExplorer.Infrastructure.Models;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Messages
{

    public class RemoveNodeMessage
    {
        public RemoveNodeMessage(string altLink)
        {
            AltLink = altLink;
        }

        public string AltLink { get; }
    }
}
