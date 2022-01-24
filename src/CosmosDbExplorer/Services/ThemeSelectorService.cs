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
                UpdateHighlightingColor(theme);
            }

            App.Current.Properties["Theme"] = theme.ToString();
        }

        public AppTheme GetCurrentTheme()
        {
            if (App.Current.Properties.Contains("Theme"))
            {
                var themeName = App.Current.Properties["Theme"].ToString();
                Enum.TryParse(themeName, out AppTheme theme);
                return theme;
            }

            return AppTheme.Default;
        }

        private void UpdateHighlightingColor(AppTheme theme)
        {
            UpdateJsonHighlightingColor(theme);
            UpdateCosomosSqlHighlightingColor(theme);
        }

        private static void UpdateJsonHighlightingColor(AppTheme theme)
        {
            var definition = HighlightingManager.Instance.GetDefinition("JSON");
            var colors = new[] { "Bool", "Number", "String", "Null", "FieldName", "Object", "Array", "Punctuation" };

            foreach (var color in colors)
            {
                var sourceColor = $"{theme}.{color}";
                definition.GetNamedColor(color).MergeWith(definition.GetNamedColor(sourceColor));
            }
        }

        private static void UpdateCosomosSqlHighlightingColor(AppTheme theme)
        {
            var definition = HighlightingManager.Instance.GetDefinition("DocumentDbSql");
            //var colors = new[] { "Digits", "Comment", "Punctuation", "String", "String2", 
            //                     "Keyword", "Function", "MethodCall", "Variable", "Variable1", 
            //                     "ObjectReference", "ObjectReference1", "ObjectReferenceInBrackets", 
            //                     "ObjectReferenceInBrackets1", "CommentsMarkerSetTodo", "CommentsMarkerSetHackUndone" };

            //definition.NamedHighlightingColors.Where(c => !c.Name.Contains("."))

            foreach (var color in definition.NamedHighlightingColors.Where(c => !c.Name.Contains('.')).Select(c => c.Name))
            {
                var sourceColor = $"{theme}.{color}";
                definition.GetNamedColor(color).MergeWith(definition.GetNamedColor(sourceColor));
            }
        }
    }
}
