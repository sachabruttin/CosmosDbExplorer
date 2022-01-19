using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;

namespace CosmosDbExplorer.Core.Services
{
    public class CosmosScriptService : ICosmosScriptService
    {
        private readonly CosmosDatabase _cosmosDatabase;
        private readonly CosmosContainer _cosmosContainer;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public CosmosScriptService(ICosmosClientService clientService, CosmosConnection connection, CosmosDatabase database, CosmosContainer container)
        {
            _cosmosClient = clientService.GetClient(connection);
            _cosmosDatabase = database;
            _cosmosContainer = container;

            _container = _cosmosClient.GetContainer(_cosmosDatabase.Id, _cosmosContainer.Id);
        }

        public async Task<IReadOnlyList<CosmosStoredProcedure>> GetStoredProceduresAsync(CancellationToken cancellationToken)
        {
            var iterator = _container.Scripts.GetStoredProcedureQueryIterator<StoredProcedureProperties>();
            var result = new List<CosmosStoredProcedure>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                result.AddRange(response.Select(r => new CosmosStoredProcedure(r)));
            }

            return result;
        }

        public async Task<IReadOnlyList<CosmosUserDefinedFunction>> GetUserDefinedFunctionsAsync(CancellationToken cancellationToken)
        {
            var iterator = _container.Scripts.GetUserDefinedFunctionQueryIterator<UserDefinedFunctionProperties>();
            var result = new List<CosmosUserDefinedFunction>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                result.AddRange(response.Select(r => new CosmosUserDefinedFunction(r)));
            }

            return result;
        }

        public async Task<IReadOnlyList<CosmosTrigger>> GetTriggersAsync(CancellationToken cancellationToken)
        {
            var iterator = _container.Scripts.GetTriggerQueryIterator<TriggerProperties>();
            var result = new List<CosmosTrigger>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                result.AddRange(response.Select(r => new CosmosTrigger(r)));
            }

            return result;
        }
    }
}
