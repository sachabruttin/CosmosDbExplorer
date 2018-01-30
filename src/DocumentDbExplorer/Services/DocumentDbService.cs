using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json.Linq;
using DocumentDbExplorer.Infrastructure.Models;

namespace DocumentDbExplorer.Services
{
    public interface IDocumentDbService
    {
        Task<List<Database>> GetDatabases(Connection connection);

        Task DeleteDatabase(Connection connection, Database database);

        Task<List<DocumentCollection>> GetCollections(Connection connection, Database database);

        Task<DocumentDescriptionList> GetDocuments(Connection connection, DocumentCollection collection, string filter, int maxItems, string continuationToken);

        Task<ResourceResponse<Document>> GetDocument(Connection connection, DocumentDescription document);

        Task<ResourceResponse<Document>> UpdateDocument(Connection connection, string altLink, string content);

        Task<FeedResponse<dynamic>> ExecuteQuery(Connection connection, DocumentCollection collection, string query, bool? enableCrossPartitionQuery, bool? enableScanInQuery);

        Task<ResourceResponse<Document>> DeleteDocument(Connection connection, string documentLink);

        Task CleanCollection(Connection connection, DocumentCollection collection);

        Task<int> ImportDocument(Connection connection, DocumentCollection collection, string content);

        Task<IList<StoredProcedure>> GetStoredProcedures(Connection connection, DocumentCollection collection);

        Task<StoredProcedure> CreateStoredProcedure(Connection connection, DocumentCollection collection, string id, string function);

        Task DeleteStoredProcedure(Connection connection, string storedProcedureLink);

        Task<IList<UserDefinedFunction>> GetUdfs(Connection connection, DocumentCollection collection);

        Task<UserDefinedFunction> CreateUdf(Connection connection, DocumentCollection collection, string id, string function);

        Task DeleteUdf(Connection connection, string udfLink);

        Task<IList<Trigger>> GetTriggers(Connection connection, DocumentCollection collection);

        Task<Trigger> CreateTrigger(Connection connection, DocumentCollection collection, string id, string trigger, TriggerType triggerType, TriggerOperation triggerOperation);

        Task DeleteTrigger(Connection connection, string triggerLink);

        Task<int> GetThroughput(Connection connection, DocumentCollection collection);

        Task UpdateCollectionSettings(Connection connection, DocumentCollection collection, int throughput);

        Task<DocumentCollection> CreateCollection(Connection connection, Database database, DocumentCollection collection, int throughput);
        Task DeleteCollection(Connection connection, DocumentCollection collection);

        Task<IList<User>> GetUsers(Connection connection, Database database);

        Task<IList<Permission>> GetPermission(Connection connection, User user);
    }

    public class DocumentDescriptionList : List<DocumentDescription>
    {
        public DocumentDescriptionList(IEnumerable<DocumentDescription> collection) 
            : base(collection)
        {

        }

        public bool HasMore
        {
            get { return ContinuationToken != null; }
        }

        public string ContinuationToken { get; set; }

        public long CollectionSize { get; set; }
    }

    public class DocumentDescription
    {
        [JsonConstructor]
        public DocumentDescription(string id, string selfLink, string partitionKey)
        {
            Id = id;
            SelfLink = selfLink;
            PartitionKey = partitionKey;
        }

        public DocumentDescription(Document document, DocumentCollection collection)
        {
            Id = document.Id;
            SelfLink = document.SelfLink;

            var partitionKey = collection.PartitionKey?.Paths.FirstOrDefault();

            if (partitionKey != null)
            {
                PartitionKey = document.GetPropertyValue<string>(partitionKey.TrimStart('/'));
            }
        }

        [JsonProperty(PropertyName ="id")]
        public string Id  { get; set; }

        [JsonProperty(PropertyName = "_self")]
        public string SelfLink { get; set; }

        [JsonProperty(PropertyName = "_partitionKey")]
        public string PartitionKey { get; set; }
    }
    
