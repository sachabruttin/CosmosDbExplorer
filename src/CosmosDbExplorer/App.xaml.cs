using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;

namespace CosmosDbExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Global exception handling
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomainUnandleException);
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);
            TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>(UnobservedTaskException);
        }

        private void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (!Debugger.IsAttached)
            {
                e.SetObserved();
                ShowUnhandledException(e.Exception, false);
            }
        }

        private void CurrentDomainUnandleException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!Debugger.IsAttached)
            {
                ShowUnhandledException(e.ExceptionObject as Exception, e.IsTerminating);
            }
        }

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
                ShowUnhandledException(e.Exception, false);
            }
        }

        private void ShowUnhandledException(Exception exception, bool isTerminating)
        {
            var details = exception.Message + (exception.InnerException != null ? "\n" + exception.InnerException.Message : null);
            var errorMessage = $@"An application error occured.
Please check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it.

Error: {details}";

            if (!isTerminating)
            {
                errorMessage += "\nDo you want to continue?";

                if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
                {
                    if (MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        Application.Current.Shutdown();
                    }
                }
            }
            else
            {
                MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
