using System;
using System.Collections.Generic;
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

            var token = string.Empty;
            if (_cosmosContainer.PartitionKeyJsonPath.Any())
            {
                token = string.Join(string.Empty, _cosmosContainer.PartitionKeyJsonPath.Select((x, index) => $", c{x} as _pk{index}"));
                token += ", true as _hasPartitionKey";
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
                result.Headers = response.Headers.ToDictionary();
            }

            return result;
        }

        public async Task<CosmosQueryResult<JObject>> GetDocumentAsync(ICosmosDocument document, IDocumentRequestOptions options, CancellationToken cancellationToken)
        {
            var result = new CosmosQueryResult<JObject>();

            var requestOptions = new ItemRequestOptions
            {
                IndexingDirective = options.IndexingDirective is not null ? Enum.Parse<IndexingDirective>(options.IndexingDirective.ToString()) : null,
                ConsistencyLevel = options.ConsistencyLevel is not null ? Enum.Parse<ConsistencyLevel>(options.ConsistencyLevel.ToString()) : null,
                IfMatchEtag = options.AccessCondition == CosmosAccessConditionType.IfMatch ? options.ETag : null,
                IfNoneMatchEtag = options.AccessCondition == CosmosAccessConditionType.IfNotMatch ? options.ETag : null,
                PreTriggers = options.PreTriggers,
                PostTriggers = options.PostTriggers
            };

            var partitionKey = PartitionKeyHelper.Build(_cosmosContainer.PartitionKeyJsonPath, document.PartitionKey0, document.PartitionKey1, document.PartitionKey2);

            var (response, exception) = await ReadItemAsync(document, requestOptions, partitionKey, cancellationToken);

            if (response == null)
            {
                (response, exception) = await ReadItemAsync(document, requestOptions, PartitionKey.None, cancellationToken);

                if (response == null && exception != null)
                {
                    throw exception;
                }
            }

            if (response != null)
            {
                result.RequestCharge = response.RequestCharge;
                result.Items = response.Resource;
                result.Headers = response.Headers.ToDictionary();
                //result.Diagnostics = JObject.Parse(result.Diagnostics?.ToString());
            }

            return result;
        }

        private async Task<(ItemResponse<JObject>? response, Exception? exception)> ReadItemAsync(ICosmosDocument document, ItemRequestOptions requestOptions, PartitionKey partitionKey, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _container.ReadItemAsync<JObject>(document.Id,
                    partitionKey: partitionKey,
                    requestOptions: requestOptions,
                    cancellationToken);

                return (response, null);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return (null, ex);
            }
        }

        public async Task<CosmosQueryResult<JObject>> SaveDocumentAsync(string content, IDocumentRequestOptions options, CancellationToken cancellationToken)
        {
            var result = new CosmosQueryResult<JObject>();

            var requestOptions = new ItemRequestOptions
            {
                IndexingDirective = options.IndexingDirective is not null ? Enum.Parse<IndexingDirective>(options.IndexingDirective.ToString()) : null,
                ConsistencyLevel = options.ConsistencyLevel is not null ? Enum.Parse<ConsistencyLevel>(options.ConsistencyLevel.ToString()) : null,
                IfMatchEtag = options.AccessCondition == CosmosAccessConditionType.IfMatch && options.ETag != null ? options.ETag : null,
                IfNoneMatchEtag = options.AccessCondition == CosmosAccessConditionType.IfNotMatch && options.ETag != null ? options.ETag : null,
                PreTriggers = options.PreTriggers,
                PostTriggers = options.PostTriggers
            };

            try
            {
                var document = GetDocuments(content, _cosmosContainer.PartitionKeyJsonPath).First();
                var partitionKey = PartitionKeyHelper.Build(_cosmosContainer.PartitionKeyJsonPath, document.PartitionKey0, document.PartitionKey1, document.PartitionKey2);

                var response = await _container.UpsertItemAsync<JToken>(document.Resource, partitionKey, requestOptions, cancellationToken);

                result.RequestCharge = response.RequestCharge;
                result.Items = (JObject)response.Resource;
                result.Headers = response.Headers.ToDictionary();
                //result.Diagnostics = JObject.Parse(result.Diagnostics?.ToString());

                return result;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return result;
            }
            catch (CosmosException ce)
            {
                throw new Exception(ce.GetMessage());
            }
        }

        public async Task<CosmosResult> DeleteDocumentsAsync(IEnumerable<ICosmosDocument> documents, CancellationToken cancellationToken)
        {
            var tasks = new List<Task<ItemResponse<ICosmosDocument>>>(documents.Count());

            foreach (var item in documents)
            {
                var partitionKey = PartitionKeyHelper.Build(_cosmosContainer.PartitionKeyJsonPath, item.PartitionKey0, item.PartitionKey1, item.PartitionKey2);

                tasks.Add(_container.DeleteItemAsync<ICosmosDocument>(item.Id, partitionKey, cancellationToken: cancellationToken)
                    .ContinueWith<ItemResponse<ICosmosDocument>>(itemResponse =>
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
                        else
                        {
                            return itemResponse.Result;
                        }
                    }));
            }

            await Task.WhenAll(tasks);

            var result = new CosmosResult
            {
                RequestCharge = tasks.Where(t => t.IsCompletedSuccessfully).Sum(t => t.Result.RequestCharge)
            };

            return result;
        }

        public async Task<int> ImportDocumentsAsync(string content, CancellationToken cancellationToken)
        {
            // TODO: https://docs.microsoft.com/en-us/azure/cosmos-db/sql/how-to-migrate-from-bulk-executor-library
            var itemsToInsert = GetDocuments(content, _cosmosContainer.PartitionKeyJsonPath);
            var tasks = new List<Task>(itemsToInsert.Length);

            foreach (var item in itemsToInsert)
            {
                var partitionKey = PartitionKeyHelper.Build(_cosmosContainer.PartitionKeyJsonPath, item.PartitionKey0, item.PartitionKey1, item.PartitionKey2);

                tasks.Add(_container.CreateItemAsync(item.Resource, partitionKey, cancellationToken: cancellationToken)
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

        public async Task<CosmosQueryResult<IReadOnlyCollection<JToken>>> ExecuteQueryAsync(ICosmosQuery query, CancellationToken cancellationToken)
        {
            var result = new CosmosQueryResult<IReadOnlyCollection<JToken>>();

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

            try
            {
                using (var resultSet = _container.GetItemQueryIterator<JToken>(
                    queryText: query.QueryText,
                    continuationToken: query.ContinuationToken,
                    requestOptions: options))
                {
                    var response = await resultSet.ReadNextAsync(cancellationToken);

                    result.RequestCharge = response.RequestCharge;
                    result.ContinuationToken = response.GetContinuationTokenSafely();
                    result.Items = response.Resource.ToArray();
                    result.Headers = response.Headers.ToDictionary();
                    //result.Diagnostics = response.Diagnostics.ToString;
                    result.IndexMetrics = response.IndexMetrics;
                }

                return result;
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage());
            }
        }



        private Document[] GetDocuments(string content, IList<string> partitionKeyPath)
        {
            if (partitionKeyPath is null)
            {
                throw new ArgumentNullException(nameof(partitionKeyPath));
            }

            var token = JToken.Parse(content);

            if (token == null)
            {
                return [];
            }

            if (token is JArray)
            {
                return token.Children().Select(child => new Document((JObject)child, partitionKeyPath)).ToArray();
            }
            else
            {
                return [new Document((JObject)token, partitionKeyPath)];
            }
        }
    }

    internal class Document
    {
        public Document(JObject resource, IList<string> partitionKeyPath)
        {
            switch (partitionKeyPath.Count)
            {
                case 1:
                    PartitionKey0 = resource.SelectToken(partitionKeyPath[0])?.ToObject<object?>();
                    break;
                case 2:
                    PartitionKey0 = resource.SelectToken(partitionKeyPath[0])?.ToObject<object?>();
                    PartitionKey1 = resource.SelectToken(partitionKeyPath[1])?.ToObject<object?>();
                    break;
                case 3:
                    PartitionKey0 = resource.SelectToken(partitionKeyPath[0])?.ToObject<object?>();
                    PartitionKey1 = resource.SelectToken(partitionKeyPath[1])?.ToObject<object?>();
                    PartitionKey2 = resource.SelectToken(partitionKeyPath[2])?.ToObject<object?>();
                    break;
            }

            Resource = resource;
        }

        public object? PartitionKey0 { get; set; }
        public object? PartitionKey1 { get; set; }
        public object? PartitionKey2 { get; set; }
        public JToken Resource { get; set; }
    }
}
