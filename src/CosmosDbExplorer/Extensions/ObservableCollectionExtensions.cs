using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CosmosDbExplorer.Extensions;

public static class ObservableCollectionExtensions
{
    public static void AddRangeSorted<T, TSort>(this ObservableCollection<T> collection, IEnumerable<T> toAdd, Func<T, TSort> sortSelector)
    {
        var sortArr = Enumerable.Concat(collection, toAdd).OrderBy(sortSelector).ToList();
        foreach (var obj in toAdd.OrderBy(o => sortArr.IndexOf(o)).ToList())
        {
            collection.Insert(sortArr.IndexOf(obj), obj);
        }
    }

    public static void AddSorted<T, TSort>(this ObservableCollection<T> collection, T toAdd, Func<T, TSort> sortSelector)
    {
        AddRangeSorted(collection, new[] { toAdd }, sortSelector);
    }
}
