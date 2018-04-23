using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbExplorer.Services.DialogSettings
{
    public class FileDialogResult
    {
        public FileDialogResult(string fileName, string[] fileNames)
        {
            FileName = fileName;
            FileNames = fileNames;
        }
        public string FileName { get; set; }
        public string[] FileNames { get; set; }
    }
}
