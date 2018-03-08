using System.Windows.Media;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{

    public interface IAssetNode<T>
        where T: Resource
    {
        Color? AccentColor { get; }
        T Resource { get; }
    }
}
