using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Contracts.Services
{
    public interface IPersistAndRestoreService
    {
        void RestoreData();

        void PersistData();

        void PersistConnection(CosmosConnection connection);
    }
}
