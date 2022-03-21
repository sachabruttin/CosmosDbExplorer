using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using AutoUpdaterDotNET;

using CosmosDbExplorer.Behaviors;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.Views;
using CosmosDbExplorer.Properties;
using CosmosDbExplorer.ViewModels;

using Fluent;

using MahApps.Metro.Controls;

using Newtonsoft.Json;

namespace CosmosDbExplorer.Views
{
    public partial class ShellWindow : MetroWindow, IShellWindow, IRibbonWindow
    {
        public RibbonTitleBar TitleBar
        {
            get => (RibbonTitleBar)GetValue(TitleBarProperty);
            private set => SetValue(TitleBarPropertyKey, value);
        }

        private static readonly DependencyPropertyKey TitleBarPropertyKey = DependencyProperty.RegisterReadOnly(nameof(TitleBar), typeof(RibbonTitleBar), typeof(ShellWindow), new PropertyMetadata());

        public static readonly DependencyProperty TitleBarProperty = TitleBarPropertyKey.DependencyProperty;

        public ShellWindow(IPageService pageService, ShellViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            navigationBehavior.Initialize(pageService);
        }

        //public Frame GetNavigationFrame()
        //    => shellFrame;

        public RibbonTabsBehavior GetRibbonTabsBehavior()
            => tabsBehavior;

        public Frame GetRightPaneFrame()
            => rightPaneFrame;

        public SplitView GetSplitView()
            => splitView;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var window = sender as MetroWindow;
            TitleBar = window.FindChild<RibbonTitleBar>("RibbonTitleBar");
            TitleBar.InvalidateArrange();
            TitleBar.UpdateLayout();

            AutoUpdater.Start(Settings.Default.AutoUpdaterUrl);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            tabsBehavior.Unsubscribe();
        }

        private void SetupAutoUpdater()
        {
            // Allow user to be reminded to update in 1 day
            AutoUpdater.ShowRemindLaterButton = true;
            AutoUpdater.LetUserSelectRemindLater = false;
            AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Days;
            AutoUpdater.RemindLaterAt = 1;

            AutoUpdater.ReportErrors = false;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.DownloadPath = Environment.CurrentDirectory;
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdateOnParseUpdateInfoEvent;
        }

        private void AutoUpdateOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            // Use JSON format for AutoUpdate release information file
            dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
            args.UpdateInfo = new UpdateInfoEventArgs
            {
                CurrentVersion = json.version,
                ChangelogURL = json.changelog,
                Mandatory = new Mandatory { Value = json.mandatory },
                DownloadURL = json.url,
                CheckSum = json.checksum != null ? new CheckSum { Value = json.checksum } : null
            };
        }
    }
}
