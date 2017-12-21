using System.Windows;
using System.Windows.Controls;
using DocumentDbExplorer.ViewModel;
using Xceed.Wpf.AvalonDock.Layout;

namespace DocumentDbExplorer.Infrastructure
{
    public class DocumentHeaderTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate TextIconTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var layout = (LayoutDocument)item;

            if (layout.Content is ImportDocumentViewModel)
            {
                return TextIconTemplate;
            }
            else
            {
                return ImageTemplate;
            }
        }
    }
}
