using System;
using System.Threading.Tasks;
using System.Windows;
using CosmosDbExplorer.Services.DialogSettings;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Win32;

namespace CosmosDbExplorer.Services
{
    public interface IDialogService : GalaSoft.MvvmLight.Views.IDialogService
    {
        Task<bool> ShowSaveFileDialog(SaveFileDialogSettings settings, Action<bool, FileDialogResult> afterHideCallback);
        //Task<bool> ShowFolderBrowserDialog(FolderBrowserDialogSettings settings, Action<bool> afterHideCallback);
        Task<bool> ShowOpenFileDialog(OpenFileDialogSettings settings, Action<bool, FileDialogResult> afterHideCallback);
    }

    public class DialogService : IDialogService
    {
        public Task ShowError(string message, string title, string buttonText, Action afterHideCallback)
        {
            DispatcherHelper.RunAsync(() => MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Error));
            return Task.Run(() => afterHideCallback);
        }

        public Task ShowError(Exception error, string title, string buttonText, Action afterHideCallback)
        {
            DispatcherHelper.RunAsync(() => MessageBox.Show(Application.Current.MainWindow, error.GetBaseException().Message, title, MessageBoxButton.OK, MessageBoxImage.Error));
            return Task.Run(() => afterHideCallback);
        }

        public Task ShowMessage(string message, string title)
        {
            DispatcherHelper.RunAsync(() => MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information));
            return null;
        }

        public Task ShowMessage(string message, string title, string buttonText, Action afterHideCallback)
        {
            DispatcherHelper.RunAsync(() => MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information));
            return Task.Run(() => afterHideCallback);
        }

        public async Task<bool> ShowMessage(string message, string title, string buttonConfirmText, string buttonCancelText, Action<bool> afterHideCallback)
        {
            var confirmed = false;
            await DispatcherHelper.RunAsync(() =>
            {
                var result = MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                confirmed = result == MessageBoxResult.Yes;
            });

            afterHideCallback(confirmed);
            return confirmed;
        }

        public Task ShowMessageBox(string message, string title)
        {
            DispatcherHelper.RunAsync(() => MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information));
            return Task.FromResult(0);
        }

        //public Task<bool> ShowFolderBrowserDialog(FolderBrowserDialogSettings settings, Action<bool> afterHideCallback)
        //{
        //    var dialog = new FolderBrowserDialog()
        //}

        public Task<bool> ShowOpenFileDialog(OpenFileDialogSettings settings, Action<bool, FileDialogResult> afterHideCallback)
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

            return Task.Run(() => { afterHideCallback(confirmed, new FileDialogResult(dialog.FileName, dialog.FileNames)); return confirmed; });
        }

        public Task<bool> ShowSaveFileDialog(SaveFileDialogSettings settings, Action<bool, FileDialogResult> afterHideCallback)
        {
            var dialog = new SaveFileDialog
            {
                AddExtension = settings.AddExtension,
                CheckFileExists = settings.CheckFileExists,
                CheckPathExists = settings.CheckPathExists,
                CreatePrompt = settings.CreatePrompt,
                DefaultExt = settings.DefaultExt,
                FileName = settings.FileName,
                Filter = settings.Filter,
                FilterIndex = settings.FilterIndex,
                InitialDirectory = settings.InitialDirectory,
                OverwritePrompt = settings.OverwritePrompt,
                Title = settings.Title
            };

            var result = dialog.ShowDialog(Application.Current.MainWindow);
            var confirmed = result.GetValueOrDefault();

            return Task.Run(() => { afterHideCallback(confirmed, new FileDialogResult(dialog.FileName, dialog.FileNames)); return confirmed; });
        }
    }
}
