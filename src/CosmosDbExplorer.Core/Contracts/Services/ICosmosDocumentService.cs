using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Models;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Core.Contracts.Services
{
    public interface ICosmosDocumentService
    {
        Task<CosmosResult> DeleteDocumentsAsync(IEnumerable<ICosmosDocument> documents, CancellationToken cancellationToken);
        Task<CosmosQueryResult<IReadOnlyCollection<JToken>>> ExecuteQueryAsync(ICosmosQuery query, CancellationToken cancellationToken);
        Task<CosmosQueryResult<JObject>> GetDocumentAsync(ICosmosDocument document, IDocumentRequestOptions options, CancellationToken cancellation);
        Task<CosmosQueryResult<IReadOnlyCollection<ICosmosDocument>>> GetDocumentsAsync(string? filter, int? maxItemsCount, string? continuationToken, CancellationToken cancellationToken);
        Task<CosmosQueryResult<JObject>> SaveDocumentAsync(string content, IDocumentRequestOptions options, CancellationToken cancellation);
    }
}
