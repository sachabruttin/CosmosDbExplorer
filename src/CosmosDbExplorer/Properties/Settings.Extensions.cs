using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbExplorer.Properties
{
    internal sealed partial class Settings
    {
        public string GetExportFolder()
        {
            if (string.IsNullOrEmpty(Default.ExportFolder) || string.IsNullOrWhiteSpace(Default.ExportFolder))
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }

            return Default.ExportFolder;
        }
    }
}
