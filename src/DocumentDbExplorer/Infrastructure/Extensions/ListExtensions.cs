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

        public static IList<T> Move<T>(this IList<T> list, int oldIndex, int newIndex)
        {
            // exit if possitions are equal or outside array
            if ((oldIndex == newIndex) || (0 > oldIndex) || (oldIndex >= list.Count) || (0 > newIndex) || (newIndex >= list.Count))
            {
                return list;
            }

            // local variables
            var i = 0;
            var tmp = list[oldIndex];
            // move element down and shift other elements up
            if (oldIndex < newIndex)
            {
                for (i = oldIndex; i < newIndex; i++)
                {
                    list[i] = list[i + 1];
                }
            }
            // move element up and shift other elements down
            else
            {
                for (i = oldIndex; i > newIndex; i--)
                {
                    list[i] = list[i - 1];
                }
            }
            // put element from position 1 to destination
            list[newIndex] = tmp;

            return list;
        }
    }
}
