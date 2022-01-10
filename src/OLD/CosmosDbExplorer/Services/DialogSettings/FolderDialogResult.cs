using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbExplorer.Services.DialogSettings
{

    public class FolderDialogResult
    {
        public FolderDialogResult(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
