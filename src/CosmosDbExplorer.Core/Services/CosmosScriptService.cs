using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Helpers;
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

        public async Task<CosmosStoredProcedure> SaveStoredProcedureAsync(CosmosStoredProcedure asset)
        {
            var properties = new StoredProcedureProperties(asset.Id, asset.Body);

            try
            {
                if (asset.SelfLink != null)
                {
                    var updatedobject = await _container.Scripts.ReplaceStoredProcedureAsync(properties);
                    return new CosmosStoredProcedure(updatedobject);
                }
                else
                {
                    var newobject = await _container.Scripts.CreateStoredProcedureAsync(properties);
                    return new CosmosStoredProcedure(newobject);
                }
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage());
            }
        }

        public async Task<CosmosResult> DeleteStoredProcedureAsync(CosmosStoredProcedure asset)
        {
            try
            {
                var response = await _container.Scripts.DeleteStoredProcedureAsync(asset.Id);

                return new CosmosResult
                {
                    RequestCharge = response.RequestCharge,
                    TimeElapsed = response.Diagnostics.GetClientElapsedTime()
                };
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage());
            }
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

        public async Task<CosmosUserDefinedFunction> SaveUserDefinedFunctionAsync(CosmosUserDefinedFunction asset)
        {
            var properties = new UserDefinedFunctionProperties
            {
                Body = asset.Body,
                Id = asset.Id
            };

            try
            {
                if (asset.SelfLink != null)
                {
                    var updatedobject = await _container.Scripts.ReplaceUserDefinedFunctionAsync(properties);
                    return new CosmosUserDefinedFunction(updatedobject);
                }
                else
                {
                    var newobject = await _container.Scripts.CreateUserDefinedFunctionAsync(properties);
                    return new CosmosUserDefinedFunction(newobject);
                }
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage());
            }
        }

        public async Task<CosmosResult> DeleteUserDefinedFunctionAsync(CosmosUserDefinedFunction asset)
        {
            try
            {
                var response = await _container.Scripts.DeleteUserDefinedFunctionAsync(asset.Id);

                return new CosmosResult
                {
                    RequestCharge = response.RequestCharge,
                    TimeElapsed = response.Diagnostics.GetClientElapsedTime()
                };
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage());
            }
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

        public async Task<CosmosTrigger> SaveTriggerAsync(CosmosTrigger asset)
        {
            var properties = new TriggerProperties
            {
                Body = asset.Body,
                Id = asset.Id,
                TriggerOperation = TriggerOperation.All,
                TriggerType = TriggerType.Pre
            };

            try
            {
                if (asset.SelfLink != null)
                {
                    var updatedobject = await _container.Scripts.ReplaceTriggerAsync(properties);
                    return new CosmosTrigger(updatedobject);
                }
                else
                {
                    var newobject = await _container.Scripts.CreateTriggerAsync(properties);
                    return new CosmosTrigger(newobject);
                }
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage());
            }
        }

        public async Task<CosmosResult> DeleteTriggerAsync(CosmosTrigger asset)
        {
            try
            {
                var response = await _container.Scripts.DeleteTriggerAsync(asset.Id);

                return new CosmosResult
                {
                    RequestCharge = response.RequestCharge,
                    TimeElapsed = response.Diagnostics.GetClientElapsedTime()
                };
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage());
            }
        }
    }
}
