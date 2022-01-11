using CosmosDbExplorer.ViewModels;

namespace CosmosDbExplorer.Messages
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
