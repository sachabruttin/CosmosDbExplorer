using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using ControlzEx.Theming;

using MahApps.Metro.Theming;

namespace CosmosDbExplorer.Styles
{
    public class CosmosDbExplorerThemeProvider : /*MahAppsLibraryThemeProvider*/LibraryThemeProvider
    {
        public static readonly CosmosDbExplorerThemeProvider DefaultInstance = new();

        public CosmosDbExplorerThemeProvider()
            : base(true)
        {

        }

        public override void FillColorSchemeValues(Dictionary<string, string> values, RuntimeThemeColorValues colorValues)
        {
            // Check if all needed parameters are not null
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (colorValues is null)
            {
                throw new ArgumentNullException(nameof(colorValues));
            }

            //var isDarkMode = colorValues.Options.BaseColorScheme.Name == ThemeManager.BaseColorDark;

            //values.Add("CosmosDbExplorer.AvalonEdit.LinkTextForegroundBrush", isDarkMode ? Color.FromRgb(155, 109, 90).ToString() : Colors.CornflowerBlue.ToString());

            //base.FillColorSchemeValues(values, colorValues);
        }
    }
}
