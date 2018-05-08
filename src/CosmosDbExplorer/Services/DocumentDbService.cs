using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json.Linq;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.ViewModel.Interfaces;
using GalaSoft.MvvmLight.Messaging;
using CosmosDbExplorer.Messages;
using System.Threading;
using CosmosDbExplorer.Infrastructure.Extensions;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Newtonsoft.Json;
using System.IO;

namespace CosmosDbExplorer.Services
{
    public class DocumentDbService : IDocumentDbService
    {
        public DocumentDbService(IMessenger messenger)
        {
            messenger.Register<ConnectionSettingSavedMessage>(this, msg =>
            {
                if (_clientInstances.ContainsKey(msg.Connection))
                {
                    _clientInstances.Remove(msg.Connection);
                }
            });
        }

        private static readonly Dictionary<Connection, DocumentClient> _clientInstances = new Dictionary<Connection, DocumentClient>();

        private DocumentClient GetClient(Connection connection)
        {
            if (!_clientInstances.ContainsKey(connection))
            {
                var policy = new ConnectionPolicy
                {
                    ConnectionMode = connection.ConnectionType == ConnectionType.Gateway ? ConnectionMode.Gateway : ConnectionMode.Direct,
                    ConnectionProtocol = connection.ConnectionType == ConnectionType.DirectHttps ? Protocol.Https : Protocol.Tcp
                };

                var client = new DocumentClient(connection.DatabaseUri, connection.AuthenticationKey, policy);
                client.OpenAsync();

                _clientInstances.Add(connection, client);
            }

            return _clientInstances[connection];
        }

        public async Task CleanCollectionAsync(Connection connection, DocumentCollection collection)
        {
            const string sqlQuery = "SELECT * FROM c";
            var client = GetClient(connection);
            var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true };
            var results = client.CreateDocumentQuery<Document>(collection.DocumentsLink, sqlQuery, feedOptions).AsDocumentQuery();
            var partitionKeyPath = collection.PartitionKey.GetSelectToken();

            //While there are more results
            while (results.HasMoreResults)
            {
                //enumerate and delete the documents in this batch
                foreach (Document doc in await results.ExecuteNextAsync().ConfigureAwait(false))
                {
                    var requestOptions = new RequestOptions();
                    if (partitionKeyPath != null)
                    {
                        requestOptions.PartitionKey = new PartitionKey(doc.GetPartitionKeyValue(partitionKeyPath));
                    }

                    await client.DeleteDocumentAsync(doc.SelfLink, requestOptions).ConfigureAwait(false);
                }
            }
        }

        public Task<ResourceResponse<Document>> DeleteDocumentAsync(Connection connection, DocumentDescription document)
        {
            var options = document.PartitionKey != null
                            ? new RequestOptions { PartitionKey = new PartitionKey(document.PartitionKey) }
                            : new RequestOptions();

            return GetClient(connection).DeleteDocumentAsync(document.SelfLink, options);
        }

        public Task<FeedResponse<dynamic>> ExecuteQueryAsync(Connection connection, DocumentCollection collection, string query, IHaveQuerySettings querySettings, string continuationToken, CancellationToken cancellationToken)
        {
            var feedOptions = new FeedOptions
            {
                EnableCrossPartitionQuery = querySettings.EnableCrossPartitionQuery.GetValueOrDefault(),
                EnableScanInQuery = querySettings.EnableScanInQuery,
                MaxItemCount = querySettings.MaxItemCount,
                MaxDegreeOfParallelism = querySettings.MaxDOP.GetValueOrDefault(-1),
                MaxBufferedItemCount = querySettings.MaxBufferItem.GetValueOrDefault(-1),
                RequestContinuation = continuationToken,
                PopulateQueryMetrics = true,
                PartitionKey = GetPartitionKey(querySettings.PartitionKeyValue)
            };

            return GetClient(connection)
                                    .CreateDocumentQuery(collection.DocumentsLink, query, feedOptions)
                                    .AsDocumentQuery()
                                    .ExecuteNextAsync(cancellationToken);
        }

