using System.Windows;
using System.Windows.Controls;

namespace CosmosDbExplorer.Infrastructure
{
    [TemplatePart(Name=PartMoreButtonName, Type = typeof(Button))]
    public class ListBoxMore : ListBox
    {
        private const string PartMoreButtonName = "PART_MoreButton";
        private Button _moreButton;

        public override void OnApplyTemplate()
        {
            // unhook any handlers from _moreButton to prevent a memory leak

            _moreButton = GetTemplateChild(PartMoreButtonName) as Button;

            // do stuff with _moreButton, register handlers, etc.
        }
    }
}