    public class DocumentDbService : IDocumentDbService
    {
        public async Task CleanCollection(Connection connection, DocumentCollection collection)
        {
            var sqlQuery = "SELECT * FROM c";
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

        public async Task<ResourceResponse<Document>> DeleteDocument(Connection connection, string documentLink)
        {
            return await GetClient(connection).DeleteDocumentAsync(documentLink);
        }

        public async Task<FeedResponse<dynamic>> ExecuteQuery(Connection connection, DocumentCollection collection, string query, bool? enableCrossPartitionQuery, bool? enableScanInQuery)
        {
            var options = new FeedOptions
            {
                EnableCrossPartitionQuery = enableCrossPartitionQuery.GetValueOrDefault(),
                EnableScanInQuery = enableScanInQuery
            };

            var result = await GetClient(connection)
                                    .CreateDocumentQuery(collection.DocumentsLink, query, options)                                            
                                    .AsDocumentQuery()
                                    .ExecuteNextAsync();

            return result;                              
        }

        private static Dictionary<Connection, DocumentClient> _clientInstances = new Dictionary<Connection, DocumentClient>();

        public DocumentClient GetClient(Connection connection)
        {
            if (!_clientInstances.ContainsKey(connection))
            {
                _clientInstances.Add(connection, new DocumentClient(connection.DatabaseUri, connection.AuthenticationKey));
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

            try
            {
                var response = await GetClient(connection).ReadDocumentAsync(document.SelfLink, options);
                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<DocumentDescriptionList> GetDocuments(Connection connection, DocumentCollection collection, string filter, int maxItems, string continuationToken)
        {
            var partitionKey = collection.PartitionKey?.Paths.FirstOrDefault();
            if (partitionKey != null)
            {
                partitionKey = $", c.{partitionKey.TrimStart('/')} as _partitionKey";
            }


            var sql = $"SELECT c.id, c._self {partitionKey} FROM c {filter}";
            var feedOptions = new FeedOptions { MaxItemCount = maxItems, RequestContinuation = continuationToken, EnableCrossPartitionQuery = true };

            var query = GetClient(connection).CreateDocumentQuery<DocumentDescription>(collection.DocumentsLink, sql, feedOptions).AsDocumentQuery();

            var result = (await query.ExecuteNextAsync<DocumentDescription>());
            var list = new DocumentDescriptionList(result.Select((doc, index) => doc).ToList())
            {
                ContinuationToken = result.ResponseContinuation,
                CollectionSize = long.Parse(result.CurrentResourceQuotaUsage.Split(new[] { ';' })[2].Split(new[] { '=' })[1])
            };

            return list;
        }

        public async Task<int> ImportDocument(Connection connection, DocumentCollection collection, string content)
        {
            var client = GetClient(connection);
            var documents = GetDocuments(content).ToList();

            foreach (var document in documents)
            {
                await client.UpsertDocumentAsync(collection.SelfLink, document);
            }

            return documents.Count;
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

        public async Task<ResourceResponse<Document>> UpdateDocument(Connection connection, string altLink, string content)
        {
            //var instance = JsonConvert.DeserializeObject(content);
            var instance = JObject.Parse(content);
            var response = await GetClient(connection).UpsertDocumentAsync(altLink, instance);
            return response;
        }

        public async Task<StoredProcedure> CreateStoredProcedure(Connection connection, DocumentCollection collection, string id, string function)
        {
            var proc = new StoredProcedure { Id = id, Body = function };

            var response = await GetClient(connection).UpsertStoredProcedureAsync(collection.SelfLink, proc);
            return response.Resource;
        }

        public async Task<UserDefinedFunction> CreateUdf(Connection connection, DocumentCollection collection, string id, string function)
        {
            var proc = new UserDefinedFunction { Id = id, Body = function };

            var response = await GetClient(connection).UpsertUserDefinedFunctionAsync(collection.SelfLink, proc);
            return response.Resource;
        }

        public async Task DeleteStoredProcedure(Connection connection, string storedProcedureLink)
        {
            await GetClient(connection).DeleteStoredProcedureAsync(storedProcedureLink);
        }

        public async Task DeleteUdf(Connection connection, string udfLink)
        {
            await GetClient(connection).DeleteUserDefinedFunctionAsync(udfLink);
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

        public async Task<Trigger> CreateTrigger(Connection connection, DocumentCollection collection, string id, string trigger, TriggerType triggerType, TriggerOperation triggerOperation)
        {
            var item = new Trigger { Id = id, Body = trigger, TriggerType = triggerType, TriggerOperation = triggerOperation };

            var response = await GetClient(connection).UpsertTriggerAsync(collection.SelfLink, item);
            return response.Resource;
        }

        public async Task DeleteTrigger(Connection connection, string triggerLink)
        {
            await GetClient(connection).DeleteTriggerAsync(triggerLink);
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
            await UpdateOfferThroughput(connection, collection, throughput);
            var client = GetClient(connection);
            await client.ReplaceDocumentCollectionAsync(collection);
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

        public async Task<IList<Permission>> GetPermission(Connection connection, User user)
        {
            var response = await GetClient(connection).ReadPermissionFeedAsync(user.PermissionsLink);
            return response.Select(p => p).OrderBy(p => p.Id).ToList();
        }
    }
}
