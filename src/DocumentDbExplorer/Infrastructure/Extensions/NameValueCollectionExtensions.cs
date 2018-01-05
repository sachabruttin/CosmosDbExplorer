using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace DocumentDbExplorer.Infrastructure.Extensions
{
    public static class NameValueCollectionExtensions
    {
        public static Dictionary<string, string> ToDictionary(this NameValueCollection source)
        {
            return source.AllKeys.ToDictionary(k => k, k => source[k]);
        }
    }
}
