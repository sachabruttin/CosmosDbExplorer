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
        private readonly CosmosDatabase _cosmosDatabase;
        private readonly CosmosContainer _cosmosContainer;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public CosmosDocumentService(ICosmosClientService clientService, CosmosConnection connection, CosmosDatabase database, CosmosContainer container)
        {
            _cosmosClient = clientService.GetClient(connection);
            _cosmosDatabase = database;
            _cosmosContainer = container;

            _container = _cosmosClient.GetContainer(_cosmosDatabase.Id, _cosmosContainer.Id);
        }

        public async Task<CosmosQueryResult<IReadOnlyCollection<ICosmosDocument>>> GetDocumentsAsync(string? filter, int? maxItemsCount, string? continuationToken, CancellationToken cancellationToken)
        {
            var result = new CosmosQueryResult<IReadOnlyCollection<ICosmosDocument>>();

            var token = _cosmosContainer.PartitionKeyJsonPath;
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

            using (var resultSet = _container.GetItemQueryIterator<CosmosDocument>(
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

        public async Task<CosmosQueryResult<JObject>> GetDocumentAsync(ICosmosDocument document, CancellationToken cancellation)
        {
            var result = new CosmosQueryResult<JObject>();

            try
            {
                var response = await _container.ReadItemAsync<JObject>(document.Id,
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

        public async Task<CosmosQueryResult<IReadOnlyCollection<JObject>>> ExecuteQueryAsync(ICosmosQuery query, CancellationToken cancellationToken)
        {
            var result = new CosmosQueryResult<IReadOnlyCollection<JObject>>();

            var options = new QueryRequestOptions
            {
                // TODO: Handle Partition key and other IHaveRequestOptions values
                PartitionKey = string.IsNullOrEmpty(query.PartitionKeyValue) ? null : PartitionKeyHelper.Get(query.PartitionKeyValue),
                EnableScanInQuery = query.EnableScanInQuery,
                MaxItemCount = query.MaxItemCount,
                MaxBufferedItemCount = query.MaxBufferItem,
                MaxConcurrency = query.MaxDOP,
                PopulateIndexMetrics = true,
            };

            using (var resultSet = _container.GetItemQueryIterator<JObject>(
                queryText: query.QueryText,
                continuationToken: query.ContinuationToken,
                requestOptions: options))
            {
                var response = await resultSet.ReadNextAsync(cancellationToken);

                result.RequestCharge = response.RequestCharge;
                result.ContinuationToken = response.ContinuationToken;
                result.Items = response.Resource.ToArray();
                result.Headers = response.Headers.AllKeys().ToDictionary(key => key, key => response.Headers.GetValueOrDefault(key));
                //result.Diagnostics = response.Diagnostics.ToString;
                result.IndexMetrics = response.IndexMetrics;
            }

            return result;
        }
    }
}
