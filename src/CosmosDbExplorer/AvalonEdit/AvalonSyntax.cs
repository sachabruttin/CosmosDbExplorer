using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace CosmosDbExplorer.AvalonEdit
{
    internal class AvalonSyntax
    {
        public static void LoadHighlighting()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var regex = new Regex(@"CosmosDbExplorer\.AvalonEdit\.(?<syntaxName>[a-zA-Z0-9-_]+).xshd");
            var resourceNames = assembly.GetManifestResourceNames();
            foreach (var resourceName in resourceNames)
            {
                var match = regex.Match(resourceName);
                if (!match.Success)
                {
                    continue;
                }

                var syntaxName = match.Groups["syntaxName"].Value;
                using var stream = assembly.GetManifestResourceStream(resourceName)!;
                using var reader = XmlReader.Create(stream);
                var definition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                HighlightingManager.Instance.RegisterHighlighting(syntaxName, Array.Empty<string>(), definition);
            }
        }
    }
}
