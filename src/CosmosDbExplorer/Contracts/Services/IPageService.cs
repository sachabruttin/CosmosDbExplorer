using System;
using System.Windows.Controls;

namespace CosmosDbExplorer.Contracts.Services
{
    public interface IPageService
    {
        Type GetPageType(Type key);

        Page GetPage(Type key);

        Page GetPage(Type key, object parameters);
    }
}
