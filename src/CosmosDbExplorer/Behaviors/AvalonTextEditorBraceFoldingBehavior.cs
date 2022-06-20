﻿using System;
using System.Windows;
using System.Windows.Threading;
using CosmosDbExplorer.AvalonEdit;
using CosmosDbExplorer.Core.Helpers;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using Microsoft.Xaml.Behaviors;

namespace CosmosDbExplorer.Behaviors
{
    public class AvalonTextEditorBraceFoldingBehavior : Behavior<TextEditor>
    {
        private readonly BraceFoldingStrategy _foldingStrategy = new();
        private FoldingManager? _foldingManager;
        private readonly TimedDebounce _timer = new() { WaitingMilliSeconds = 500 };

        protected override void OnAttached()
        {
            base.OnAttached();
            _timer.Idled += OnTextChangedIdle;
            AssociatedObject.TextChanged += OnTextChanged;
        }
        protected override void OnDetaching()
        {
            _timer.Idled -= OnTextChangedIdle;
            AssociatedObject.TextChanged -= OnTextChanged;
            ReleaseFoldingManager();
            base.OnDetaching();
        }

        private void OnTextChanged(object? sender, EventArgs e)
        {
            if (_foldingManager is null || AssociatedObject is null)
            {
                return;
            }

            _timer.DebounceEvent();
        }

        private void OnTextChangedIdle(object? sender, EventArgs e)
        {
            if (_foldingManager is null)
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                _foldingStrategy.UpdateFoldings(_foldingManager, AssociatedObject.Document);
            });
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
                InitFoldingManager();
            }
            else
            {
                ReleaseFoldingManager();
            }
        }

        private void InitFoldingManager()
        {
            if (_foldingManager is null && AssociatedObject?.TextArea is not null)
            {
                _foldingManager = FoldingManager.Install(AssociatedObject.TextArea);
            }
        }

        private void ReleaseFoldingManager()
        {
            if (_foldingManager is not null)
            {
                _foldingManager.Clear();
                FoldingManager.Uninstall(_foldingManager);
                _foldingManager = null;
            }
        }
    }
}
