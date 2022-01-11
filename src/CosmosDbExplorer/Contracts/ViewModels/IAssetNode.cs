using System.Windows.Media;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    // TODO: Define an CosmosDB Resource Class
    public interface IAssetNode<T> : IContent
    //where T : Resource
    {
        Color? AccentColor { get; }
        T Resource { get; set; }
    }
}
