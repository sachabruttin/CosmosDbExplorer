using System.Collections.Specialized;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface IStatusBarInfo
    {
        double? RequestCharge { get; }

        object Resource { get; }

        NameValueCollection ResponseHeaders { get; }
    }
}
