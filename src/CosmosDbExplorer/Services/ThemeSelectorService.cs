using System;
using System.Windows;
using System.Linq;
using ControlzEx.Theming;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Models;
using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro.Theming;

namespace CosmosDbExplorer.Services
{
    public class ThemeSelectorService : IThemeSelectorService
    {
        private const string HcDarkTheme = "pack://application:,,,/Styles/Themes/HC.Dark.Blue.xaml";
        private const string HcLightTheme = "pack://application:,,,/Styles/Themes/HC.Light.Blue.xaml";

        public ThemeSelectorService()
        {
        }

        public void InitializeTheme()
        {
            // TODO WTS: Mahapps.Metro supports syncronization with high contrast but you have to provide custom high contrast themes
            // We've added basic high contrast dictionaries for Dark and Light themes
            // Please complete these themes following the docs on https://mahapps.com/docs/themes/thememanager#creating-custom-themes
            ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(HcDarkTheme), MahAppsLibraryThemeProvider.DefaultInstance));
            ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(HcLightTheme), MahAppsLibraryThemeProvider.DefaultInstance));

            var theme = GetCurrentTheme();
            SetTheme(theme);
        }

        public void SetTheme(AppTheme theme)
        {
            if (theme == AppTheme.Default)
            {
                ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
                ThemeManager.Current.SyncTheme();
            }
            else
            {
                ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithHighContrast;
                ThemeManager.Current.SyncTheme();
                ThemeManager.Current.ChangeTheme(Application.Current, $"{theme}.Blue", SystemParameters.HighContrast);
            }

            UpdateHighlightingColor(theme);
            Properties.Settings.Default.Theme = theme.ToString();
        }

        public AppTheme GetCurrentTheme()
        {
            return Enum.Parse<AppTheme>(Properties.Settings.Default.Theme);
        }

        private void UpdateHighlightingColor(AppTheme theme)
        {
            if (theme == AppTheme.Default)
            {
                theme = WindowsThemeHelper.AppsUseLightTheme() ? AppTheme.Light : AppTheme.Dark;
            }

            UpdateHighlightingColor(HighlightingManager.Instance.GetDefinition("JSON"), theme);
            UpdateHighlightingColor(HighlightingManager.Instance.GetDefinition("DocumentDbSql"), theme);
        }

        private static void UpdateHighlightingColor(IHighlightingDefinition definition, AppTheme theme)
        {
            foreach (var color in definition.NamedHighlightingColors.Where(c => !c.Name.Contains('.')).Select(c => c.Name))
            {
                var sourceColor = $"{theme}.{color}";
                definition.GetNamedColor(color).MergeWith(definition.GetNamedColor(sourceColor));
            }
        }

    }
}
