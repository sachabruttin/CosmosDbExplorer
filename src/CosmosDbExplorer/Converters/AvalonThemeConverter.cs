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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var themeName = value as string;

            return themeName switch
            {
                "Dark" => new AvalonDock.Themes.Vs2013DarkTheme(),
                "Light" => new AvalonDock.Themes.GenericTheme(),
                _ => WindowsThemeHelper.AppsUseLightTheme() ? new AvalonDock.Themes.GenericTheme() : new AvalonDock.Themes.Vs2013DarkTheme()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
