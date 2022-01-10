using System;

namespace CosmosDbExplorer.Contracts.Services
{
    public interface IApplicationInfoService
    {
        Version GetVersion();
    }
}
