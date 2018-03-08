using DocumentDbExplorer.Infrastructure.Models;

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
