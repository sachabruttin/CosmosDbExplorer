using System.Windows.Media;
using CosmosDbExplorer.Core.Contracts;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    // TODO: Define an CosmosDB Resource Class
    public interface IAssetNode<T> : IContent
    where T : ICosmosResource
    {
        System.Drawing.Color? AccentColor { get; }
        T? Resource { get; set; }
    }
}
