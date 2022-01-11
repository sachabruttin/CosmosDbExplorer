using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Services
{
    public class CosmosContainerService : ICosmosContainerService
    {
        private readonly CosmosClient _client;
        private readonly CosmosDatabase _cosmosDatabase;

        public CosmosContainerService(ICosmosClientService clientService, CosmosConnection connection, CosmosDatabase cosmosDatabase)
        {
            _client = clientService.GetClient(connection);
            _cosmosDatabase = cosmosDatabase;
        }

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

            return result;
        }
    }
}
