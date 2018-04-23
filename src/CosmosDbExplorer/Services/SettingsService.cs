using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure.Extensions;
using CosmosDbExplorer.Infrastructure.Models;
using Newtonsoft.Json;

namespace CosmosDbExplorer.Services
{
    public interface ISettingsService
    {
        Task<Dictionary<Guid, Connection>> GetConnectionsAsync();
        Task SaveConnectionAsync(Connection connection);
        Task RemoveConnection(Connection connection);
        Task ReorderConnections(int sourceIndex, int targetIndex);
    }

    public class SettingsService : ISettingsService
    {
        private const string _configurationFileName = "connection-settings.json";
        public static readonly string _configurationFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CosmosDbExplorer" , _configurationFileName);

        public static Dictionary<Guid, Connection> _connections;

        public SettingsService()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_configurationFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_configurationFilePath));

                // Use old config file if exists...
                var oldConfigurationFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DocumentDbExplorer", _configurationFileName);
                if (File.Exists(oldConfigurationFilePath))
                {
                    File.Copy(oldConfigurationFilePath, _configurationFilePath);
                }
            }
        }

        public async Task<Dictionary<Guid, Connection>> GetConnectionsAsync()
        {
            if (_connections != null)
            {
                return _connections;
            }

            if (File.Exists(_configurationFilePath))
            {
                using (var reader = File.OpenText(_configurationFilePath))
                {
                    var json = await reader.ReadToEndAsync();
                    _connections = JsonConvert.DeserializeObject<IEnumerable<Connection>>(json)
                                              .ToDictionary(c => c.Id);
                }
            }
            else
            {
                _connections = new Dictionary<Guid, Connection>();
            }

            return _connections;
        }

        public async Task RemoveConnection(Connection connection)
        {           
            if (_connections.Remove(connection.Id))
            {
                await SaveAsync(_connections.Values);
            }
        }

        public Task ReorderConnections(int sourceIndex, int targetIndex)
        {
            _connections = _connections.Values.ToList().Move(sourceIndex, targetIndex).ToDictionary(c => c.Id);

            return SaveAsync(_connections.Values);
        }

        public async Task SaveConnectionAsync(Connection connection)
        {
            if (_connections.ContainsKey(connection.Id))
            {
                _connections[connection.Id] = connection;
            }
            else
            {
                _connections.Add(connection.Id, connection);
            }

            await SaveAsync(_connections.Values);
        }

        private async Task SaveAsync(IEnumerable<Connection> connections)
        {
            var json = JsonConvert.SerializeObject(connections, Formatting.Indented);

            using (var fs = File.Open(_configurationFilePath, FileMode.Create))
            {
                var info = new UTF8Encoding(true).GetBytes(json);
                await fs.WriteAsync(info, 0, info.Length);
            }
        }
    }
}