        public async Task<List<DocumentCollection>> GetCollectionsAsync(Connection connection, Database database)
        {
            var client = GetClient(connection);

            var result = await client.ReadDocumentCollectionFeedAsync(database.SelfLink).ConfigureAwait(false);
            return result.ToList();
        }

        public async Task<List<Database>> GetDatabasesAsync(Connection connection)
        {
            var client = GetClient(connection);
            var result = await client.ReadDatabaseFeedAsync().ConfigureAwait(false);

            return result.ToList();
        }

        public Task<ResourceResponse<Document>> GetDocumentAsync(Connection connection, DocumentDescription document)
        {
            var options = document.PartitionKey != null
                            ? new RequestOptions { PartitionKey = new PartitionKey(document.PartitionKey) }
                            : new RequestOptions();

            return GetClient(connection).ReadDocumentAsync(document.SelfLink, options);
        }

        public async Task<DocumentDescriptionList> GetDocumentsAsync(Connection connection, DocumentCollection collection, string filter, int maxItems, string continuationToken, IHaveRequestOptions requestOptions)
        {
            var token = collection.PartitionKey.GetQueryToken();
            if (token != null)
            {
                token = $", c{token} as _partitionKey";
            }

            var sql = $"SELECT c.id, c._self {token} FROM c {filter}";

            var feedOptions = new FeedOptions
            {
                MaxItemCount = maxItems,
                RequestContinuation = continuationToken,
                EnableCrossPartitionQuery = true,
                EnableScanInQuery = false,
                PartitionKey = GetPartitionKey(requestOptions?.PartitionKeyValue),

                MaxDegreeOfParallelism = -1,
                MaxBufferedItemCount = -1,
                PopulateQueryMetrics = true,
            };

            var data = await GetClient(connection)
                                        .CreateDocumentQuery(collection.DocumentsLink, sql, feedOptions)
                                        .AsDocumentQuery()
                                        .ExecuteNextAsync<DocumentDescription>().ConfigureAwait(true);

            return new DocumentDescriptionList(data.ToList())
            {
                ContinuationToken = data.ResponseContinuation,
                RequestCharge = data.RequestCharge
            };
        }

        public async Task<BulkImportResponse> ImportDocumentAsync(Connection connection, DocumentCollection collection, 
            string content, 
            bool allowUpsert, 
            bool allowIdGeneration, 
            int? maxConcurrencyPerPartitionKeyRange,
            int? maxInMemorySortingBatchSize,
            CancellationToken cancellationToken)
        {
            var client = GetClient(connection);

            // save retry options
            var maxRetryWaitTimeInSeconds = client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds;
            var maxRetryAttemptsOnThrottledRequests = client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests;

            try
            {
                // Set retry options high during initialization (default values).
                client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 30;
                client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 9;

                var bulkExecutor = new BulkExecutor(client, collection);
                await bulkExecutor.InitializeAsync().ConfigureAwait(false);

                // Set retries to 0 to pass complete control to bulk executor.
                client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 0;
                client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 0;

                // see https://github.com/Azure/azure-cosmosdb-bulkexecutor-dotnet-getting-started

                return await bulkExecutor.BulkImportAsync(
                    documents: GetDocuments(content),
                    enableUpsert: allowUpsert,
                    disableAutomaticIdGeneration: !allowIdGeneration,
                    maxConcurrencyPerPartitionKeyRange: maxConcurrencyPerPartitionKeyRange,
                    maxInMemorySortingBatchSize: maxInMemorySortingBatchSize,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                // Reset retry options.
                client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = maxRetryWaitTimeInSeconds;
                client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = maxRetryAttemptsOnThrottledRequests;
            }
        }

        private IEnumerable<Document> GetDocuments(string path)
        {
            var serializer = new JsonSerializer();

            using (var filestream = File.Open(path, FileMode.Open))
            using (var streamReader = new StreamReader(filestream))
            using (var reader = new JsonTextReader(streamReader))
            {
                while (reader.Read())
                {
                    // deserialize only when there's "{" character in the stream
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        yield return serializer.Deserialize<Document>(reader);
                    }
                }
            }
        }

