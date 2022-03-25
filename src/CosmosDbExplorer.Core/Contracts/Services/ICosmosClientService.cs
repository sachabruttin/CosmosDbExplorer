using CosmosDbExplorer.Core.Models;
using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Contracts.Services
{
    public interface ICosmosClientService
    {
        CosmosClient GetClient(CosmosConnection connection);

        void DeleteClient(CosmosConnection connection);
    }
}
