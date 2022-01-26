using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Helpers
{
    public static class CosmosExceptionHelper
    {
        public static string GetMessage(this CosmosException exception)
        {
            var regex = new Regex(@"Message: ({.*})", RegexOptions.Multiline);

            var match = regex.Match(exception.ResponseBody);

            var json = match.Captures.First().Value.Replace("Message:", string.Empty);
            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
            return string.Join(Environment.NewLine, obj["Errors"]?.Values<string>());
        }
    }
}
