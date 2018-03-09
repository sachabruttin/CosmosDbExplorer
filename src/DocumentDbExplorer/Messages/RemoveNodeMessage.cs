using DocumentDbExplorer.Infrastructure.Models;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Messages
{

    public class RemoveNodeMessage
    {
        public RemoveNodeMessage(TreeViewItemViewModel node)
        {
            Node = node;
        }

        public TreeViewItemViewModel Node { get; }
    }
}
