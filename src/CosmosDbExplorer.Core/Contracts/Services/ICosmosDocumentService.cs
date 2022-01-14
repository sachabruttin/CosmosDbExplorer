using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Core.Contracts.Services
{
    public interface ICosmosDocumentService
    {
        Task<ICosmosDocument> GetAsync(string id, CancellationToken cancellationToken);
        Task<CosmosQueryResult> ReadAllItem(string? filter, int? maxItemsCount, string? continuationToken, CancellationToken cancellationToken);
    }
}
