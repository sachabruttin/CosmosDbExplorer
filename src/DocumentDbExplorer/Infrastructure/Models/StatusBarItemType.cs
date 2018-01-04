using System.Windows.Controls;

namespace DocumentDbExplorer.Infrastructure.Models
{
    public class StatusBarItem
    {
        public StatusBarItem(object dataContext, StatusBarItemType type, Dock dock)
        {
            DataContext = dataContext;
            Type = type;
            Dock = dock;
        }

        public object DataContext { get; set; }
        public StatusBarItemType Type { get; set; }
        public Dock Dock { get; set; } 
    }

    public enum StatusBarItemType
    {
        UsedMemory,
        Zoom
    }
}
