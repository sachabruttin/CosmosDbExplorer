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
        Task<CosmosContainer> CreateContainerAsync(CosmosContainer container, int? throughput, bool? isAutoscale, CancellationToken cancellationToken);
        Task DeleteContainserAsync(CosmosContainer container, CancellationToken cancellationToken);
        Task<CosmosContainerMetric> GetContainerMetricsAsync(CosmosContainer container, CancellationToken cancellationToken);
        Task<IList<CosmosContainer>> GetContainersAsync(CancellationToken cancellationToken);
    }
}
