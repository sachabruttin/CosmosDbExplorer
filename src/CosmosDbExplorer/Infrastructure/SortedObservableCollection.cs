using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CosmosDbExplorer.Infrastructure
{
    public class SortedObservableCollection<TValue>
        : ObservableCollection<TValue>
    {
        private readonly IComparer<TValue> _comparer;

        public SortedObservableCollection(IComparer<TValue> comparer)
        {
            _comparer = comparer ?? throw new System.ArgumentNullException(nameof(comparer));
        }

        public SortedObservableCollection(IEnumerable<TValue> collection, IComparer<TValue> comparer)
            : base(collection)
        {
            _comparer = comparer ?? throw new System.ArgumentNullException(nameof(comparer));
        }

        public SortedObservableCollection(List<TValue> list, IComparer<TValue> comparer)
            : base(list)
        {
            _comparer = comparer ?? throw new System.ArgumentNullException(nameof(comparer));
        }

        protected override void InsertItem(int index, TValue item)
        {
            index = this.TakeWhile(i => _comparer.Compare(item, i) > 0).Count();
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, TValue item)
        {
            RemoveAt(index);
            InsertItem(default, item);
        }
    }
}
