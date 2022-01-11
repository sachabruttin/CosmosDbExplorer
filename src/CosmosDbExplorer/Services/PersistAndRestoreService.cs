using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Models;

using Microsoft.Extensions.Options;

namespace CosmosDbExplorer.Services
{
    public class PersistAndRestoreService : IPersistAndRestoreService
    {
        private readonly IFileService _fileService;
        private readonly AppConfig _appConfig;
        private readonly string _localAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CosmosDbExplorer");

        public PersistAndRestoreService(IFileService fileService, IOptions<AppConfig> appConfig)
        {
            _fileService = fileService;
            _appConfig = appConfig.Value;
        }

        public void PersistData()
        {
            if (App.Current.Properties != null)
            {
                var fileName = _appConfig.AppPropertiesFileName;
                _fileService.Save(_localAppData, fileName, App.Current.Properties);
            }
        }

        public void RestoreData()
        {
            RestoreSettings();
            RestoreConnections();
        }

        private void RestoreSettings()
        {
            var fileName = _appConfig.AppPropertiesFileName;
            var properties = _fileService.Read<IDictionary>(_localAppData, fileName);
            if (properties != null)
            {
                foreach (DictionaryEntry property in properties)
                {
                    App.Current.Properties.Add(property.Key, property.Value);
                }
            }
        }

        private void RestoreConnections()
        {
            var fileName = _appConfig.ConnectionsFileName;
            var connections = _fileService.Read<List<CosmosConnection>>(_localAppData, fileName);
            
            if (connections != null)
            {
                App.Current.Properties.Add("Connections", connections.ToDictionary(connection => connection.Id));
            }
        }
    }
}
