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
            var results = new List<CosmosDatabase>();

            while (properties.HasMoreResults)
            {
                var response = await properties.ReadNextAsync(cancellationToken);
                results.AddRange(response.Select(properties => new CosmosDatabase(properties)));
            }

            return results;
        }
    }
}
