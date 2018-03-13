namespace DocumentDbExplorer.Messages
{
    public class IsBusyMessage
    {
        public IsBusyMessage(bool isBusy)
        {
            IsBusy = isBusy;
        }

        public bool IsBusy { get; }
    }
}
