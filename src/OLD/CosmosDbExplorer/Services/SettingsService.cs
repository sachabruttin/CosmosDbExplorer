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
        private const string ConfigurationFileName = "connection-settings.json";
        public static readonly string ConfigurationFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CosmosDbExplorer" , ConfigurationFileName);

        public static Dictionary<Guid, Connection> Connections;

        public SettingsService()
        {
            if (!Directory.Exists(Path.GetDirectoryName(ConfigurationFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigurationFilePath));

                // Use old config file if exists...
                var oldConfigurationFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DocumentDbExplorer", ConfigurationFileName);
                if (File.Exists(oldConfigurationFilePath))
                {
                    File.Copy(oldConfigurationFilePath, ConfigurationFilePath);
                }
            }
        }

        public async Task<Dictionary<Guid, Connection>> GetConnectionsAsync()
        {
            if (Connections != null)
            {
                return Connections;
            }

            if (File.Exists(ConfigurationFilePath))
            {
                using (var reader = File.OpenText(ConfigurationFilePath))
                {
                    var json = await reader.ReadToEndAsync();
                    Connections = JsonConvert.DeserializeObject<IEnumerable<Connection>>(json)
                                              .ToDictionary(c => c.Id);
                }
            }
            else
            {
                Connections = new Dictionary<Guid, Connection>();
            }

            return Connections;
        }

        public async Task RemoveConnection(Connection connection)
        {           
            if (Connections.Remove(connection.Id))
            {
                await SaveAsync(Connections.Values);
            }
        }

        public Task ReorderConnections(int sourceIndex, int targetIndex)
        {
            Connections = Connections.Values.ToList().Move(sourceIndex, targetIndex).ToDictionary(c => c.Id);

            return SaveAsync(Connections.Values);
        }

        public async Task SaveConnectionAsync(Connection connection)
        {
            if (Connections.ContainsKey(connection.Id))
            {
                Connections[connection.Id] = connection;
            }
            else
            {
                Connections.Add(connection.Id, connection);
            }

            await SaveAsync(Connections.Values);
        }

        private async Task SaveAsync(IEnumerable<Connection> connections)
        {
            var json = JsonConvert.SerializeObject(connections, Formatting.Indented);

            using (var fs = File.Open(ConfigurationFilePath, FileMode.Create))
            {
                var info = new UTF8Encoding(true).GetBytes(json);
                await fs.WriteAsync(info, 0, info.Length);
            }
        }
    }
}
