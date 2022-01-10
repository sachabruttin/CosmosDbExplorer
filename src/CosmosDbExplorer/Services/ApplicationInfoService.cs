using System;
using System.Diagnostics;
using System.Reflection;

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
            var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
            return new Version(version);
        }


    }
}
