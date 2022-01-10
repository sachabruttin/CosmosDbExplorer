using System;
using System.Windows.Threading;
using AutoUpdaterDotNET;
using CosmosDbExplorer.Properties;
using CosmosDbExplorer.ViewModel;
using Newtonsoft.Json;

namespace CosmosDbExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupAutoUpdater();
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

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(Settings.Default.AutoUpdaterIntervalInSeconds) };
            timer.Tick += delegate
            {
                AutoUpdater.Start(Settings.Default.AutoUpdaterUrl);
            };
            timer.Start();
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

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = (MainViewModel)DataContext;
            vm.RequestClose += () => Close();

            AutoUpdater.Start(Settings.Default.AutoUpdaterUrl);
        }
    }
}
