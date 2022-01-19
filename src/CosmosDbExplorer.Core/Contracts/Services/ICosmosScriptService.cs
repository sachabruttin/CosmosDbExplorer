using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Core.Contracts.Services
{
    public interface ICosmosScriptService
    {
        Task<IReadOnlyList<CosmosStoredProcedure>> GetStoredProceduresAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<CosmosTrigger>> GetTriggersAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<CosmosUserDefinedFunction>> GetUserDefinedFunctionsAsync(CancellationToken cancellationToken);
    }
}
