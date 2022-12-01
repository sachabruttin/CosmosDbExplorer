using System;
using System.Text.RegularExpressions;

using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Models
{
    public class GenericQuery
    {
        public static string GetQuery(GenericQueryTypes type, CosmosContainer container) => type switch
        {
            GenericQueryTypes.Default => $"SELECT * FROM {GetContainerName(container)} AS {GetContainerAlias(container)}",
            GenericQueryTypes.Top100 => $"SELECT TOP 100 * FROM {GetContainerName(container)} AS {GetContainerAlias(container)}",
            GenericQueryTypes.Count => $"SELECT VALUE COUNT({GetContainerAlias(container)}) FROM {GetContainerName(container)} AS {GetContainerAlias(container)}",
            GenericQueryTypes.Where => $"SELECT * FROM {GetContainerName(container)} AS {GetContainerAlias(container)} WHERE {GetContainerAlias(container)}.id",
            GenericQueryTypes.OrderBy => $"SELECT * FROM {GetContainerName(container)} AS {GetContainerAlias(container)} ORDER BY {GetContainerAlias(container)}.id",
            GenericQueryTypes.OrderByDescending => $"SELECT * FROM {GetContainerName(container)} AS {GetContainerAlias(container)} ORDER BY {GetContainerAlias(container)}.id DESC",
            _ => throw new NotImplementedException(),
        };

        private static string GetContainerName(CosmosContainer container) => Regex.Replace(container.Id, @"[^0-9a-zA-Z]+", "_");
        private static string GetContainerAlias(CosmosContainer container) => container.Id[..1].ToLower();
    }

    public enum GenericQueryTypes
    {
        Default = 0,
        Top100 = 1,
        Count = 2,
        Where = 3,
        OrderBy = 4,
        OrderByDescending = 5
    }
}
