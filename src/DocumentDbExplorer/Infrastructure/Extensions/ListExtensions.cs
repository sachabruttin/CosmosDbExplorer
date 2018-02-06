using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentDbExplorer.Infrastructure.Extensions
{
    public static class ListExtensions
    {
        public static int Replace<T>(this IList<T> source, T oldValue, T newValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var index = source.IndexOf(oldValue);
            if (index != -1)
            {
                source[index] = newValue;
            }

            return index;
        }

        public static void ReplaceAll<T>(this IList<T> source, T oldValue, T newValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var index = -1;
            do
            {
                index = source.IndexOf(oldValue);
                if (index != -1)
                {
                    source[index] = newValue;
                }
            } while (index != -1);
        }


        public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, T oldValue, T newValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return source.Select(x => EqualityComparer<T>.Default.Equals(x, oldValue) ? newValue : x);
        }
    }
}
