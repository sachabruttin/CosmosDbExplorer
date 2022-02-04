using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Extensions;
using CosmosDbExplorer.Models;

using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CosmosDbExplorer.Services
{
    public class PersistAndRestoreService : IPersistAndRestoreService
    {
        private readonly IFileService _fileService;
        private readonly AppConfig _appConfig;
        private readonly string _localAppData;
        private readonly string _configurationFilePath;

        private const string ConfigurationFileName = "connection-settings.json";

        private List<CosmosConnection> _connections = new();


        public PersistAndRestoreService(IFileService fileService, IOptions<AppConfig> appConfig)
        {
            _fileService = fileService;
            _appConfig = appConfig.Value;

            _localAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CosmosDbExplorer");
            _configurationFilePath = Path.Combine(_localAppData, ConfigurationFileName);

            if (!Directory.Exists(_localAppData))
            {
                Directory.CreateDirectory(_localAppData);

                // Use old config file if exists...
                var oldConfigurationFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DocumentDbExplorer", ConfigurationFileName);
                if (File.Exists(oldConfigurationFilePath))
                {
                    File.Copy(oldConfigurationFilePath, _configurationFilePath);
                }
            }
        }

        public void PersistData()
        {
            Properties.Settings.Default.Save();
        }

        public void RestoreData()
        {
            Properties.Settings.Default.Reload();
        }

        public void ResetData()
        {
            Properties.Settings.Default.Reset();
        }

        public List<CosmosConnection> GetConnections()
        {
            if (File.Exists(_configurationFilePath))
            {
                _connections = _fileService.Read<List<CosmosConnection>>(_localAppData, ConfigurationFileName);
            }

            return _connections;
        }

        public void PersistConnection(CosmosConnection connection)
        {
            var index = _connections.IndexOf(connection);

            if (index != -1)
            {
                _connections.Replace(_connections[index], connection);
            }
            else
            {
                _connections.Add(connection);
            }

            SaveConnections();
        }


        public void RemoveConnection(CosmosConnection connection)
        {
            if (_connections.Remove(connection))
            {
                SaveConnections();
            }
        }

        public void ReorderConnections(int sourceIndex, int targetIndex)
        {
            _connections = _connections.Move(sourceIndex, targetIndex).ToList();

            SaveConnections();
        }

        private void SaveConnections()
        {
            var json = JsonConvert.SerializeObject(_connections, Formatting.Indented);
            _fileService.Save(_configurationFilePath, ConfigurationFileName, json);
        }
    }
}
