using System;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface ICanClose
    {
        Action SetResult { get; }
    }
}
