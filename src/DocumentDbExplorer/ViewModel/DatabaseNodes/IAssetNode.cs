using System.Windows.Media;
using CosmosDbExplorer.Infrastructure.Models;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModel
{
    public interface IContent
    {
        string ContentId { get;  }
    }

    public interface IAssetNode<T> : IContent
        where T: Resource
    {
        Color? AccentColor { get; }
        T Resource { get; set; }
    }
}
