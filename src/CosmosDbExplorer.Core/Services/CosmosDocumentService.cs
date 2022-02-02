using System;
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

            var options = new ItemRequestOptions
            {
                
            };

            try
            {
                var response = await _container.ReadItemAsync<JObject>(document.Id,
                    partitionKey: PartitionKeyHelper.Get(document.PartitionKey),
                    requestOptions: options,
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

        public async Task<int> ImportDocumentsAsync(string content, CancellationToken cancellationToken)
        {
            // TODO: https://docs.microsoft.com/en-us/azure/cosmos-db/sql/how-to-migrate-from-bulk-executor-library
            var itemsToInsert = GetDocuments(content, _cosmosContainer.PartitionKeyJsonPath);
            var tasks = new List<Task>(itemsToInsert.Length);

            foreach (var item in itemsToInsert)
            {
                tasks.Add(_container.CreateItemAsync(item.Resource, PartitionKeyHelper.Get(item.PK), cancellationToken: cancellationToken)
                    .ContinueWith(itemResponse =>
                    {
                        if (!itemResponse.IsCompletedSuccessfully)
                        {
                            var innerExceptions = itemResponse.Exception.Flatten();
                            if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                            {
                                throw new Exception(cosmosException.GetMessage());
                            }
                            else
                            {
                                throw new Exception($"Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
                            }
                        }
                    }));
            }

            // Wait until all are done
            await Task.WhenAll(tasks);

            return tasks.Where(t => t.IsCompletedSuccessfully).Count();
        }

        public async Task<CosmosQueryResult<IReadOnlyCollection<JObject>>> ExecuteQueryAsync(ICosmosQuery query, CancellationToken cancellationToken)
        {
            var result = new CosmosQueryResult<IReadOnlyCollection<JObject>>();

            var options = new QueryRequestOptions
            {
                PartitionKey = query.PartitionKeyValue.IsSome ? PartitionKeyHelper.Get(query.PartitionKeyValue.Value) : null,
                EnableScanInQuery = query.EnableScanInQuery,
                MaxItemCount = query.MaxItemCount,
                MaxBufferedItemCount = query.MaxBufferItem,
                MaxConcurrency = query.MaxDOP,
                PopulateIndexMetrics = true,
                //ConsistencyLevel = ConsistencyLevel.Strong
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

        private Document[] GetDocuments(string content, string pkPath)
        {
            var token = JToken.Parse(content);

            if (token == null)
            {
                return Array.Empty<Document>();
            }

            if (token is JArray)
            {
                return token.Select(t => new Document { PK = t.SelectToken(pkPath).Value<string>(), Resource = t }).ToArray();
            }
            else
            {
                return new[] { new Document { PK = token.SelectToken(pkPath).Value<string>(), Resource = token } };
            }
        }

        internal class Document
        {
            public string PK { get; set; }
            public JToken Resource { get; set; }
        }
    }
}
