using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DocumentDbExplorer.Infrastructure.AvalonEdit;
using DocumentDbExplorer.Infrastructure.Models;
using ICSharpCode.AvalonEdit.Folding;

namespace DocumentDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for StoredProcedureTabView.xaml
    /// </summary>
    public partial class StoredProcedureTabView : UserControl
    {
        private readonly BraceFoldingStrategy _foldingStrategy = new BraceFoldingStrategy();
        private FoldingManager _foldingManager;

        public StoredProcedureTabView()
        {
            InitializeComponent();

            RoslynPad.Editor.SearchReplacePanel.Install(editor);

            var foldingUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            foldingUpdateTimer.Tick += FoldingUpdateTimer_Tick;
            //foldingUpdateTimer.Start();
        }

        private void FoldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            try
            {

                if (_foldingManager == null && editor.TextArea?.Document?.Text != null)
                {
                    _foldingManager = FoldingManager.Install(editor.TextArea);
                }

                if (_foldingStrategy != null && _foldingManager != null && editor.Document != null)
                {
                    _foldingStrategy.UpdateFoldings(_foldingManager, editor.Document);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("FoldingUpdateTimer_Tick Exception: " + ex.Message);
                // silently fails
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaneViewModel datacontext)
            {
                datacontext.IconSource = FindResource("StoredProcedureIcon") as ImageSource;
            }
        }
    }
}
