using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Helpers
{
    public static class FeedResponseExtensions
    {
        public static string? GetContinuationTokenSafely<T>(this FeedResponse<T> feedResponse)
        {
            try
            {
                return feedResponse.ContinuationToken;
            }
            catch (ArgumentException ex) when (ex.Message == "Continuation token is not supported for queries with GROUP BY. Do not use FeedResponse.ResponseContinuation or remove the GROUP BY from the query.")
            {
                // Silently catch exception 
                return null;
            }
        }
    }
}
