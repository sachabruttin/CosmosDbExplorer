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
            var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = 500 };
            var results = client.CreateDocumentQuery<Document>(collection.DocumentsLink, sqlQuery, feedOptions).AsDocumentQuery();
            var partitionKeyPath = collection.PartitionKey.GetSelectToken();

            //While there are more results
            while (results.HasMoreResults)
            {
                var tasks = new List<Task>();

                //enumerate and delete the documents in this batch
                foreach (Document doc in await results.ExecuteNextAsync().ConfigureAwait(false))
                {
                    var requestOptions = new RequestOptions();
                    if (partitionKeyPath != null)
                    {
                        requestOptions.PartitionKey = new PartitionKey(doc.GetPartitionKeyValue(partitionKeyPath));
                    }

                    tasks.Add(client.DeleteDocumentAsync(doc.SelfLink, requestOptions));
                    //await client.DeleteDocumentAsync(doc.SelfLink, requestOptions).ConfigureAwait(false);
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        public async Task<DocumentCollection> RecreateCollectionAsync(Connection connection, Database database, DocumentCollection collection)
        {
            throw new Exception("Must redo... ");

            var throughput = await GetThroughputAsync(connection, collection).ConfigureAwait(false);
            
            // Copy StoredProcedure, Triggers, UDFs
            var storeProcedures = await GetStoredProceduresAsync(connection, collection).ConfigureAwait(false);
            var triggers = await GetTriggersAsync(connection, collection).ConfigureAwait(false);
            var udfs = await GetUdfsAsync(connection, collection).ConfigureAwait(false);

            // Delete existing collection
            await DeleteCollectionAsync(connection, collection).ConfigureAwait(false);

            // Create new collection object
            var duplicate = new DocumentCollection
            {
                ConflictResolutionPolicy = collection.ConflictResolutionPolicy,
                DefaultTimeToLive = collection.DefaultTimeToLive,
                Id = collection.Id,
                IndexingPolicy = collection.IndexingPolicy,
                PartitionKey = collection.PartitionKey,
                UniqueKeyPolicy = collection.UniqueKeyPolicy
            };

            var isDatabaseThroughput = false;

            var result = await CreateCollectionAsync(connection, database, duplicate, throughput.Value, isDatabaseThroughput).ConfigureAwait(false);

            // Add StoredProcedure, Triggers, UDFs
            var tasks = new List<Task>();
            tasks.AddRange(storeProcedures.Select(sp => SaveStoredProcedureAsync(connection, result, sp.Id, sp.Body, null)));
            tasks.AddRange(triggers.Select(t => SaveTriggerAsync(connection, result, t.Id, t.Body, t.TriggerType, t.TriggerOperation, null)));
            tasks.AddRange(udfs.Select(udf => SaveUdfAsync(connection, result, udf.Id, udf.Body, null)));

            await Task.WhenAll(tasks);

            return result;
        }

        public async Task<IEnumerable<ResourceResponse<Document>>> DeleteDocumentsAsync(Connection connection, IEnumerable<DocumentDescription> documents)
        {
            var client = GetClient(connection);
            var tasks = documents.Select(doc => DeleteDocumentAsync(client, doc)).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return tasks.Select(t => t.Result);
        }

        private Task<ResourceResponse<Document>> DeleteDocumentAsync(DocumentClient client, DocumentDescription document)
        {
            var options = document.PartitionKey != null
                ? new RequestOptions { PartitionKey = new PartitionKey(document.PartitionKey) }
                : new RequestOptions();

            return client.DeleteDocumentAsync(document.SelfLink, options);
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

        public async Task<int> ImportDocumentAsync(Connection connection, DocumentCollection collection, string content, IHaveRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            var documents = GetDocuments(content).ToList();
            var options = GetRequestOptions(requestOptions);

            foreach (var document in documents)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                await GetClient(connection).UpsertDocumentAsync(collection.SelfLink, document, options, disableAutomaticIdGeneration: true).ConfigureAwait(false);
            }

            return documents.Count;
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

        private IEnumerable<Document> GetDocuments(string content)
        {
            var token = JToken.Parse(content);

            if (token is JArray)
            {
                return token.ToObject<IEnumerable<Document>>();
            }
            else
            {
                return new[] { token.ToObject<Document>() };
            }
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

        public Task UpdateThroughputAsync(Connection connection, Resource resource, int throughput)
        {
            return UpdateOfferThroughput(connection, resource, throughput);
        }

        public async Task<int?> GetThroughputAsync(Connection connection, Resource resource)
        {
            var query = GetClient(connection).CreateOfferQuery().Where(o => o.ResourceLink == resource.SelfLink).AsDocumentQuery();

            var result = await query.ExecuteNextAsync<OfferV2>().ConfigureAwait(false);
            var offer = result.SingleOrDefault();

            return offer?.Content.OfferThroughput;
        }

        public async Task UpdateCollectionSettingsAsync(Connection connection, DocumentCollection collection, int? throughput)
        {
            var tasks = new List<Task>
            {
                GetClient(connection).ReplaceDocumentCollectionAsync(collection),
            };

            if (throughput.HasValue)
            {
                tasks.Add(UpdateOfferThroughput(connection, collection, throughput.Value));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task UpdateOfferThroughput(Connection connection, Resource resource, int throughtput)
        {
            var client = GetClient(connection);
            var query = client.CreateOfferQuery().Where(o => o.ResourceLink == resource.SelfLink).AsDocumentQuery();

            var result = await query.ExecuteNextAsync<OfferV2>().ConfigureAwait(false);
            var offer = result.Single();

            if (offer.Content.OfferThroughput != throughtput)
            {
                offer = new OfferV2(offer, throughtput);
                await client.ReplaceOfferAsync(offer).ConfigureAwait(false);
            }
        }

        public async Task<DocumentCollection> CreateCollectionAsync(Connection connection, Database database, DocumentCollection collection, int throughput, bool isDatabaseThroughput)
        {
            var client = GetClient(connection);
            var options = new RequestOptions { OfferThroughput = throughput };

            if (database.SelfLink == null)
            {
                database = await client.CreateDatabaseIfNotExistsAsync(database, isDatabaseThroughput ? options : null).ConfigureAwait(false);
            }

            return await client.CreateDocumentCollectionAsync(database.SelfLink, collection, isDatabaseThroughput ? null : options).ConfigureAwait(false);
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
