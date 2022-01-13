using ICSharpCode.AvalonEdit;
using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;

namespace CosmosDbExplorer.Behaviors
{
    public class AvalonTextEditorBindingBehavior : Behavior<TextEditor>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextChanged += AssociatedObjectTextChanged;
            AssociatedObject.TextArea.SelectionChanged += AssociatedObjectSelectionChanged;
            AssociatedObject.TextArea.Caret.PositionChanged += AssociatedObjectCaretPositionChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TextChanged -= AssociatedObjectTextChanged;
            base.OnDetaching();
            AssociatedObject.TextArea.SelectionChanged -= AssociatedObjectSelectionChanged;
            AssociatedObject.TextArea.Caret.PositionChanged -= AssociatedObjectCaretPositionChanged;
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(AvalonTextEditorBindingBehavior),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextPropertyChanged,
                    CoerceTextProperty));

        private static object CoerceTextProperty(DependencyObject d, object baseValue)
        {
            return baseValue ?? string.Empty;
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AvalonTextEditorBindingBehavior b)
            {
                b.OnTextChanged();
            }
        }

        private void OnTextChanged()
        {
            if (AssociatedObject is null)
            {
                return;
            }

            if (AssociatedObject.Text != Text)
            {
                AssociatedObject.Text = Text ?? string.Empty;
            }
        }

        private void AssociatedObjectTextChanged(object? sender, EventArgs e)
        {
            if (Text != AssociatedObject.Text)
            {
                SetCurrentValue(TextProperty, AssociatedObject.Text ?? string.Empty);
            }
        }

        public string SelectedText
        {
            get => (string)GetValue(SelectedTextProperty);
            set => SetValue(SelectedTextProperty, value);
        }

        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.Register(
                "SelectedText",
                typeof(string),
                typeof(AvalonTextEditorBindingBehavior),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedTextPropertyChanged));

        private static void OnSelectedTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AvalonTextEditorBindingBehavior b)
            {
                b.OnSelectedTextChanged();
            }
        }

        private void OnSelectedTextChanged()
        {
            if (AssociatedObject is null)
            {
                return;
            }

            if (AssociatedObject.SelectedText != SelectedText)
            {
                AssociatedObject.SelectedText = SelectedText ?? string.Empty;
            }
        }


        public (int start, int length) Selection
        {
            get { return ((int start, int length))GetValue(SelectionProperty); }
            set { SetValue(SelectionProperty, value); }
        }

        public static readonly DependencyProperty SelectionProperty =
            DependencyProperty.Register(
                "Selection",
                typeof((int start, int length)),
                typeof(AvalonTextEditorBindingBehavior),
                new FrameworkPropertyMetadata(
                    (0, 0),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectionPropertyChanged));

        private static void OnSelectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AvalonTextEditorBindingBehavior b)
            {
                b.OnSelectionChanged();
            }
        }

        private void OnSelectionChanged()
        {
            if (AssociatedObject is null)
            {
                return;
            }

            var associatedObjectSelection = (AssociatedObject.SelectionStart, AssociatedObject.SelectionLength);
            if (associatedObjectSelection != Selection)
            {
                var (start, end) = Selection;
                AssociatedObject.Select(start, end);
            }
        }

        private void AssociatedObjectSelectionChanged(object? sender, EventArgs e)
        {
            if (SelectedText != AssociatedObject.SelectedText)
            {
                SetCurrentValue(SelectedTextProperty, AssociatedObject.SelectedText ?? string.Empty);
            }

            var associatedObjectSelection = (AssociatedObject.SelectionStart, AssociatedObject.SelectionLength);
            if (Selection != associatedObjectSelection)
            {
                SetCurrentValue(SelectionProperty, associatedObjectSelection);
            }
        }

        public int CursorPosition
        {
            get { return (int)GetValue(CursorPositionProperty); }
            set { SetValue(CursorPositionProperty, value); }
        }

        public static readonly DependencyProperty CursorPositionProperty =
            DependencyProperty.Register(
                "CursorPosition",
                typeof(int),
                typeof(AvalonTextEditorBindingBehavior),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnCursorPositionPropertyChanged));

        private static void OnCursorPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AvalonTextEditorBindingBehavior b)
            {
                b.OnCursorPositionChanged();
            }
        }

        private void OnCursorPositionChanged()
        {
            if (AssociatedObject is null)
            {
                return;
            }

            if (AssociatedObject.CaretOffset != CursorPosition)
            {
                AssociatedObject.CaretOffset = CursorPosition;
            }
        }

        private void AssociatedObjectCaretPositionChanged(object? sender, EventArgs e)
        {
            if (CursorPosition != AssociatedObject.CaretOffset)
            {
                SetCurrentValue(CursorPositionProperty, AssociatedObject.CaretOffset);
            }
        }
    }
}
