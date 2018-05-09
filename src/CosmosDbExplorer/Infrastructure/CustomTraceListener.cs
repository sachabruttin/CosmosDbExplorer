using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;

namespace CosmosDbExplorer.Infrastructure
{
    public class CustomTraceListener : TraceListener
    {
        private readonly string _name;
        private readonly TextBox _textBox;

        public CustomTraceListener(TextBox textBox)
        {
            _name = Path.GetFileName(Assembly.GetEntryAssembly().Location);

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
            _textBox.Dispatcher.Invoke(() =>
            {
                if (message.StartsWith(_name))
                {
                    message = message.Remove(0, _name.Length);
                }

                _textBox.AppendText(message);
            });
        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }
    }
}
