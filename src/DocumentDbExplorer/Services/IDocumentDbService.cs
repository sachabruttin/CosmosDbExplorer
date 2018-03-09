using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.ViewModel.Interfaces;
using System.Threading;

namespace DocumentDbExplorer.Services
{
    public interface IDocumentDbService
    {
        Task<List<Database>> GetDatabases(Connection connection);

        Task DeleteDatabase(Connection connection, Database database);

        Task<List<DocumentCollection>> GetCollections(Connection connection, Database database);

        Task<DocumentDescriptionList> GetDocuments(Connection connection, DocumentCollection collection, string filter, int maxItems, string continuationToken);

        Task<ResourceResponse<Document>> GetDocument(Connection connection, DocumentDescription document);

        Task<ResourceResponse<Document>> UpdateDocument(Connection connection, string altLink, string content, IHaveRequestOptions requestOptions);

        Task<FeedResponse<dynamic>> ExecuteQuery(Connection connection, DocumentCollection collection, string query, IHaveQuerySettings querySettings,  string continuationToken, CancellationToken cancellationToken);

        Task<ResourceResponse<Document>> DeleteDocument(Connection connection, DocumentDescription document);

        Task CleanCollection(Connection connection, DocumentCollection collection);

        Task<int> ImportDocument(Connection connection, DocumentCollection collection, string content, IHaveRequestOptions requestOptions, CancellationToken cancellationToken);

        Task<IList<StoredProcedure>> GetStoredProcedures(Connection connection, DocumentCollection collection);

        Task<StoredProcedure> SaveStoredProcedure(Connection connection, DocumentCollection collection, string id, string function, string storeProcedureLink);

        Task DeleteStoredProcedure(Connection connection, string storedProcedureLink);

        Task<IList<UserDefinedFunction>> GetUdfs(Connection connection, DocumentCollection collection);

        Task<UserDefinedFunction> SaveUdf(Connection connection, DocumentCollection collection, string id, string function, string altLink);

        Task DeleteUdf(Connection connection, string udfLink);

        Task<IList<Trigger>> GetTriggers(Connection connection, DocumentCollection collection);

        Task<Trigger> SaveTrigger(Connection connection, DocumentCollection collection, string id, string trigger, TriggerType triggerType, TriggerOperation triggerOperation, string altLink);

        Task DeleteTrigger(Connection connection, string triggerLink);

        Task<int> GetThroughput(Connection connection, DocumentCollection collection);

        Task UpdateCollectionSettings(Connection connection, DocumentCollection collection, int throughput);

        Task<DocumentCollection> CreateCollection(Connection connection, Database database, DocumentCollection collection, int throughput);

        Task DeleteCollection(Connection connection, DocumentCollection collection);

        Task<IList<User>> GetUsers(Connection connection, Database database);

        Task<User> SaveUser(Connection connection, Database database, User user);

        Task DeleteUser(Connection connection, User user);

        Task<IList<Permission>> GetPermission(Connection connection, User user);

        Task<Permission> SavePermission(Connection connection, User user, Permission permission);

        Task DeletePermission(Connection connection, Permission permission);

        Task<int> GetPartitionKeyRangeCount(Connection connection, DocumentCollection collection);
    }
}
