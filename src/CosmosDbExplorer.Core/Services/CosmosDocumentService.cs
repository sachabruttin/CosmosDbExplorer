using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Contracts;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Core.Services
{
    public class CosmosDocumentService : ICosmosDocumentService
    {
        private readonly CosmosDatabase _database;
        private readonly CosmosContainer _container;
        private CosmosClient _client;

        public CosmosDocumentService(ICosmosClientService clientService, CosmosConnection connection, CosmosDatabase database, CosmosContainer container)
        {
            _client = clientService.GetClient(connection);
            _database = database;
            _container = container;
        }

        public Task<ICosmosDocument> GetAsync(string id, CancellationToken cancellationToken)
        {
            //var container = _client.GetContainer(_database.Id, _container.Id);
            //var response = await container.ReadItemAsync<JObject>(id, 
            //    _container.PartitionKeyPath,
            //    null,
            //    cancellationToken);
            throw new System.NotImplementedException();
        }

        public async Task<CosmosQueryResult> ReadAllItem(string? filter, int? maxItemsCount, string? continuationToken, CancellationToken cancellationToken)
        {
            var container = _client.GetContainer(_database.Id, _container.Id);
            var result = new CosmosQueryResult();

            var options = new QueryRequestOptions
            {
                MaxItemCount = maxItemsCount
            };

            using (var resultSet = container.GetItemQueryIterator<JObject>(
                queryDefinition: null,
                continuationToken: continuationToken,
                requestOptions: options))
            {
                var response = await resultSet.ReadNextAsync(cancellationToken);

                result.RequestCharge = response.RequestCharge;
                result.ContinuationToken = response.ContinuationToken;
                result.Items = response.Select(i => new CosmosDocument { Document = i, PartitionKey = GetPartitionKeyValue(i) }).ToArray();
                result.Headers = response.Headers.AllKeys().ToDictionary(key => key, key => response.Headers.GetValueOrDefault(key));
            }

            return result;
        }

        private string? GetPartitionKeyValue(JObject document)
        {
            if (_container.PartitionKeyJsonPath == null)
            {
                return null;
            }

            return document.SelectToken(_container.PartitionKeyJsonPath)?.ToString();
        }
    }
}
