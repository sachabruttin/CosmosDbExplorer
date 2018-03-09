using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json.Linq;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.ViewModel.Interfaces;
using GalaSoft.MvvmLight.Messaging;
using DocumentDbExplorer.Messages;
using System.Threading;

namespace DocumentDbExplorer.Services
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
            var partitionKey = collection.PartitionKey?.Paths.FirstOrDefault()?.TrimStart('/');

            //While there are more results
            while (results.HasMoreResults)
            {
                //enumerate and delete the documents in this batch
                foreach (Document doc in await results.ExecuteNextAsync().ConfigureAwait(false))
                {
                    var requestOptions = new RequestOptions();
                    if (partitionKey != null)
                    {
                        requestOptions.PartitionKey = new PartitionKey(doc.GetPropertyValue<string>(partitionKey));
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

        public async Task<DocumentDescriptionList> GetDocumentsAsync(Connection connection, DocumentCollection collection, string filter, int maxItems, string continuationToken)
        {
            var partitionKey = collection.PartitionKey?.Paths.FirstOrDefault();
            if (partitionKey != null)
            {
                partitionKey = $", c.{partitionKey.TrimStart('/')} as _partitionKey";
            }

            var sql = $"SELECT c.id, c._self {partitionKey} FROM c {filter}";
            var feedOptions = new FeedOptions
            {
                MaxItemCount = maxItems,
                RequestContinuation = continuationToken,
                EnableCrossPartitionQuery = true,
                EnableScanInQuery = true
            };

            var query = GetClient(connection).CreateDocumentQuery<DocumentDescription>(collection.DocumentsLink, sql, feedOptions).AsDocumentQuery();
            var result = await query.ExecuteNextAsync<DocumentDescription>().ConfigureAwait(false);

            return new DocumentDescriptionList(result.Select(doc => doc).ToList())
            {
                ContinuationToken = result.ResponseContinuation,
                CollectionSize = long.Parse(result.CurrentResourceQuotaUsage.Split(new[] { ';' })[2].Split(new[] { '=' })[1]),
                RequestCharge = result.RequestCharge
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
            var options = new RequestOptions
            {
                ConsistencyLevel = request.ConsistencyLevel,
                IndexingDirective = request.IndexingDirective,
                PreTriggerInclude = request.PreTrigger != null ? new List<string> { request.PreTrigger } : null,
                PostTriggerInclude = request.PreTrigger != null ? new List<string> { request.PostTrigger } : null,
                PartitionKey = request.PartitionKey != null ? new PartitionKey(request.PartitionKey) : null,
                AccessCondition = request.AccessConditionType != null ? new AccessCondition { Condition = request.AccessCondition, Type = request.AccessConditionType.Value } : null,
            };

            return options;
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

        public async Task<int> GetThroughputAsync(Connection connection, DocumentCollection collection)
        {
            var query = GetClient(connection).CreateOfferQuery().Where(o => o.ResourceLink == collection.SelfLink).AsDocumentQuery();

            var result = await query.ExecuteNextAsync<OfferV2>().ConfigureAwait(false);
            var offer = result.Single();

            return offer.Content.OfferThroughput;
        }

        public async Task UpdateCollectionSettingsAsync(Connection connection, DocumentCollection collection, int throughput)
        {
            await UpdateOfferThroughput(connection, collection, throughput).ConfigureAwait(false);
            await GetClient(connection).ReplaceDocumentCollectionAsync(collection).ConfigureAwait(false);
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

            if (database.SelfLink == null)
            {
                database = await client.CreateDatabaseIfNotExistsAsync(database).ConfigureAwait(false);
            }

            return await client.CreateDocumentCollectionAsync(database.SelfLink, collection, new RequestOptions { OfferThroughput = throughput }).ConfigureAwait(false);
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
            FeedResponse<PartitionKeyRange> response;
            var result = 0;

            do
            {
                response = await GetClient(connection).ReadPartitionKeyRangeFeedAsync(collection.SelfLink, new FeedOptions { MaxItemCount = 1000 }).ConfigureAwait(false);

                foreach (var item in response)
                {
                    result++;
                }
            }
            while (!string.IsNullOrEmpty(response.ResponseContinuation));

            return result;
        }
    }
}
