using DocumentDbExplorer.ViewModel;

namespace DocumentDbExplorer.Messages
{
    public class OpenScaleAndSettingsViewMessage
    {
        public OpenScaleAndSettingsViewMessage(ScaleSettingsNodeViewModel node)
        {
            Node = node;
        }

        public ScaleSettingsNodeViewModel Node { get; }
    }
}
