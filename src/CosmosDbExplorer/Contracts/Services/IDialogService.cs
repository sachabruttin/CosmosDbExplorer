using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmosDbExplorer.Services.DialogSettings;

namespace CosmosDbExplorer.Contracts.Services
{
    public interface IFileDialogService
    {
        void ShowOpenFileDialog(OpenFileDialogSettings settings, Action<bool, FileDialogResult>? afterHideCallback = null);
        void ShowSaveFileDialog(SaveFileDialogSettings settings, Action<bool, FileDialogResult>? afterHideCallback = null);
        void ShowFolderBrowserDialog(FolderBrowserDialogSettings settings, Action<bool, FolderDialogResult>? afterHideCallback = null);

    }

    public interface IDialogService : IFileDialogService
    {
        Task ShowError(string message, string title, Action? afterHideCallback = null);
        Task ShowError(Exception error, string title, Action? afterHideCallback = null);
        Task ShowMessage(string message, string title, Action? afterHideCallback = null);
        Task ShowQuestion(string message, string title, Action<bool>? afterHideCallback = null);
    }
}
