using System;

using CosmosDbExplorer.Models;

namespace CosmosDbExplorer.Contracts.Services
{
    public interface IThemeSelectorService
    {
        void InitializeTheme();

        void SetTheme(AppTheme theme);

        AppTheme GetCurrentTheme();
    }
}
