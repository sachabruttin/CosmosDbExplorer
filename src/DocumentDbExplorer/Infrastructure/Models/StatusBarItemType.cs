using System.Windows.Controls;
using System.Windows.Data;
using GalaSoft.MvvmLight;

namespace DocumentDbExplorer.Infrastructure.Models
{
    public class StatusBarItem : ObservableObject
    {
        public StatusBarItem(object dataContext, StatusBarItemType type, string title = null, Dock dock = Dock.Left)
        {
            DataContext = dataContext;
            Type = type;
            Title = title;
            Dock = dock;
        }

        public object DataContext { get; set; }
        public StatusBarItemType Type { get; set; }
        public string Title { get; set; }
        public Dock Dock { get; set; } 
    }

    public enum StatusBarItemType
    {
        UsedMemory,
        Zoom,
        SimpleText
    }
}
