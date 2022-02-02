using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace CosmosDbExplorer.Models
{
    public class CheckedItem<T> : ObservableObject
    {
        public CheckedItem(T item, bool isChecked = false)
        {
            Item = item;
            IsChecked = isChecked;
        }

        public T Item { get; }

        public bool IsChecked { get; set; }
    }
}