        private static RequestOptions GetRequestOptions(IHaveRequestOptions request)
        {
            return new RequestOptions
            {
                ConsistencyLevel = request.ConsistencyLevel,
                IndexingDirective = request.IndexingDirective,
                PreTriggerInclude = request.PreTrigger != null ? new List<string> { request.PreTrigger } : null,
                PostTriggerInclude = request.PreTrigger != null ? new List<string> { request.PostTrigger } : null,
                PartitionKey = GetPartitionKey(request.PartitionKeyValue),
                AccessCondition = request.AccessConditionType != null ? new AccessCondition { Condition = request.AccessCondition, Type = request.AccessConditionType.Value } : null,
            };
        }

        private static PartitionKey GetPartitionKey(string partitionKeyValue)
        {
            if (string.IsNullOrEmpty(partitionKeyValue?.Trim()))
            {
                return null;
            }

            var value = JToken.Parse(partitionKeyValue).ToObject<object>();
            return new PartitionKey(value);
        }

        public Task<ResourceResponse<Document>> UpdateDocumentAsync(Connection connection, string altLink, string content, IHaveRequestOptions requestOptions)
        {
            var instance = JObject.Parse(content);
            var options = GetRequestOptions(requestOptions);
            return GetClient(connection).UpsertDocumentAsync(altLink, instance, options);
        }

        public async Task<StoredProcedure> SaveStoredProcedureAsync(Connection connection, DocumentCollection collection, string id, string function, string storeProcedureLink)
        {
            var item = new StoredProcedure { Id = id, Body = function };

            if (!string.IsNullOrEmpty(storeProcedureLink))
            {
                var oldId = storeProcedureLink.Split('/').Last();

                if (item.Id != oldId)
                {
                    var itemList = await GetStoredProceduresAsync(connection, collection).ConfigureAwait(false);
                    if (itemList.Any(t => t.Id == item.Id))
                    {
                        throw new Exception("An item with the same id already exists!");
                    }
                }

                await GetClient(connection).DeleteStoredProcedureAsync(storeProcedureLink).ConfigureAwait(false);
            }

            var response = await GetClient(connection).CreateStoredProcedureAsync(collection.SelfLink, item).ConfigureAwait(false);
            return response.Resource;
        }

        public async Task<UserDefinedFunction> SaveUdfAsync(Connection connection, DocumentCollection collection, string id, string function, string altLink)
        {
            var item = new UserDefinedFunction { Id = id, Body = function };
            var client = GetClient(connection);

            if (!string.IsNullOrEmpty(altLink))
            {
                var oldId = altLink.Split('/').Last();

                if (item.Id != oldId)
                {
                    var itemList = await GetUdfsAsync(connection, collection).ConfigureAwait(false);
                    if (itemList.Any(t => t.Id == item.Id))
                    {
                        throw new Exception("An item with the same id already exists!");
                    }
                }

                await client.DeleteUserDefinedFunctionAsync(altLink).ConfigureAwait(false);
            }

            var response = await client.CreateUserDefinedFunctionAsync(collection.SelfLink, item).ConfigureAwait(false);
            return response.Resource;
        }

        public Task DeleteStoredProcedureAsync(Connection connection, string storedProcedureLink)
        {
            return GetClient(connection).DeleteStoredProcedureAsync(storedProcedureLink);
        }

        public Task DeleteUdfAsync(Connection connection, string udfLink)
        {
            return GetClient(connection).DeleteUserDefinedFunctionAsync(udfLink);
        }

        public async Task<IList<StoredProcedure>> GetStoredProceduresAsync(Connection connection, DocumentCollection collection)
        {
            var response = await GetClient(connection).ReadStoredProcedureFeedAsync(collection.StoredProceduresLink).ConfigureAwait(false);
            return response.Select(sp => sp).ToList();
        }

