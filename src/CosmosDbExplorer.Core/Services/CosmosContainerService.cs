using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Helpers;
using CosmosDbExplorer.Core.Models;
using Microsoft.Azure.Cosmos;

using Newtonsoft.Json;

namespace CosmosDbExplorer.Core.Services
{
    public class CosmosContainerService(ICosmosClientService clientService, CosmosConnection connection, CosmosDatabase cosmosDatabase) : ICosmosContainerService
    {
        private readonly CosmosClient _client = clientService.GetClient(connection);
        private readonly CosmosDatabase _cosmosDatabase = cosmosDatabase;

        public async Task<IList<CosmosContainer>> GetContainersAsync(CancellationToken cancellationToken)
        {
            var db = _client.GetDatabase(_cosmosDatabase.Id);
            var properties = db.GetContainerQueryIterator<ContainerProperties>();
            var result = new List<CosmosContainer>();

            while (properties.HasMoreResults)
            {
                var response = await properties.ReadNextAsync(cancellationToken);
                result.AddRange(response.Select(properties => new CosmosContainer(properties)));
            }

            return result.OrderBy(r => r.Id).ToList();
        }

        public async Task<CosmosContainer> CreateContainerAsync(CosmosContainer container, int? throughput, bool? isAutoscale, CancellationToken cancellationToken)
        {
            var db = _client.GetDatabase(_cosmosDatabase.Id);

            var containerProperties = new ContainerProperties()
            {
                Id = container.Id,
                // TODO: PartitionKeyPath = container.PartitionKeyPath,
                DefaultTimeToLive = container.DefaultTimeToLive,
                PartitionKeyDefinitionVersion = container.PartitionKeyDefVersion
            };

            try
            {
                if (throughput.HasValue)
                {
                    var throughputProperties = isAutoscale.GetValueOrDefault(true)
                        ? ThroughputProperties.CreateManualThroughput(throughput.Value)
                        : ThroughputProperties.CreateAutoscaleThroughput(throughput.Value);

                    var result = await db.CreateContainerAsync(containerProperties, throughputProperties, requestOptions: null, cancellationToken);
                    return new CosmosContainer(result.Resource);
                }
                else
                {
                    var result = await db.CreateContainerAsync(containerProperties, throughput, requestOptions: null, cancellationToken);
                    return new CosmosContainer(result.Resource);
                }
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage());
            }
        }

        public async Task<CosmosContainer> UpdateContainerAsync(CosmosContainer container)
        {
            var ct = _client.GetContainer(_cosmosDatabase.Id, container.Id);
            var containerProperties = new ContainerProperties
            {
                Id = container.Id,
                // TODO: PartitionKeyPath = container.PartitionKeyPath,
                DefaultTimeToLive = container.DefaultTimeToLive,
                PartitionKeyDefinitionVersion = container.PartitionKeyDefVersion,
                IndexingPolicy = JsonConvert.DeserializeObject<IndexingPolicy>(container.IndexingPolicy),
                GeospatialConfig = new GeospatialConfig(container.GeospatialType.FromLocalType())
            };

            try
            {
                var result = await ct.ReplaceContainerAsync(containerProperties);
                return new CosmosContainer(result.Resource);
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage());
            }
        }

        public async Task<CosmosThroughput?> GetThroughputAsync(CosmosContainer container)
        {
            try
            {
                var ct = _client.GetContainer(_cosmosDatabase.Id, container.Id);
                var response = await ct.ReadThroughputAsync(requestOptions: null);

                return new CosmosThroughput(response);
            }
            catch (CosmosException ce) when (ce.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<CosmosThroughput> UpdateThroughputAsync(CosmosContainer container, int throughput, bool isAutoscale)
        {
            try
            {
                var ct = _client.GetContainer(_cosmosDatabase.Id, container.Id);

                var properties = isAutoscale
                    ? ThroughputProperties.CreateAutoscaleThroughput(throughput)
                    : ThroughputProperties.CreateManualThroughput(throughput);

                var result = await ct.ReplaceThroughputAsync(properties);
                return new CosmosThroughput(result);
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage(), ex);
            }
        }


        public async Task DeleteContainserAsync(CosmosContainer container, CancellationToken cancellationToken)
        {
            var client = _client.GetDatabase(_cosmosDatabase.Id).GetContainer(container.Id);

            try
            {
                await client.DeleteContainerAsync(cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage());
            }
        }

        public async Task<CosmosContainerMetric> GetContainerMetricsAsync(CosmosContainer container, CancellationToken cancellationToken)
        {
            var ctx = _client.GetContainer(_cosmosDatabase.Id, container.Id);
            var options = new ContainerRequestOptions
            {
                PopulateQuotaInfo = true,
            };

            var response = await ctx.ReadContainerAsync(options, cancellationToken);
            return new CosmosContainerMetric(response);
        }
    }
}
