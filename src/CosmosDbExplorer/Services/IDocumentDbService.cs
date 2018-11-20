using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.ViewModel.Interfaces;
using System.Threading;

namespace CosmosDbExplorer.Services
{
    public interface IDocumentDbService
    {
        Task<List<Database>> GetDatabasesAsync(Connection connection);

        Task DeleteDatabaseAsync(Connection connection, Database database);

        Task<List<DocumentCollection>> GetCollectionsAsync(Connection connection, Database database);

        Task<DocumentDescriptionList> GetDocumentsAsync(Connection connection, DocumentCollection collection, string filter, int maxItems, string continuationToken, IHaveRequestOptions requestOptions);

        Task<ResourceResponse<Document>> GetDocumentAsync(Connection connection, DocumentDescription document);

        Task<ResourceResponse<Document>> UpdateDocumentAsync(Connection connection, string altLink, string content, IHaveRequestOptions requestOptions);

        Task<FeedResponse<dynamic>> ExecuteQueryAsync(Connection connection, DocumentCollection collection, string query, IHaveQuerySettings querySettings,  string continuationToken, CancellationToken cancellationToken);

        Task<IEnumerable<ResourceResponse<Document>>> DeleteDocumentsAsync(Connection connection, IEnumerable<DocumentDescription> documents);

        Task CleanCollectionAsync(Connection connection, DocumentCollection collection);

        Task<int> ImportDocumentAsync(Connection connection, DocumentCollection collection, string content, IHaveRequestOptions requestOptions, CancellationToken cancellationToken);

        Task<IList<StoredProcedure>> GetStoredProceduresAsync(Connection connection, DocumentCollection collection);

        Task<StoredProcedure> SaveStoredProcedureAsync(Connection connection, DocumentCollection collection, string id, string function, string storeProcedureLink);

        Task DeleteStoredProcedureAsync(Connection connection, string storedProcedureLink);

        Task<StoredProcedureResponse<dynamic>> ExecuteStoreProcedureAsync(Connection connection, string altLink, IList<dynamic> parameters, string partitionKey);

        Task<IList<UserDefinedFunction>> GetUdfsAsync(Connection connection, DocumentCollection collection);

        Task<UserDefinedFunction> SaveUdfAsync(Connection connection, DocumentCollection collection, string id, string function, string altLink);

        Task DeleteUdfAsync(Connection connection, string udfLink);

        Task<IList<Trigger>> GetTriggersAsync(Connection connection, DocumentCollection collection);

        Task<Trigger> SaveTriggerAsync(Connection connection, DocumentCollection collection, string id, string trigger, TriggerType triggerType, TriggerOperation triggerOperation, string altLink);

        Task DeleteTriggerAsync(Connection connection, string triggerLink);

        Task<int> GetThroughputAsync(Connection connection, DocumentCollection collection);

        Task UpdateCollectionSettingsAsync(Connection connection, DocumentCollection collection, int throughput);

        Task<DocumentCollection> CreateCollectionAsync(Connection connection, Database database, DocumentCollection collection, int throughput);

        Task DeleteCollectionAsync(Connection connection, DocumentCollection collection);

        Task<IList<User>> GetUsersAsync(Connection connection, Database database);

        Task<User> SaveUserAsync(Connection connection, Database database, User user);

        Task DeleteUserAsync(Connection connection, User user);

        Task<IList<Permission>> GetPermissionAsync(Connection connection, User user);

        Task<Permission> SavePermissionAsync(Connection connection, User user, Permission permission);

        Task DeletePermissionAsync(Connection connection, Permission permission);

        Task<int> GetPartitionKeyRangeCountAsync(Connection connection, DocumentCollection collection);

        Task<Dictionary<string, int>> GetTopPartitionKeys(Connection connection, DocumentCollection collection, string partitionKeyRangeId, int sampleCount = 100);

        Task<CollectionMetric> GetPartitionMetricsAsync(Connection connection, DocumentCollection collection);
    }
}