        public async Task<IList<UserDefinedFunction>> GetUdfsAsync(Connection connection, DocumentCollection collection)
        {
            var response = await GetClient(connection).ReadUserDefinedFunctionFeedAsync(collection.UserDefinedFunctionsLink).ConfigureAwait(false);
            return response.Select(sp => sp).ToList();
        }

        public async Task<IList<Trigger>> GetTriggersAsync(Connection connection, DocumentCollection collection)
        {
            var response = await GetClient(connection).ReadTriggerFeedAsync(collection.TriggersLink).ConfigureAwait(false);
            return response.Select(sp => sp).ToList();
        }

        public async Task<Trigger> SaveTriggerAsync(Connection connection, DocumentCollection collection, string id, string trigger, TriggerType triggerType, TriggerOperation triggerOperation, string altLink)
        {
            var item = new Trigger { Id = id, Body = trigger, TriggerType = triggerType, TriggerOperation = triggerOperation };
            var client = GetClient(connection);

            if (!string.IsNullOrEmpty(altLink))
            {
                var oldId = altLink.Split('/').Last();

                if (item.Id != oldId)
                {
                    var itemList = await GetTriggersAsync(connection, collection).ConfigureAwait(false);
                    if (itemList.Any(t => t.Id == item.Id))
                    {
                        throw new Exception("An item with the same id already exists!");
                    }
                }

                await client.DeleteTriggerAsync(altLink).ConfigureAwait(false);
            }

            var response = await client.CreateTriggerAsync(collection.SelfLink, item).ConfigureAwait(false);
            return response.Resource;
        }

        public Task DeleteTriggerAsync(Connection connection, string triggerLink)
        {
            return GetClient(connection).DeleteTriggerAsync(triggerLink);
        }

        public async Task<int> GetThroughputAsync(Connection connection, DocumentCollection collection)
        {
            var query = GetClient(connection).CreateOfferQuery().Where(o => o.ResourceLink == collection.SelfLink).AsDocumentQuery();

            var result = await query.ExecuteNextAsync<OfferV2>().ConfigureAwait(false);
            var offer = result.Single();

            return offer.Content.OfferThroughput;
        }

