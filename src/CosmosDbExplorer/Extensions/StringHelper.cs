using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CosmosDbExplorer.Extensions
{
    public static class StringExtensions
    {
        public static string SafeForFilename(this string input)
        {
            return Regex.Replace(input, "[" + string.Join("", Path.GetInvalidFileNameChars().Select(p => p.ToString())) + "]", "_");
        }
    }
}
