using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Windows.Controls;
using System.Xml;
using ICSharpCode.AvalonEdit.Folding;
using CosmosDbExplorer.Infrastructure.AvalonEdit;
using System.Windows.Threading;
using System.Windows;
using System.Linq;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for JsonEditor.xaml
    /// </summary>
    public partial class JsonEditorView : UserControl
    {
        private readonly BraceFoldingStrategy _foldingStrategy = new BraceFoldingStrategy();
        private FoldingManager _foldingManager;

        public JsonEditorView()
        {
            RegisterCustomHighlighting("JSON");

            InitializeComponent();
            RoslynPad.Editor.SearchReplacePanel.Install(editor);

            var foldingUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            foldingUpdateTimer.Tick += FoldingUpdateTimer_Tick;
            foldingUpdateTimer.Start();

            Unloaded += (s, e) =>
            {
                if (foldingUpdateTimer?.IsEnabled == true)
                {
                    foldingUpdateTimer.Stop();
                    foldingUpdateTimer = null;
                }
            };
        }

        private void RegisterCustomHighlighting(string name)
        {
            if (!HighlightingManager.Instance.HighlightingDefinitions.Any(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                // Load our custom highlighting definition
                IHighlightingDefinition customHightlighting;
                using (var stream = typeof(MainWindow).Assembly.GetManifestResourceStream($"CosmosDbExplorer.Infrastructure.AvalonEdit.{name}.xshd"))
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

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register("ZoomLevel", typeof(double), typeof(JsonEditorView), new PropertyMetadata(0.5d, OnZoomLevelChanged));

        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (double)e.NewValue;
            var target = (JsonEditorView)d;
            target.zoomBehavior.ZoomLevel = value;
        }
    }
}
