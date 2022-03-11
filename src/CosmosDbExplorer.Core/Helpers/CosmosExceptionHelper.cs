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
                if (TryGetErrors(exception, out var errors))
                {
                    return errors ?? string.Empty;
                }

                if (TryGetQueryException(exception, out var queryException))
                {
                    return queryException ?? string.Empty;
                }

                if (TryGetResponseException(exception, out var responseException))
                {
                    return responseException ?? string.Empty;
                }

                if (TryGetUpdatingOfferException(exception, out var offerException))
                {
                    return offerException ?? string.Empty;
                }

                return exception.Message;
            }
            catch
            {
                return exception.Message;
            }
        }

        private static bool TryGetErrors(CosmosException exception, out string? message)
        {
            var regex = new Regex(@"Errors : (\[.*\])");
            var match = regex.Match(exception.Message.Replace(Environment.NewLine, string.Empty));

            if (match.Success)
            {
                var json = match.Groups.Last().Value;

                var obj = Newtonsoft.Json.Linq.JArray.Parse(json);

                message = string.Join(Environment.NewLine, obj?.Values<string>());
                return true;
            }
            else
            {
                message = null;
                return false;
            }
        }

        private static bool TryGetUpdatingOfferException(CosmosException exception, out string? message)
        {
            try
            {
                var index = exception.ResponseBody.IndexOf(Environment.NewLine);
                var start = "Message: ".Length;

                var json = exception.ResponseBody.Substring(start, index - start);

                var obj = Newtonsoft.Json.Linq.JObject.Parse(json);

                message = string.Join(Environment.NewLine, obj["Errors"]?.Values<string>());
                return true;
            }
            catch
            {
                message = null;
                return false;
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
