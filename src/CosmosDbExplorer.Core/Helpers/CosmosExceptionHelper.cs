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
            try
            {
                if (TryGetQueryException(exception, out var queryException))
                {
                    return queryException;
                }

                if (TryGetResponseException(exception, out var responseException))
                {
                    return responseException;
                }

                return exception.Message;
            }
            catch
            {
                return exception.Message;
            }
        }

        private static bool TryGetResponseException(CosmosException exception, out string? message)
        {
            var regex = new Regex(@"Message:: ({.*})");
            var match = regex.Match(exception.Message);

            if (match.Success)
            {
                var json = match.Captures.First().Value.Replace("Message::", string.Empty);

                var obj = Newtonsoft.Json.Linq.JObject.Parse(json);

                message = string.Join(Environment.NewLine, obj["Errors"]?.Values<string>());
                return true;
            }
            else
            {
                message = null;
                return false;
            }
        }

        private static bool TryGetQueryException(CosmosException exception, out string? message)
        {
            var regex = new Regex(@"Microsoft.Azure.Cosmos.Query.Core.Exceptions.ExpectedQueryPartitionProviderException: ({.*})");
            var match = regex.Match(exception.Message);

            if (match.Success)
            {
                var json = match.Captures.First().Value.Replace("Microsoft.Azure.Cosmos.Query.Core.Exceptions.ExpectedQueryPartitionProviderException:", string.Empty);

                var obj = Newtonsoft.Json.Linq.JObject.Parse(json);

                message = string.Join(Environment.NewLine, obj["errors"]);
                return true;
            }
            else
            {
                message = null;
                return false;
            }
        }
    }
}
