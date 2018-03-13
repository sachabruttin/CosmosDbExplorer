using GalaSoft.MvvmLight;

namespace DocumentDbExplorer.Messages
{
    public class ActivePaneChangedMessage
    {
        public ActivePaneChangedMessage(ViewModelBase paneViewModel)
        {
            PaneViewModel = paneViewModel;
        }

        public ViewModelBase PaneViewModel { get; }
    }
}
