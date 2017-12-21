using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using DocumentDbExplorer.Infrastructure.AvalonEdit;
using DocumentDbExplorer.ViewModel;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Indentation.CSharp;

namespace DocumentDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for ScaleAndSettingsTabView.xaml
    /// </summary>
    public partial class ScaleAndSettingsTabView : UserControl
    {
        private readonly BraceFoldingStrategy _foldingStrategy = new BraceFoldingStrategy();
        private FoldingManager _foldingManager;

        public ScaleAndSettingsTabView()
        {
            RegisterCustomHighlighting("JSON");

            InitializeComponent();

            editor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(editor.Options);

            var foldingUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            foldingUpdateTimer.Tick += FoldingUpdateTimer_Tick;
            //foldingUpdateTimer.Start();
        }

        private void RegisterCustomHighlighting(string name)
        {
            // Load our custom highlighting definition
            IHighlightingDefinition customHightlighting;
            using (var stream = typeof(MainWindow).Assembly.GetManifestResourceStream($"DocumentDbExplorer.Infrastructure.AvalonEdit.{name}.xshd"))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Could not find embedded resource");
                }

                using (var reader = new XmlTextReader(stream))
                {
                    customHightlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting(name, new string[] { $".{name.ToLower()}" }, customHightlighting);
        }

        private void FoldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (_foldingManager == null && editor.TextArea?.Document?.Text != null)
            {
                _foldingManager = FoldingManager.Install(editor.TextArea);
            }

            if (_foldingStrategy != null && _foldingManager != null)
            {
                _foldingStrategy.UpdateFoldings(_foldingManager, editor.Document);
            }
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as ScaleAndSettingsTabViewModel;
            vm.IconSource = FindResource("ScaleSettingsIcon") as ImageSource;
            await vm.LoadDataAsync();
        }
    }
}
