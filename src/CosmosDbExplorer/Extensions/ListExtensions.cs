﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CosmosDbExplorer.Extensions;

public static class ListExtensions
{
    public static int Replace<T>(this IList<T> source, T oldValue, T newValue)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
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
            throw new ArgumentNullException(nameof(source));
        }

        int index;
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
            throw new ArgumentNullException(nameof(source));
        }

        return source.Select(x => EqualityComparer<T>.Default.Equals(x, oldValue) ? newValue : x);
    }

    public static IList<T> Move<T>(this IList<T> list, int oldIndex, int newIndex)
    {
        // exit if positions are equal or outside array
        if (oldIndex == newIndex || 0 > oldIndex || oldIndex >= list.Count || 0 > newIndex || newIndex >= list.Count)
        {
            return list;
        }

        var tmp = list[oldIndex];

        // local variables
        int i;
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
