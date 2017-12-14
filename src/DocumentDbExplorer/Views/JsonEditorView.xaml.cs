using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Windows.Controls;
using System.Xml;
using ICSharpCode.AvalonEdit.Indentation.CSharp;
using ICSharpCode.AvalonEdit.Folding;
using DocumentDbExplorer.Infrastructure.AvalonEdit;
using System.Windows.Threading;
using System.Windows;
using ICSharpCode.AvalonEdit.Search;

namespace DocumentDbExplorer.Views
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

            editor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(editor.Options);

            SearchPanel.Install(editor);

            var foldingUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            foldingUpdateTimer.Tick += FoldingUpdateTimer_Tick;
            foldingUpdateTimer.Start();
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
    }
}
