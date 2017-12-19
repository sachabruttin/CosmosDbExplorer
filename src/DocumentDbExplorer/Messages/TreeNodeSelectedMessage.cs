using DocumentDbExplorer.Infrastructure.Models;

namespace DocumentDbExplorer.Messages
{
    public class TreeNodeSelectedMessage
    {
        public TreeNodeSelectedMessage(TreeViewItemViewModel item)
        {
            Item = item;
        }

        public TreeViewItemViewModel Item { get; }
    }
}
