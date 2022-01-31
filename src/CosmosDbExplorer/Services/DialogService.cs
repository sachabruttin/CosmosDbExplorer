using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Services.DialogSettings;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace CosmosDbExplorer.Services
{
    public class FileDialogService : IFileDialogService
    {
        public void ShowOpenFileDialog(OpenFileDialogSettings settings, Action<bool, FileDialogResult>? afterHideCallback = null)
        {
            var dialog = new OpenFileDialog
            {
                AddExtension = settings.AddExtension,
                CheckFileExists = settings.CheckFileExists,
                CheckPathExists = settings.CheckPathExists,
                DefaultExt = settings.DefaultExt,
                FileName = settings.FileName,
                Filter = settings.Filter,
                FilterIndex = settings.FilterIndex,
                InitialDirectory = settings.InitialDirectory,
                Multiselect = settings.Multiselect,
                Title = settings.Title,
            };

            var result = dialog.ShowDialog(Application.Current.MainWindow);
            var confirmed = result.GetValueOrDefault();

            afterHideCallback?.Invoke(confirmed, new FileDialogResult(dialog.FileName, dialog.FileNames));
        }
    }

    public class MetroDialogService : FileDialogService, IDialogService
    {
        private static MetroWindow MainWindow => (MetroWindow)Application.Current.MainWindow;

        public async Task ShowError(string message, string title, Action? afterHideCallback = null)
        {
            var settings = new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Theme };
            var result = await MainWindow.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, settings);
            afterHideCallback?.Invoke();
        }

        public Task ShowError(Exception error, string title, Action? afterHideCallback = null)
        {
            return ShowError(error.GetBaseException().Message, title, afterHideCallback);
        }

        public Task ShowMessage(string message, string title, Action? afterHideCallback = null)
        {
            return ShowError(message, title, afterHideCallback);
        }

        public async Task ShowQuestion(string message, string title, Action<bool>? afterHideCallback = null)
        {
            var settings = new MetroDialogSettings
            {
                ColorScheme = MetroDialogColorScheme.Theme,
                DefaultButtonFocus = MessageDialogResult.Negative,
                NegativeButtonText = "NO",
                AffirmativeButtonText = "YES"
            };

            var result = await MainWindow.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, settings);
            afterHideCallback?.Invoke(result == MessageDialogResult.Affirmative);
        }
    }

    public class DialogService : FileDialogService, IDialogService
    {
        public Task ShowError(string message, string title, Action? afterHideCallback = null)
        {
            MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            afterHideCallback?.Invoke();
            return Task.CompletedTask;
        }

        public Task ShowError(Exception error, string title, Action? afterHideCallback = null)
        {
            return ShowError(error.GetBaseException().Message, title, afterHideCallback);
        }

        public Task ShowMessage(string message, string title, Action? afterHideCallback = null)
        {
            MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            afterHideCallback?.Invoke();
            return Task.CompletedTask;
        }

        public Task ShowQuestion(string message, string title, Action<bool>? afterHideCallback = null)
        {
            var result = MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            afterHideCallback?.Invoke(result == MessageBoxResult.Yes);
            return Task.CompletedTask;
        }
    }
}
