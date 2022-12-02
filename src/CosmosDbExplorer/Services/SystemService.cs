using System;
using System.Diagnostics;

using CosmosDbExplorer.Contracts.Services;

namespace CosmosDbExplorer.Services
{
    public class SystemService : ISystemService
    {
        public SystemService()
        {
        }

        public void OpenInWebBrowser(string? url)
        {
            if (url == null)
            {
                throw new NullReferenceException("Can not open a NULL Url!");
            }

            // For more info see https://github.com/dotnet/corefx/issues/10361
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
