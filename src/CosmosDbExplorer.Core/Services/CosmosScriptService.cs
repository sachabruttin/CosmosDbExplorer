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
        //private readonly CosmosDatabase _cosmosDatabase;
        //private readonly CosmosContainer _cosmosContainer;
        //private readonly CosmosClient _cosmosClient;
        //private readonly Container _container;

        private readonly Scripts _scripts;

        public CosmosScriptService(ICosmosClientService clientService, CosmosConnection connection, CosmosDatabase database, CosmosContainer container)
        {
            var cosmosClient = clientService.GetClient(connection);

            _scripts = cosmosClient.GetContainer(database.Id, container.Id).Scripts;
        }

        public async Task<IReadOnlyList<CosmosStoredProcedure>> GetStoredProceduresAsync(CancellationToken cancellationToken)
        {
            var iterator = _scripts.GetStoredProcedureQueryIterator<StoredProcedureProperties>();
            var result = new List<CosmosStoredProcedure>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                result.AddRange(response.OrderBy(r => r.Id).Select(r => new CosmosStoredProcedure(r)));
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
                    var updatedobject = await _scripts.ReplaceStoredProcedureAsync(properties);
                    return new CosmosStoredProcedure(updatedobject);
                }
                else
                {
                    var newobject = await _scripts.CreateStoredProcedureAsync(properties);
                    return new CosmosStoredProcedure(newobject);
                }
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage());
            }
        }

        public async Task<CosmosStoredProcedureResult> ExecuteStoredProcedureAsync(string storedProcedureId, string? partitionKey, dynamic[] parameters)
        {
            var pk = PartitionKeyHelper.Parse(partitionKey) ?? PartitionKey.None;

            var options = new StoredProcedureRequestOptions
            {
                EnableScriptLogging = true,
            };

            try
            {
                var response = await _scripts.ExecuteStoredProcedureAsync<string>(storedProcedureId, pk, parameters, options);
                return new CosmosStoredProcedureResult(response);
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
                var response = await _scripts.DeleteStoredProcedureAsync(asset.Id);

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
            var iterator = _scripts.GetUserDefinedFunctionQueryIterator<UserDefinedFunctionProperties>();
            var result = new List<CosmosUserDefinedFunction>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                result.AddRange(response.OrderBy(r => r.Id).Select(r => new CosmosUserDefinedFunction(r)));
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
                    var updatedobject = await _scripts.ReplaceUserDefinedFunctionAsync(properties);
                    return new CosmosUserDefinedFunction(updatedobject);
                }
                else
                {
                    var newobject = await _scripts.CreateUserDefinedFunctionAsync(properties);
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
                var response = await _scripts.DeleteUserDefinedFunctionAsync(asset.Id);

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
            var iterator = _scripts.GetTriggerQueryIterator<TriggerProperties>();
            var result = new List<CosmosTrigger>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                result.AddRange(response.OrderBy(r => r.Id).Select(r => new CosmosTrigger(r)));
            }

            return result;
        }

        public async Task<CosmosTrigger> SaveTriggerAsync(CosmosTrigger asset)
        {
            var properties = new TriggerProperties
            {
                Body = asset.Body,
                Id = asset.Id,
                TriggerOperation = (TriggerOperation)(int)asset.Operation,
                TriggerType = (TriggerType)(int)asset.Type
            };

            try
            {
                if (asset.SelfLink != null)
                {
                    var updatedobject = await _scripts.ReplaceTriggerAsync(properties);
                    return new CosmosTrigger(updatedobject);
                }
                else
                {
                    var newobject = await _scripts.CreateTriggerAsync(properties);
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
                var response = await _scripts.DeleteTriggerAsync(asset.Id);

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
