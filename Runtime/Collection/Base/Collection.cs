using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Darklight.Collection;

namespace Darklight.Collection
{
    /// <summary>
    /// Abstract class for a library.
    /// </summary>
    public abstract class Collection
        : IEnumerable<CollectionItem>,
            IEquatable<Collection>,
            INotifyCollectionChanged,
            IDisposable
    {

        /// <summary>
        /// The event handler for the collection changed event.
        /// </summary>
        private EventHandler<CollectionEventArgs> _collectionChanged;

        /// <summary>
        /// The event handler for the collection changing event.
        /// </summary>
        private EventHandler<CollectionEventArgs> _collectionChanging;
        private List<CollectionItem> _items = new();

        private int _position = -1;

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event EventHandler<CollectionEventArgs> OnCollectionChanged
        {
            add { _collectionChanged += value; }
            remove { _collectionChanged -= value; }
        }

        /// <summary>
        /// Occurs before the collection changes.
        /// </summary>
        public event EventHandler<CollectionEventArgs> OnCollectionChanging
        {
            add { _collectionChanging += value; }
            remove { _collectionChanging -= value; }
        }

        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add =>
                _collectionChanged += (_, args) =>
                    value(
                        this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
                    );
            remove =>
                _collectionChanged -= (_, args) =>
                    value(
                        this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
                    );
        }
        public abstract int Capacity { get; }
        public abstract int Count { get; }
        public abstract bool IsReadOnly { get; }
        public abstract bool IsSynchronized { get; }
        public abstract object SyncRoot { get; }

        public object Current => Items.ElementAtOrDefault(_position);
        public virtual IEnumerable<int> IDs => Items.Select(x => x.Id);
        public virtual IEnumerable<CollectionItem> Items => _items;
        public virtual IEnumerable<object> Objects => Items.Select(x => x.Object);

        /// <summary>
        /// Gets whether events are currently suspended.
        /// </summary>
        protected bool EventsSuspended { get; private set; }

        public void Add(CollectionItem item)
        {
            CollectionChanging(
                new CollectionEventArgs(CollectionEventType.ADD, item, null, item.Id)
            );
            var items = Items.ToList();
            items.Add(item);
            CollectionChanged(
                new CollectionEventArgs(CollectionEventType.ADD, item, null, item.Id)
            );
        }

        public void AddRange(IEnumerable<CollectionItem> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public void AddRange(params CollectionItem[] items)
        {
            AddRange(items as IEnumerable<CollectionItem>);
        }

        public abstract void AddDefaultItem();

        public void Clear()
        {
            CollectionChanging(new CollectionEventArgs(CollectionEventType.RESET));
            var items = Items.ToList();
            items.Clear();
            CollectionChanged(new CollectionEventArgs(CollectionEventType.RESET));
        }

        public Collection Clone()
        {
            var clone = (Collection)MemberwiseClone();
            return clone;
        }

        public bool Contains(CollectionItem item)
        {
            return Items.Contains(item);
        }

        public void CopyTo(CollectionItem[] array, int arrayIndex)
        {
            var items = Items.ToList();
            items.CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            var items = Items.ToList();
            Array.Copy(items.ToArray(), 0, array, index, items.Count);
        }

        public void Dispose()
        {
            OnCollectionChanged -= (_, __) => { };
            OnCollectionChanging -= (_, __) => { };
            GC.SuppressFinalize(this);
        }

        public bool Equals(Collection other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Count == other.Count && Items.SequenceEqual(other.Items);
        }

        public IEnumerator<CollectionItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public CollectionItem GetItemById(int id)
        {
            return TryGetItem(id);
        }

        public IEnumerable<CollectionItem> GetItemsInRange(int startIndex, int count)
        {
            return Items.Skip(startIndex).Take(count);
        }

        public int IndexOf(CollectionItem item)
        {
            return Items.ToList().IndexOf(item);
        }

        public void Insert(int index, CollectionItem item)
        {
            CollectionChanging(
                new CollectionEventArgs(CollectionEventType.ADD, item, null, item.Id, index)
            );
            var items = Items.ToList();
            items.Insert(index, item);
            CollectionChanged(
                new CollectionEventArgs(CollectionEventType.ADD, item, null, item.Id, index)
            );
        }

        public bool MoveNext()
        {
            var items = Items.ToList();
            if (_position < items.Count - 1)
            {
                _position++;
                return true;
            }
            return false;
        }

        public virtual void Refresh()
        {
            _items = Items.ToList();
        }

        public bool Remove(CollectionItem item)
        {
            var items = Items.ToList();
            var index = items.IndexOf(item);
            if (index >= 0)
            {
                CollectionChanging(
                    new CollectionEventArgs(CollectionEventType.REMOVE, item, null, item.Id, index)
                );
                items.Remove(item);
                CollectionChanged(
                    new CollectionEventArgs(CollectionEventType.REMOVE, item, null, item.Id, index)
                );
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            var items = Items.ToList();
            var item = items[index];
            CollectionChanging(
                new CollectionEventArgs(CollectionEventType.REMOVE, item, null, item.Id, index)
            );
            items.RemoveAt(index);
            CollectionChanged(
                new CollectionEventArgs(CollectionEventType.REMOVE, item, null, item.Id, index)
            );
        }

        public void RemoveRange(IEnumerable<CollectionItem> items)
        {
            foreach (var item in items)
                Remove(item);
        }

        public void RemoveRange(params CollectionItem[] items)
        {
            RemoveRange(items as IEnumerable<CollectionItem>);
        }

        public void RemoveWhere(Predicate<CollectionItem> predicate)
        {
            var items = Items.ToList();
            items.RemoveAll(predicate);
        }

        public void Reset()
        {
            _position = -1;
        }

        /// <summary>
        /// Suspends collection change events.
        /// </summary>
        /// <returns>An IDisposable that resumes events when disposed.</returns>
        public IDisposable SuspendEvents()
        {
            return new EventSuspender(this);
        }

        public CollectionItem TryGetItem(int id)
        {
            return Items.FirstOrDefault(x => x.Id == id);
        }

        public bool TryGetItem(int id, out CollectionItem item)
        {
            item = TryGetItem(id);
            return item != null;
        }

        public IEnumerable<CollectionItem> Where(Func<CollectionItem, bool> predicate)
        {
            return Items.Where(predicate);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Raises the CollectionChanged event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void CollectionChanged(CollectionEventArgs args)
        {
            _collectionChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the CollectionChanging event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void CollectionChanging(CollectionEventArgs args)
        {
            _collectionChanging?.Invoke(this, args);
        }

        private class EventSuspender : IDisposable
        {
            private readonly Collection _collection;
            private bool _disposed;

            public EventSuspender(Collection collection)
            {
                _collection = collection;
                _collection.EventsSuspended = true;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _collection.EventsSuspended = false;
                    _disposed = true;
                }
            }
        }
    }
}
