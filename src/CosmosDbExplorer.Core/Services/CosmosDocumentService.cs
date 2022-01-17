using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Contracts;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Helpers;
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

        public async Task<CosmosQueryResult<IReadOnlyCollection<ICosmosDocument>>> GetDocumentsAsync(string? filter, int? maxItemsCount, string? continuationToken, CancellationToken cancellationToken)
        {
            var container = _client.GetContainer(_database.Id, _container.Id);
            var result = new CosmosQueryResult<IReadOnlyCollection<ICosmosDocument>>();

            var token = _container.PartitionKeyJsonPath;
            if (token != null)
            {
                token = $", c{token} as _partitionKey, true as _hasPartitionKey";
            }

            var sql = $"SELECT c.id, c._self, c._etag, c._ts, c._attachments {token} FROM c {filter}";

            var options = new QueryRequestOptions
            {
                MaxItemCount = maxItemsCount,
                // TODO: Handle Partition key and other IHaveRequestOptions values
                //PartitionKey = 
            };

            using (var resultSet = container.GetItemQueryIterator<CosmosDocument>(
                queryText: sql,
                continuationToken: continuationToken,
                requestOptions: options))
            {
                var response = await resultSet.ReadNextAsync(cancellationToken);

                result.RequestCharge = response.RequestCharge;
                result.ContinuationToken = response.ContinuationToken;
                result.Items = response.Resource.ToArray();
                result.Headers = response.Headers.AllKeys().ToDictionary(key => key, key => response.Headers.GetValueOrDefault(key));
            }

            return result;
        }

        public async Task<CosmosQueryResult<JObject?>> GetDocumentAsync(ICosmosDocument document, CancellationToken cancellation)
        {
            var container = _client.GetContainer(_database.Id, _container.Id);
            var result = new CosmosQueryResult<JObject?>();

            try
            {
                var response = await container.ReadItemAsync<JObject>(document.Id,
                    partitionKey: PartitionKeyHelper.Get(document.PartitionKey),
                    requestOptions: null,
                    cancellation);

                result.RequestCharge = response.RequestCharge;
                result.Items = response.Resource;
                result.Headers = response.Headers.AllKeys().ToDictionary(key => key, key => response.Headers.GetValueOrDefault(key));
                //result.Diagnostics = JObject.Parse(result.Diagnostics?.ToString());

                return result;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return result;
            }

        }
    }
}
