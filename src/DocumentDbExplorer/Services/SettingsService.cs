using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure.Models;
using Newtonsoft.Json;

namespace DocumentDbExplorer.Services
{
    public interface ISettingsService
    {
        Task<IEnumerable<Connection>> GetConnectionsAsync();

        Task SaveConnectionAsync(Connection connection);
        Task RemoveConnection(Connection connection);
    }

    public class SettingsService : ISettingsService
    {
        private const string _configurationFileName = "connection-settings.json";
        public static readonly string _configurationFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DocumentDbExplorer" , _configurationFileName);

        public SettingsService()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_configurationFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_configurationFilePath));
            }
        }

        public async Task<IEnumerable<Connection>> GetConnectionsAsync()
        {
            if (File.Exists(_configurationFilePath))
            {
                using (var reader = File.OpenText(_configurationFilePath))
                {
                    var json = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<IEnumerable<Connection>>(json);
                }
            }
            else
            {
                return Enumerable.Empty<Connection>();
            }
        }

        public async Task RemoveConnection(Connection connection)
        {
            var connections = (await GetConnectionsAsync()).ToList();
            
            if (connections.Remove(connection))
            {
                await SaveAsync(connections);
            }
        }

        public async Task SaveConnectionAsync(Connection connection)
        {
            var connections = (await GetConnectionsAsync()).ToList();
            var existing = connections.FirstOrDefault(e => e.Equals(connection));

            if (existing != null)
            {
                existing = connection;
            }
            else
            {
                connections.Add(connection);
            }

            await SaveAsync(connections);
        }

        private async Task SaveAsync(IEnumerable<Connection> connections)
        {

            var json = JsonConvert.SerializeObject(connections);

            using (var fs = File.Open(_configurationFilePath, FileMode.Truncate))
            {
                var info = new UTF8Encoding(true).GetBytes(json);
                await fs.WriteAsync(info, 0, info.Length);
            }
        }
    }
}
