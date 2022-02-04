using System;
using System.Collections.Generic;

using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Contracts.Services
{
    public interface IPersistAndRestoreService
    {
        void RestoreData();

        void PersistData();

        List<CosmosConnection> GetConnections();

        void PersistConnection(CosmosConnection connection);

        void RemoveConnection(CosmosConnection connection);

        void ReorderConnections(int sourceIndex, int targetIndex);
    }
}
