using CosmosDbExplorer.AvalonEdit;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;

using Microsoft.Xaml.Behaviors;

using System;
using System.Windows;
using System.Windows.Threading;

namespace CosmosDbExplorer.Behaviors
{
    public class AvalonTextEditorBraceFoldingBehavior : Behavior<TextEditor>
    {
        private readonly BraceFoldingStrategy _foldingStrategy = new();
        private FoldingManager? _foldingManager;
        private readonly DispatcherTimer _timer = new();

        protected override void OnAttached()
        {
            base.OnAttached();

            //_timer.Interval = System.TimeSpan.FromMilliseconds(Interval);
            _timer.Tick += OnTimerTicked;

            //if (UseFolding)
            //{
            //    _timer.Start();
            //}
        }

        protected override void OnDetaching()
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTicked;

            if (_foldingManager is not null)
            {
                FoldingManager.Uninstall(_foldingManager);
            }

            base.OnDetaching();
        }

        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register(
                "Interval",
                typeof(int),
                typeof(AvalonTextEditorBraceFoldingBehavior),
                new PropertyMetadata(1000, OnIntervalPropertyChanged));

        private static void OnIntervalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AvalonTextEditorBraceFoldingBehavior behavior)
            {
                behavior.OnIntervalChanged();
            }
        }

        public bool UseFolding
        {
            get { return (bool)GetValue(UseFoldingProperty); }
            set { SetValue(UseFoldingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UseFolding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseFoldingProperty =
            DependencyProperty.Register(
                "UseFolding", 
                typeof(bool), 
                typeof(AvalonTextEditorBraceFoldingBehavior), 
                new PropertyMetadata(false, OnUseFoldingPropertyChanged));

        private static void OnUseFoldingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AvalonTextEditorBraceFoldingBehavior behavior)
            {
                behavior.OnUseFoldingChanged();
            }
        }

        private void OnUseFoldingChanged()
        {
            if (UseFolding)
            {
                _timer.Start();
            }
            else
            {
                if (_foldingManager != null)
                {
                    _foldingManager.Clear();
                    FoldingManager.Uninstall(_foldingManager);
                }

                _timer.Stop();
            }
        }

        private void OnIntervalChanged()
        {
            if (AssociatedObject is null)
            {
                return;
            }

            _timer.Stop();
            _timer.Interval = System.TimeSpan.FromMilliseconds(Interval);
            _timer.Start();
        }

        private void OnTimerTicked(object? sender, System.EventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }

            if (_foldingManager == null && AssociatedObject?.TextArea?.Document?.Text != null)
            {
                _foldingManager = FoldingManager.Install(AssociatedObject.TextArea);
            }

            if (_foldingManager != null)
            {
                _foldingStrategy.UpdateFoldings(_foldingManager, AssociatedObject.Document);
            }
        }
    }
}
