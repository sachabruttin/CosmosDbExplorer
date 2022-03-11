using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ControlzEx.Theming;
using CosmosDbExplorer.Helpers;

namespace CosmosDbExplorer.Converters
{
    // TODO: Temporary solution. Must find a way to integrate AvalonDock with MahaApps Theme manager
    public class AvalonThemeConverter : IValueConverter
    {
        private object? _darkTheme;
        private object? _lightTheme;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var themeName = value as string;

            return themeName switch
            {
                "Dark" => GetDarkTheme(),
                "Light" => GetLightTheme(),
                _ => WindowsThemeHelper.AppsUseLightTheme() ? GetLightTheme() : GetDarkTheme()
            };
        }

        private object GetDarkTheme() => _darkTheme ??= new AvalonDock.Themes.Vs2013DarkTheme();

        private object GetLightTheme() => _lightTheme ??= new AvalonDock.Themes.GenericTheme();


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
