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
    /// Interaction logic for TriggerTabView.xaml
    /// </summary>
    public partial class TriggerTabView : UserControl
    {
        private readonly BraceFoldingStrategy _foldingStrategy = new BraceFoldingStrategy();
        private FoldingManager _foldingManager;

        public TriggerTabView()
        {
            InitializeComponent();

            RoslynPad.Editor.SearchReplacePanel.Install(editor);

            var foldingUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            foldingUpdateTimer.Tick += FoldingUpdateTimer_Tick;
            foldingUpdateTimer.Start();
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
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaneViewModel datacontext)
            {
                datacontext.IconSource = FindResource("TriggerIcon") as ImageSource;
            }
        }
    }
}
