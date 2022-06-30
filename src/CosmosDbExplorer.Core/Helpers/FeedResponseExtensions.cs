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
            catch
            {
                // Silently catch exception 
                return null;
            }
        }
    }
}
