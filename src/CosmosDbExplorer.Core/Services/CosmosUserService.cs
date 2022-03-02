using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Models;

using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Services
{
    public class CosmosUserService : ICosmosUserService
    {
        private readonly Database _database;

        public CosmosUserService(ICosmosClientService clientService, CosmosConnection connection, CosmosDatabase database)
        {
            var client = clientService.GetClient(connection);
            _database = client.GetDatabase(database.Id);
        }

        public async Task<List<CosmosUser>> GetUsersAsync(CancellationToken cancellationToken)
        {
            var properties = _database.GetUserQueryIterator<UserProperties>();
            var result = new List<CosmosUser>();

            while (properties.HasMoreResults)
            {
                var response = await properties.ReadNextAsync(cancellationToken);

                result.AddRange(response.Select(properties => new CosmosUser(properties)));
            }

            return result;
        }

        public async Task<CosmosQueryResult<CosmosUser>> GetUserAsync(string id, CancellationToken cancellationToken)
        {
            var userCtx = _database.GetUser(id);
            var response = await userCtx.ReadAsync(cancellationToken: cancellationToken);

            return new CosmosQueryResult<CosmosUser>
            {
                RequestCharge = response.RequestCharge,
                Headers = response.Headers.AllKeys().ToDictionary(key => key, key => response.Headers.GetValueOrDefault(key)),
                Items = new CosmosUser(response.Resource)
            };
        }

        public Task<CosmosQueryResult<CosmosUser>> SaveUserAsync(CosmosUser user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<CosmosResult> DeleteUserAsync(CosmosUser user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<CosmosPermission>> GetPermissionsAsync(CosmosUser user, CancellationToken cancellationToken)
        {
            var userCtx = _database.GetUser(user.Id);
            var properties = userCtx.GetPermissionQueryIterator<PermissionProperties>();
            var result = new List<CosmosPermission>();

            while (properties.HasMoreResults)
            {
                var response = await properties.ReadNextAsync(cancellationToken);

                result.AddRange(response.Select(properties => new CosmosPermission(properties)));
            }

            return result;
        }

        public Task<CosmosQueryResult<CosmosPermission>> SavePermissionAsync(CosmosUser user, CosmosPermission permission, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<CosmosResult> DeletePermissionAsync(CosmosPermission permission, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
