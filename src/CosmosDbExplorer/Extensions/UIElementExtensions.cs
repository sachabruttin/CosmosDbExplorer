using System.Windows;
using System.Windows.Media;

namespace CosmosDbExplorer.Extensions
{
    public static class UIElementExtensions
    {
        public static TAncestor? GetAncestorOrSelf<TAncestor>(this UIElement element)
            where TAncestor : UIElement
        {
            var uiElement = element;

            while (uiElement != null && uiElement is not TAncestor)
            {
                uiElement = VisualTreeHelper.GetParent(uiElement) as UIElement;
            }

            return uiElement as TAncestor;
        }
    }
}
