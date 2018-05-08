using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CosmosDbExplorer.Infrastructure
{
    public class CustomTraceListener : TraceListener
    {
        private readonly TextBox _textBox;

        public CustomTraceListener(TextBox textBox)
        {
            _textBox = textBox;
            _textBox.TextChanged += (s, e) =>
            {
                var oldFocusedElement = FocusManager.GetFocusedElement(_textBox.Parent);

                _textBox.Focus();
                _textBox.CaretIndex = _textBox.Text.Length;
                _textBox.ScrollToEnd();

                FocusManager.SetFocusedElement(_textBox.Parent, oldFocusedElement);
            };
        }

        public override void Write(string message)
        {
            _textBox.Dispatcher.Invoke(() => _textBox.AppendText(message));
        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }
    }
}
