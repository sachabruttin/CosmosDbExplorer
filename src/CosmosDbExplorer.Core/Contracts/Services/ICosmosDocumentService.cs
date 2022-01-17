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
        Task<CosmosQueryResult<IReadOnlyCollection<JObject>>> ExecuteQueryAsync(ICosmosQuery query, CancellationToken cancellationToken);
        Task<CosmosQueryResult<JObject>> GetDocumentAsync(ICosmosDocument document, CancellationToken cancellation);
        Task<CosmosQueryResult<IReadOnlyCollection<ICosmosDocument>>> GetDocumentsAsync(string? filter, int? maxItemsCount, string? continuationToken, CancellationToken cancellationToken);
    }
}
