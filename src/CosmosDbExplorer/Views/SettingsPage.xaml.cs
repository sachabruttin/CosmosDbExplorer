using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

using CosmosDbExplorer.ViewModels;

namespace CosmosDbExplorer.Views
{
    public partial class SettingsPage : Page
    {
        public List<string> Modifiers => new() { "", "Ctrl", "Alt" };

        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var txtBox = (TextBox)sender;
            var keys = new List<string?>();

            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                keys.Add("Ctrl");
            }
            if (e.KeyboardDevice.IsKeyDown(Key.LeftAlt) || e.KeyboardDevice.IsKeyDown(Key.RightAlt))
            {
                keys.Add("Alt");
            }
            if (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift))
            {
                keys.Add("Shift");
            }

            keys.Add(GetPressedKey(e.Key));

            if (keys.All(k => k is not null))
            {
                txtBox.Text = string.Join("+", keys);
            }

            e.Handled = true;
        }

        private static Key[] _modifiers = { Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.LeftShift, Key.RightShift };

        private static string? GetPressedKey(Key key)
        {
            if (_modifiers.Contains(key))
            {
                return null;
            }

            return key switch
            {
                Key.D1 => "1",
                Key.D2 => "2",
                Key.D3 => "3",
                Key.D4 => "4",
                Key.D5 => "5",
                Key.D6 => "6",
                Key.D7 => "7",
                Key.D8 => "8",
                Key.D9 => "9",
                Key.Return => "Enter",
                _ => key.ToString()
            };
        }
    }
}
