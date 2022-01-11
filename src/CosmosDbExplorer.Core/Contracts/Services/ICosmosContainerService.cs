using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Core.Contracts.Services
{
    public interface ICosmosContainerService
    {
        Task<IList<CosmosContainer>> GetContainersAsync(CancellationToken cancellationToken);
    }
}
