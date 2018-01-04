/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:DocumentDbExplorer"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using System;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;

namespace DocumentDbExplorer.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                // Create design time view services and models
            }
            else
            {
                // Create run time view services and models
                SimpleIoc.Default.Register<Services.IDialogService, DialogService>();
                SimpleIoc.Default.Register<IMessenger, Messenger>();
                SimpleIoc.Default.Register<IDocumentDbService, DocumentDbService>();
                SimpleIoc.Default.Register<ISettingsService, SettingsService>();
                SimpleIoc.Default.Register<ISimpleIoc>(() => SimpleIoc.Default);
            }

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<DocumentEditorViewModel>();
            SimpleIoc.Default.Register<AccountSettingsViewModel>();
            SimpleIoc.Default.Register<DatabaseViewModel>();
            SimpleIoc.Default.Register<DocumentsTabViewModel>();
            SimpleIoc.Default.Register<QueryEditorViewModel>();
            SimpleIoc.Default.Register<JsonViewerViewModel>();
            SimpleIoc.Default.Register<FeedResponseEditorViewModel>();
            SimpleIoc.Default.Register<ImportDocumentViewModel>();
            SimpleIoc.Default.Register<AboutViewModel>();

            SimpleIoc.Default.Register<ConnectionNodeViewModel>();
            SimpleIoc.Default.Register<StoredProcedureTabViewModel>();
            SimpleIoc.Default.Register<UserDefFuncTabViewModel>();
            SimpleIoc.Default.Register<TriggerTabViewModel>();
            SimpleIoc.Default.Register<ScaleAndSettingsTabViewModel>();

            SimpleIoc.Default.Register<AddCollectionViewModel>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public AccountSettingsViewModel AccountSettings => SimpleIoc.Default.GetInstanceWithoutCaching<AccountSettingsViewModel>();
        public DatabaseViewModel Database => ServiceLocator.Current.GetInstance<DatabaseViewModel>();
        public AboutViewModel About => ServiceLocator.Current.GetInstance<AboutViewModel>();

        public AddCollectionViewModel AddCollection => SimpleIoc.Default.GetInstanceWithoutCaching<AddCollectionViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
