using System;
using System.Diagnostics;
using System.Reflection;

using AvalonDock.Properties;

using CosmosDbExplorer.Contracts.Services;

namespace CosmosDbExplorer.Services
{
    public class ApplicationInfoService : IApplicationInfoService
    {

        public ApplicationInfoService()
        {
        }

        public Version GetVersion()
        {
            // Set the app version in CosmosDbExplorer > Properties > Package > PackageVersion
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion ?? "0.0.0.0";
            return new Version(version);
        }

        public string GetTitle()
        {
            return Properties.Resources.AppDisplayName;
        }
    }
}
