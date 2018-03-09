using System.Windows.Media;
using DocumentDbExplorer.Infrastructure.Models;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
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
