using System;
using System.Threading;

namespace CosmosDbExplorer.Core.Helpers
{
    public class TimedDebounce
    {
        public event EventHandler Idled = delegate { };
        public int WaitingMilliSeconds { get; set; }
        private readonly Timer _timer;

        public TimedDebounce(int waitingMilliSeconds = 600)
        {
            WaitingMilliSeconds = waitingMilliSeconds;
            _timer = new Timer(p => Idled(this, EventArgs.Empty));
        }

        public void DebounceEvent() => _timer.Change(WaitingMilliSeconds, Timeout.Infinite);
    }
}
