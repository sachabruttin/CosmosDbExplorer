using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public interface IHaveContainerNodeViewModel
    {
        ContainerNodeViewModel ContainerNode { get; }
    }
}
