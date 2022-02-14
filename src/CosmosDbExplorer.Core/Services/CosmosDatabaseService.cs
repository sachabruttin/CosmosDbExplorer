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

namespace CosmosDbExplorer.Core.Services
{
    public class CosmosDatabaseService : ICosmosDatabaseService
    {
        private readonly CosmosClient _client;

        public CosmosDatabaseService(ICosmosClientService clientService, CosmosConnection connection)
        {
            _client = clientService.GetClient(connection);
        }

        public async Task<IList<CosmosDatabase>> GetDatabasesAsync(CancellationToken cancellationToken)
        {
            var properties = _client.GetDatabaseQueryIterator<DatabaseProperties>();
            var result = new List<CosmosDatabase>();

            while (properties.HasMoreResults)
            {
                var response = await properties.ReadNextAsync(cancellationToken);

                foreach (var item in response)
                {
                    var db = _client.GetDatabase(item.Id);
                    var throughput = await db.ReadThroughputAsync(cancellationToken);

                    result.Add(new CosmosDatabase(item, throughput));
                }
            }

            return result;
        }

        public async Task<CosmosDatabase> CreateDatabaseAsync(CosmosDatabase database, int? throughput, bool? isAutoscale, CancellationToken cancellationToken)
        {
            try
            {
                if (throughput.HasValue)
                {
                    var throughputProperties = isAutoscale.GetValueOrDefault(true)
                        ? ThroughputProperties.CreateAutoscaleThroughput(throughput.Value)
                        : ThroughputProperties.CreateManualThroughput(throughput.Value);

                    var result = await _client.CreateDatabaseAsync(database.Id, throughputProperties, requestOptions: null, cancellationToken: cancellationToken);
                    return new CosmosDatabase(result.Resource, throughput);
                }
                else
                {
                    var result = await _client.CreateDatabaseAsync(database.Id, throughput, requestOptions: null, cancellationToken: cancellationToken);
                    return new CosmosDatabase(result.Resource, throughput);
                }
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage());
            }
        }

        public async Task DeleteDatabaseAsync(CosmosDatabase database, CancellationToken cancellationToken)
        {
            var db = _client.GetDatabase(database.Id);
            
            try
            {
                await db.DeleteAsync(cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage());
            }
        }

        public async Task<AccountProperties> GetDatabaseMetricsAsync()
        {
            return await _client.ReadAccountAsync();
        }

        public async Task<CosmosThroughput> GetThroughputAsync(CosmosDatabase database)
        {
            var db = _client.GetDatabase(database.Id);
            var result = await db.ReadThroughputAsync(requestOptions: null);
            return new CosmosThroughput(result);
        }
    }
}
