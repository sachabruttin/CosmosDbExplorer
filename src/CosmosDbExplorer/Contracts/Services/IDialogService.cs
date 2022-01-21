using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbExplorer.Contracts.Services
{
    public interface IDialogService
    {
        Task ShowError(string message, string title, Action? afterHideCallback = null);
        Task ShowError(Exception error, string title, Action? afterHideCallback = null);
        Task ShowMessage(string message, string title, Action? afterHideCallback = null);
        Task ShowQuestion(string message, string title, Action<bool>? afterHideCallback = null);
    }
}
