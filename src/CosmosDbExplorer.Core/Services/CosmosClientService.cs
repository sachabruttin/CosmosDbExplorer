using System;
using System.Collections.Concurrent;
using Azure.Identity;
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
            var accountEndpoint = connection.DatabaseUri?.ToString();
            var options = new CosmosClientOptions
            {
                ConnectionMode = connection.ConnectionType == ConnectionType.Gateway ? ConnectionMode.Gateway : ConnectionMode.Direct,
                EnableTcpConnectionEndpointRediscovery = connection.EnableEndpointDiscovery,
                LimitToEndpoint = connection.LimitToEndpoint
            };

            return string.IsNullOrWhiteSpace(connection.AuthenticationKey)
                ? new(accountEndpoint, new DefaultAzureCredential(), options)
                : new(accountEndpoint, connection.AuthenticationKey, options);
        }
    }
}