        public async Task UpdateCollectionSettingsAsync(Connection connection, DocumentCollection collection, int throughput)
        {
            var tasks = new[]
            {
                GetClient(connection).ReplaceDocumentCollectionAsync(collection),
                UpdateOfferThroughput(connection, collection, throughput)
            };

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task UpdateOfferThroughput(Connection connection, DocumentCollection collection, int throughtput)
        {
            var client = GetClient(connection);
            var query = client.CreateOfferQuery().Where(o => o.ResourceLink == collection.SelfLink).AsDocumentQuery();

            var result = await query.ExecuteNextAsync<OfferV2>().ConfigureAwait(false);
            var offer = result.Single();

            if (offer.Content.OfferThroughput != throughtput)
            {
                offer = new OfferV2(offer, throughtput);
                await client.ReplaceOfferAsync(offer).ConfigureAwait(false);
            }
        }

        public async Task<DocumentCollection> CreateCollectionAsync(Connection connection, Database database, DocumentCollection collection, int throughput)
        {
            var client = GetClient(connection);
            var options = new RequestOptions { OfferThroughput = throughput };

            if (database.SelfLink == null)
            {
                database = await client.CreateDatabaseIfNotExistsAsync(database).ConfigureAwait(false);
            }

            return await client.CreateDocumentCollectionAsync(database.SelfLink, collection, options).ConfigureAwait(false);
        }

        public Task DeleteCollectionAsync(Connection connection, DocumentCollection collection)
        {
            return GetClient(connection).DeleteDocumentCollectionAsync(collection.SelfLink);
        }

        public Task DeleteDatabaseAsync(Connection connection, Database database)
        {
            return GetClient(connection).DeleteDatabaseAsync(database.SelfLink);
        }

        public async Task<IList<User>> GetUsersAsync(Connection connection, Database database)
        {
            var response = await GetClient(connection).ReadUserFeedAsync(database.UsersLink).ConfigureAwait(false);
            return response.Select(u => u).OrderBy(u => u.Id).ToList();
        }

        public async Task<User> SaveUserAsync(Connection connection, Database database, User user)
        {
            if (user.SelfLink != null)
            {
                var response = await GetClient(connection).ReplaceUserAsync(user).ConfigureAwait(false);
                return response.Resource;
            }
            else
            {
                var response = await GetClient(connection).CreateUserAsync(database.SelfLink, user).ConfigureAwait(false);
                return response.Resource;
            }
        }

        public Task DeleteUserAsync(Connection connection, User user)
        {
            return GetClient(connection).DeleteUserAsync(user.SelfLink);
        }

        public async Task<IList<Permission>> GetPermissionAsync(Connection connection, User user)
        {
            var response = await GetClient(connection).ReadPermissionFeedAsync(user.PermissionsLink).ConfigureAwait(false);
            return response.Select(p => p).OrderBy(p => p.Id).ToList();
        }

        public async Task<Permission> SavePermissionAsync(Connection connection, User user, Permission permission)
        {
            if (permission.SelfLink != null)
            {
                return await GetClient(connection).ReplacePermissionAsync(permission).ConfigureAwait(false);
            }

            return await GetClient(connection).CreatePermissionAsync(user.SelfLink, permission).ConfigureAwait(false);
        }

        public Task DeletePermissionAsync(Connection connection, Permission permission)
        {
            return GetClient(connection).DeletePermissionAsync(permission.SelfLink);
        }

        public async Task<int> GetPartitionKeyRangeCountAsync(Connection connection, DocumentCollection collection)
        {
            var metrics = await GetPartitionMetricsAsync(connection, collection).ConfigureAwait(false);
            return metrics.PartitionCount;
        }

        public async Task<CollectionMetric> GetPartitionMetricsAsync(Connection connection, DocumentCollection collection)
        {
            var documentCollection = await GetClient(connection).ReadDocumentCollectionAsync(collection.AltLink,
                new RequestOptions
                {
                    PopulateQuotaInfo = true,
                    PopulatePartitionKeyRangeStatistics = true,
                }).ConfigureAwait(false);

            return new CollectionMetric(documentCollection);
        }

        public async Task<Dictionary<string, int>> GetTopPartitionKeys(Connection connection, DocumentCollection collection, string partitionKeyRangeId, int sampleCount = 100)
        {
            var stats = new Dictionary<string, int>();
            var partitionKey = collection.PartitionKey.Paths[0].TrimStart('/');
            var readCount = 0;
            var client = GetClient(connection);
            var options = new ChangeFeedOptions { StartFromBeginning = true, MaxItemCount = -1, PartitionKeyRangeId = partitionKeyRangeId };

            while (readCount < sampleCount)
            {
                var results = await client.CreateDocumentChangeFeedQuery(collection.AltLink, options)
                                          .ExecuteNextAsync<Document>().ConfigureAwait(false);

                if (results.Count == 0)
                {
                    break;
                }

                foreach (var document in results)
                {
                    var key = document.GetPropertyValue<string>(partitionKey) ?? "N/A";

                    if (stats.ContainsKey(key))
                    {
                        stats[key]++;
                    }
                    else
                    {
                        stats.Add(key, 1);
                    }
                }

                readCount += results.Count;
            }

            return stats;
        }

        public Task<StoredProcedureResponse<dynamic>> ExecuteStoreProcedureAsync(Connection connection, string altLink, IList<dynamic> parameters, string partitionKey)
        {
            var options = new RequestOptions
            {
                EnableScriptLogging = true,
                PartitionKey = GetPartitionKey(partitionKey)
            };

            return GetClient(connection).ExecuteStoredProcedureAsync<dynamic>(altLink, options, parameters.ToArray());
        }
    }
}
