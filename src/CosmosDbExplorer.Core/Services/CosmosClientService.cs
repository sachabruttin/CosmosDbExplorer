using System;
using System.Collections.Concurrent;

using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Models;

using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Services
{
    public class CosmosClientService : ICosmosClientService
    {
        private readonly ConcurrentDictionary<Guid, CosmosClient> _client = new();

        public CosmosClient GetClient(CosmosConnection connection)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return _client.GetOrAdd(connection.Id, CreateClient(connection));
        }

        public void DeleteClient(CosmosConnection connection)
        {
            throw new NotImplementedException();
        }

        private CosmosClient CreateClient(CosmosConnection connection)
        {
            var options = new CosmosClientOptions
            {
                ConnectionMode = connection.ConnectionType == ConnectionType.Gateway ? ConnectionMode.Gateway : ConnectionMode.Direct,
                EnableTcpConnectionEndpointRediscovery = connection.EnableEndpointDiscovery,
                LimitToEndpoint = connection.LimitToEndpoint
            };

            return new CosmosClient(accountEndpoint: connection.DatabaseUri?.ToString(), authKeyOrResourceToken: connection.AuthenticationKey, options);
        }
    }
}
