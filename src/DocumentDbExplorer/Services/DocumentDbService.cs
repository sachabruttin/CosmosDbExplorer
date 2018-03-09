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

        public async Task CleanCollection(Connection connection, DocumentCollection collection)
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
                foreach (Document doc in await results.ExecuteNextAsync())
                {
                    var requestOptions = new RequestOptions();
                    if (partitionKey != null)
                    {
                        requestOptions.PartitionKey = new PartitionKey(doc.GetPropertyValue<string>(partitionKey));
                    }

                    await client.DeleteDocumentAsync(doc.SelfLink, requestOptions);
                }
            }
        }

        public async Task<ResourceResponse<Document>> DeleteDocument(Connection connection, DocumentDescription document)
        {
            var options = document.PartitionKey != null
                            ? new RequestOptions { PartitionKey = new PartitionKey(document.PartitionKey) }
                            : new RequestOptions();

            return await GetClient(connection).DeleteDocumentAsync(document.SelfLink, options);
        }

        public async Task<FeedResponse<dynamic>> ExecuteQuery(Connection connection, DocumentCollection collection, string query, IHaveQuerySettings querySettings, string continuationToken, CancellationToken cancellationToken)
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

            return await GetClient(connection)
                                    .CreateDocumentQuery(collection.DocumentsLink, query, feedOptions)                                            
                                    .AsDocumentQuery()
                                    .ExecuteNextAsync(cancellationToken);
        }

        private static Dictionary<Connection, DocumentClient> _clientInstances = new Dictionary<Connection, DocumentClient>();

        public DocumentClient GetClient(Connection connection)
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

        public async Task<List<DocumentCollection>> GetCollections(Connection connection, Database database)
        {
            var client = GetClient(connection);

            var query = client.CreateDocumentCollectionQuery(database.SelfLink).AsDocumentQuery();
            var result = await query.ExecuteNextAsync<DocumentCollection>();

            return result.ToList();
        }

        public async Task<List<Database>> GetDatabases(Connection connection)
        {
            var client = GetClient(connection);
            var query =  client.CreateDatabaseQuery().AsDocumentQuery();
            var result = await query.ExecuteNextAsync<Database>();

            return result.ToList();
        }

        public async Task<ResourceResponse<Document>> GetDocument(Connection connection, DocumentDescription document)
        {

            var options = document.PartitionKey != null
                            ? new RequestOptions { PartitionKey = new PartitionKey(document.PartitionKey) }
                            : new RequestOptions();

            var response = await GetClient(connection).ReadDocumentAsync(document.SelfLink, options);
            return response;
        }

        public async Task<DocumentDescriptionList> GetDocuments(Connection connection, DocumentCollection collection, string filter, int maxItems, string continuationToken)
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

            var result = (await query.ExecuteNextAsync<DocumentDescription>());
            var list = new DocumentDescriptionList(result.Select((doc, index) => doc).ToList())
            {
                ContinuationToken = result.ResponseContinuation,
                CollectionSize = long.Parse(result.CurrentResourceQuotaUsage.Split(new[] { ';' })[2].Split(new[] { '=' })[1]),
                RequestCharge = result.RequestCharge
            };

            return list;
        }

        public async Task<int> ImportDocument(Connection connection, DocumentCollection collection, string content, IHaveRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            var client = GetClient(connection);
            var documents = GetDocuments(content).ToList();

            var options = GetRequestOptions(requestOptions);

            foreach (var document in documents)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                await client.UpsertDocumentAsync(collection.SelfLink, document, options, disableAutomaticIdGeneration: true);
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

        public Task<ResourceResponse<Document>> UpdateDocument(Connection connection, string altLink, string content, IHaveRequestOptions requestOptions)
        {
            var instance = JObject.Parse(content);
            var options = GetRequestOptions(requestOptions);
            return GetClient(connection).UpsertDocumentAsync(altLink, instance, options);
        }

        public async Task<StoredProcedure> SaveStoredProcedure(Connection connection, DocumentCollection collection, string id, string function, string storeProcedureLink)
        {
            var item = new StoredProcedure { Id = id, Body = function };

            if (!string.IsNullOrEmpty(storeProcedureLink))
            {
                var oldId = storeProcedureLink.Split('/').Last();

                if (item.Id != oldId)
                {
                    var itemList = await GetStoredProcedures(connection, collection).ConfigureAwait(false);
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

        public async Task<UserDefinedFunction> SaveUdf(Connection connection, DocumentCollection collection, string id, string function, string altLink)
        {
            var item = new UserDefinedFunction { Id = id, Body = function };
            var client = GetClient(connection);

            if (!string.IsNullOrEmpty(altLink))
            {
                var oldId = altLink.Split('/').Last();

                if (item.Id != oldId)
                {
                    var itemList = await GetUdfs(connection, collection).ConfigureAwait(false);
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

        public Task DeleteStoredProcedure(Connection connection, string storedProcedureLink)
        {
            return GetClient(connection).DeleteStoredProcedureAsync(storedProcedureLink);
        }

        public Task DeleteUdf(Connection connection, string udfLink)
        {
            return GetClient(connection).DeleteUserDefinedFunctionAsync(udfLink);
        }

        public async Task<IList<StoredProcedure>> GetStoredProcedures(Connection connection, DocumentCollection collection)
        {
            var response = await GetClient(connection).ReadStoredProcedureFeedAsync(collection.StoredProceduresLink);
            return response.Select(sp => sp).ToList();
        }

        public async Task<IList<UserDefinedFunction>> GetUdfs(Connection connection, DocumentCollection collection)
        {
            var response = await GetClient(connection).ReadUserDefinedFunctionFeedAsync(collection.UserDefinedFunctionsLink);
            return response.Select(sp => sp).ToList();
        }

        public async Task<IList<Trigger>> GetTriggers(Connection connection, DocumentCollection collection)
        {
            var response = await GetClient(connection).ReadTriggerFeedAsync(collection.TriggersLink);
            return response.Select(sp => sp).ToList();
        }

        public async Task<Trigger> SaveTrigger(Connection connection, DocumentCollection collection, string id, string trigger, TriggerType triggerType, TriggerOperation triggerOperation, string altLink)
        {
            var item = new Trigger { Id = id, Body = trigger, TriggerType = triggerType, TriggerOperation = triggerOperation };
            var client = GetClient(connection);

            if (!string.IsNullOrEmpty(altLink))
            {
                var oldId = altLink.Split('/').Last();

                if (item.Id != oldId)
                {
                    var itemList = await GetTriggers(connection, collection).ConfigureAwait(false);
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

        public Task DeleteTrigger(Connection connection, string triggerLink)
        {
            return GetClient(connection).DeleteTriggerAsync(triggerLink);
        }

        public async Task<int> GetThroughput(Connection connection, DocumentCollection collection)
        {
            var client = GetClient(connection);
            var query = client.CreateOfferQuery().Where(o => o.ResourceLink == collection.SelfLink).AsDocumentQuery();

            var result = await query.ExecuteNextAsync<OfferV2>();
            var offer = result.Single();

            return offer.Content.OfferThroughput;
        }

        public async Task UpdateCollectionSettings(Connection connection, DocumentCollection collection, int throughput)
        {
            await UpdateOfferThroughput(connection, collection, throughput).ConfigureAwait(false);
            await GetClient(connection).ReplaceDocumentCollectionAsync(collection).ConfigureAwait(false);
        }

        private async Task UpdateOfferThroughput(Connection connection, DocumentCollection collection, int throughtput)
        {
            var client = GetClient(connection);
            var query = client.CreateOfferQuery().Where(o => o.ResourceLink == collection.SelfLink).AsDocumentQuery();

            var result = await query.ExecuteNextAsync<OfferV2>();
            var offer = result.Single();

            if (offer.Content.OfferThroughput != throughtput)
            {
                offer = new OfferV2(offer, throughtput);
                await client.ReplaceOfferAsync(offer);
            }
        }

        public async Task<DocumentCollection> CreateCollection(Connection connection, Database database, DocumentCollection collection, int throughput)
        {
            var client = GetClient(connection);

            if (database.SelfLink == null)
            {
                database = await client.CreateDatabaseIfNotExistsAsync(database);
            }

            var response = await client.CreateDocumentCollectionAsync(database.SelfLink, collection, new RequestOptions { OfferThroughput = throughput });
            return response;
        }

        public async Task DeleteCollection(Connection connection, DocumentCollection collection)
        {
            await GetClient(connection).DeleteDocumentCollectionAsync(collection.SelfLink);
        }

        public async Task DeleteDatabase(Connection connection, Database database)
        {
            await GetClient(connection).DeleteDatabaseAsync(database.SelfLink);
        }

        public async Task<IList<User>> GetUsers(Connection connection, Database database)
        {
            var response = await GetClient(connection).ReadUserFeedAsync(database.UsersLink);
            return response.Select(u => u).OrderBy(u => u.Id).ToList();
        }

        public async Task<User> SaveUser(Connection connection, Database database, User user)
        {
            if (user.SelfLink != null)
            {
                var response = await GetClient(connection).ReplaceUserAsync(user);
                return response.Resource;
            }
            else
            {
                var response = await GetClient(connection).CreateUserAsync(database.SelfLink, user);
                return response.Resource;
            }
        }

        public Task DeleteUser(Connection connection, User user)
        {
            return GetClient(connection).DeleteUserAsync(user.SelfLink);
        }

        public async Task<IList<Permission>> GetPermission(Connection connection, User user)
        {
            var response = await GetClient(connection).ReadPermissionFeedAsync(user.PermissionsLink);
            return response.Select(p => p).OrderBy(p => p.Id).ToList();
        }

        public async Task<Permission> SavePermission(Connection connection, User user, Permission permission)
        {
            if (permission.SelfLink != null)
            {
                var response = await GetClient(connection).ReplacePermissionAsync(permission);
                return response.Resource;
            }
            else
            {
                var response = await GetClient(connection).CreatePermissionAsync(user.SelfLink, permission);
                return response;
            }
        }

        public Task DeletePermission(Connection connection, Permission permission)
        {
            return GetClient(connection).DeletePermissionAsync(permission.SelfLink);
        }

        public async Task<int> GetPartitionKeyRangeCount(Connection connection, DocumentCollection collection)
        {
            var client = GetClient(connection);
            FeedResponse<PartitionKeyRange> response;
            var result = 0;

            do
            {
                response = await client.ReadPartitionKeyRangeFeedAsync(collection.SelfLink, new FeedOptions { MaxItemCount = 1000 });

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
