using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Core.Contracts.Services
{
    public interface ICosmosUserService
    {
        Task<List<CosmosUser>> GetUsersAsync(CancellationToken cancellationToken);
    }
}
