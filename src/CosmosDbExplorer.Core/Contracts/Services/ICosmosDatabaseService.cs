using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Core.Contracts.Services
{
    public interface ICosmosDatabaseService
    {
        Task<IList<CosmosDatabase>> GetDatabasesAsync(CancellationToken cancellationToken);
    }
}
