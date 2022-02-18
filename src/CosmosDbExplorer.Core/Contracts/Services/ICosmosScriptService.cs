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
        Task<CosmosResult> DeleteStoredProcedureAsync(CosmosStoredProcedure asset);
        Task<CosmosResult> DeleteTriggerAsync(CosmosTrigger asset);
        Task<CosmosResult> DeleteUserDefinedFunctionAsync(CosmosUserDefinedFunction asset);
        Task<CosmosStoredProcedureResult> ExecuteStoredProcedureAsync(string storedProcedureId, object partitionKey, dynamic[] parameters);
        Task<IReadOnlyList<CosmosStoredProcedure>> GetStoredProceduresAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<CosmosTrigger>> GetTriggersAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<CosmosUserDefinedFunction>> GetUserDefinedFunctionsAsync(CancellationToken cancellationToken);
        Task<CosmosStoredProcedure> SaveStoredProcedureAsync(CosmosStoredProcedure asset);
        Task<CosmosTrigger> SaveTriggerAsync(CosmosTrigger asset);
        Task<CosmosUserDefinedFunction> SaveUserDefinedFunctionAsync(CosmosUserDefinedFunction asset);
    }
}
