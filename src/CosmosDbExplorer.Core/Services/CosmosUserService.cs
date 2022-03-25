using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Helpers;
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

            return result.OrderBy(r => r.Id).ToList();
        }

        public async Task<CosmosQueryResult<CosmosUser>> GetUserAsync(string id, CancellationToken cancellationToken)
        {
            var userCtx = _database.GetUser(id);
            var response = await userCtx.ReadAsync(cancellationToken: cancellationToken);

            return new CosmosQueryResult<CosmosUser>
            {
                RequestCharge = response.RequestCharge,
                Headers = response.Headers.ToDictionary(),
                Items = new CosmosUser(response.Resource)
            };
        }

        public async Task<CosmosQueryResult<CosmosUser>> SaveUserAsync(CosmosUser user, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _database.CreateUserAsync(user.Id, cancellationToken: cancellationToken);

                return new CosmosQueryResult<CosmosUser>
                {
                    RequestCharge = response.RequestCharge,
                    Headers = response.Headers.ToDictionary(),
                    Items = new CosmosUser(response.Resource)
                };
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage(), ex);
            }
        }

        public async Task<CosmosResult> DeleteUserAsync(CosmosUser user, CancellationToken cancellationToken)
        {
            var userCtx = _database.GetUser(user.Id);
            var response = await userCtx.DeleteAsync(cancellationToken: cancellationToken);

            return new CosmosResult
            {
                RequestCharge = response.RequestCharge,
            };
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

            return result.OrderBy(r => r.Id).ToList();
        }

        public async Task<CosmosQueryResult<CosmosPermission>> SavePermissionAsync(CosmosUser user, CosmosPermission permission, string container, CancellationToken cancellationToken)
        {
            try
            {
                var userCtx = _database.GetUser(user.Id);

                var permissionMode = (PermissionMode)permission.PermissionMode;
                var pk = PartitionKeyHelper.Parse(permission.PartitionKey);
                var ct = _database.GetContainer(container);

                var permissionProperties = new PermissionProperties(permission.Id, permissionMode, ct, pk);
                var response = await userCtx.UpsertPermissionAsync(permissionProperties, cancellationToken: cancellationToken);

                return new CosmosQueryResult<CosmosPermission>
                {
                    RequestCharge = response.RequestCharge,
                    Headers = response.Headers.ToDictionary(),
                    Items = new CosmosPermission(response.Resource)
                };
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage(), ex);
            }
        }

        public async Task<CosmosResult> DeletePermissionAsync(CosmosUser user, CosmosPermission permission, CancellationToken cancellationToken)
        {
            try
            {
                var userCtx = _database.GetUser(user.Id);
                var permCtx = userCtx.GetPermission(permission.Id);

                var response = await permCtx.DeleteAsync(cancellationToken: cancellationToken);

                return new CosmosResult
                {
                    RequestCharge = response.RequestCharge,
                };
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.GetMessage(), ex);
            }
        }
    }
}
