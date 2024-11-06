using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Darklight.UnityExt.Collection
{
    public enum CollectionEventType
    {
        ADD,
        REMOVE,
        CLEAR,
        UPDATE,
        REPLACE,
        RESET,
        SORT,
        BATCH_ADD,
        BATCH_REMOVE,
        INITIALIZE,
        DISPOSE
    }

    public class CollectionEventArgs : EventArgs
    {
        public CollectionEventType EventType { get; }
        public ICollectionItem Item { get; }
        public IEnumerable<ICollectionItem> Items { get; }
        public int? AffectedId { get; }
        public int? Index { get; }

        public CollectionEventArgs(
            CollectionEventType eventType,
            ICollectionItem item = null,
            IEnumerable<ICollectionItem> items = null,
            int? affectedId = null,
            int? index = null
        )
        {
            EventType = eventType;
            Item = item;
            Items = items;
            AffectedId = affectedId;
            Index = index;
        }
    }

    /// <summary>
    /// Abstract class for a library.
    /// </summary>
    [Serializable]
    public abstract class CollectionLibrary
        : ICollection<ICollectionItem>,
            IEnumerable<ICollectionItem>,
            IEnumerable,
            ICollection,
            IList<ICollectionItem>,
            IEquatable<CollectionLibrary>,
            IDisposable
    {
        public abstract IEnumerable<ICollectionItem> Items { get; }
        public abstract int Count { get; }
        public abstract int Capacity { get; }
        public abstract bool IsReadOnly { get; }
        public abstract object SyncRoot { get; }
        public abstract bool IsSynchronized { get; }

        private EventHandler<CollectionEventArgs> _collectionChanged;
        private EventHandler<CollectionEventArgs> _collectionChanging;

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

        /// <summary>
        /// Raises the CollectionChanging event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void CollectionChanging(CollectionEventArgs args)
        {
            _collectionChanging?.Invoke(this, args);
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
        /// Gets whether events are currently suspended.
        /// </summary>
        protected bool EventsSuspended { get; private set; }

        /// <summary>
        /// Suspends collection change events.
        /// </summary>
        /// <returns>An IDisposable that resumes events when disposed.</returns>
        public IDisposable SuspendEvents()
        {
            return new EventSuspender(this);
        }

        private class EventSuspender : IDisposable
        {
            private readonly CollectionLibrary _collection;
            private bool _disposed;

            public EventSuspender(CollectionLibrary collection)
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

        #region ---- < ICollection Implementation > ---------------------------------
        /// <summary>
        /// Adds an item to the ICollection.
        /// </summary>
        /// <param name="item">The object to add to the ICollection.</param>
        public abstract void Add(ICollectionItem item);

        /// <summary>
        /// Determines whether the ICollection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the ICollection.</param>
        /// <returns>true if item is found in the ICollection; otherwise, false.</returns>
        public abstract bool Contains(ICollectionItem item);

        /// <summary>
        /// Removes the first occurrence of a specific object from the ICollection.
        /// </summary>
        /// <param name="item">The object to remove from the ICollection.</param>
        /// <returns>true if item was successfully removed from the ICollection; otherwise, false. This method also returns false if item is not found in the original ICollection.</returns>
        public abstract bool Remove(ICollectionItem item);

        /// <summary>
        /// Copies the elements of the ICollection to an ILibraryItem[], starting at a particular ILibraryItem[] index.
        /// </summary>
        /// <param name="array">The one-dimensional ILibraryItem[] that is the destination of the elements copied from ICollection.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public abstract void CopyTo(ICollectionItem[] array, int arrayIndex);

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public abstract void CopyTo(Array array, int index);

        /// <summary>
        /// Removes all items from the ICollection.
        /// </summary>
        public abstract void Clear();

        #endregion ---- < ICollection Implementation > ---------------------------------

        #region ---- < IEnumerator Implementation > ---------------------------------

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public abstract IEnumerator<ICollectionItem> GetEnumerator();
        #endregion ---- < IEnumerator Implementation > ---------------------------------


        #region ---- < IList Implementation > ---------------------------------
        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The item at the specified index.</returns>

        public abstract ICollectionItem this[int index] { get; set; }

        /// <summary>
        /// Gets the index of the specified item in the collection.
        /// </summary>
        /// <param name="item">The item to locate.</param>
        /// <returns>The index of the item if found; otherwise, -1.</returns>
        public abstract int IndexOf(ICollectionItem item);

        /// <summary>
        /// Inserts an item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which to insert the item.</param>
        /// <param name="item">The item to insert.</param>
        public abstract void Insert(int index, ICollectionItem item);

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public abstract void RemoveAt(int index);

        #endregion ---- < IList Implementation > ---------------------------------


        #region ---- < IEquatable Implementation > ---------------------------------
        /// <summary>
        /// Determines whether the current collection is equal to another collection.
        /// </summary>
        /// <param name="other">The collection to compare with the current collection.</param>
        /// <returns>true if the current collection is equal to the other collection; otherwise, false.</returns>
        public abstract bool Equals(CollectionLibrary other);
        #endregion ---- < IEquatable Implementation > ---------------------------------

        #region ---- < IDisposable Implementation > ---------------------------------
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();
        #endregion ---- < IDisposable Implementation > ---------------------------------

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Adds or updates an item in the collection.
        /// </summary>
        /// <param name="item">The item to add or update.</param>
        /// <returns>True if the item was added, false if it was updated.</returns>
        public abstract bool AddOrUpdate(ICollectionItem item);

        /// <summary>
        /// Adds a range of items to the collection.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public abstract void AddRange(IEnumerable<ICollectionItem> items);

        /// <summary>
        /// Removes a range of items from the collection.
        /// </summary>
        /// <param name="items">The items to remove.</param>
        public abstract void RemoveRange(IEnumerable<ICollectionItem> items);

        /// <summary>
        /// Replaces an item in the collection with a new item.
        /// </summary>
        /// <param name="item">The item to replace.</param>
        /// <param name="newItem">The new item to replace the old item with.</param>
        public abstract void Replace(ICollectionItem item, ICollectionItem newItem);

        /// <summary>
        /// Removes all items from the collection that match the predicate.
        /// </summary>
        /// <param name="predicate">The condition to test items against.</param>
        public abstract void RemoveWhere(Func<ICollectionItem, bool> predicate);

        /// <summary>
        /// Generates a hash code for the collection based on its items.
        /// </summary>
        /// <returns>A hash code that represents the current collection state.</returns>
        /// <remarks>
        /// The hash is computed using a combination of all item hashes in the collection.
        /// For consistent hashing behavior, ensure all ICollectionItems implement GetHashCode properly.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked // Allow arithmetic overflow
            {
                int hash = 17; // Prime number starting point
                foreach (var item in Items)
                {
                    hash = hash * 31 + (item?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }

        /// <summary>
        /// Gets all IDs in the collection.
        /// </summary>
        public abstract IEnumerable<int> IDs { get; }

        /// <summary>
        /// Gets all values in the collection.
        /// </summary>
        public abstract IEnumerable<object> Values { get; }

        /// <summary>
        /// Tries to get an item by its ID.
        /// </summary>
        /// <param name="id">The ID to look for.</param>
        /// <param name="item">The found item, if any.</param>
        /// <returns>True if the item was found, false otherwise.</returns>
        public abstract bool TryGetItem(int id, out ICollectionItem item);
    }

    [Serializable]
    public class CollectionLibrary<TValue> : CollectionLibrary
    {
        private readonly ConcurrentDictionary<int, ICollectionItem> _items;
        private readonly ReaderWriterLockSlim _lock;
        private bool _isInitialized;

        public CollectionLibrary()
        {
            _items = new ConcurrentDictionary<int, ICollectionItem>();
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _isInitialized = true;
            // Raise initialization event
            CollectionChanged(new CollectionEventArgs(CollectionEventType.INITIALIZE));
        }

        public override IEnumerable<ICollectionItem> Items => _items.Values;
        public override int Count => _items.Count;
        public override int Capacity => _items.Count;
        public override bool IsReadOnly => false;
        public override object SyncRoot => _lock;
        public override bool IsSynchronized => true;

        public override ICollectionItem this[int index]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.ElementAt(index);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    var key = _items.Keys.ElementAt(index);
                    _items[key] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public override void Add(ICollectionItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _lock.EnterWriteLock();
            try
            {
                var args = new CollectionEventArgs(CollectionEventType.ADD, item, affectedId: item.Id);

                if (!EventsSuspended)
                    CollectionChanging(args);

                if (!_items.TryAdd(item.Id, item))
                    throw new ArgumentException($"An item with ID {item.Id} already exists");

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override bool Remove(ICollectionItem item)
        {
            if (item == null) return false;

            _lock.EnterWriteLock();
            try
            {
                var args = new CollectionEventArgs(
                    CollectionEventType.REMOVE,
                    item,
                    affectedId: item.Id
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                bool result = _items.TryRemove(item.Id, out _);

                if (!EventsSuspended && result)
                    CollectionChanged(args);

                return result;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                var items = _items.Values.ToList();
                var args = new CollectionEventArgs(CollectionEventType.CLEAR, items: items);

                if (!EventsSuspended)
                    CollectionChanging(args);

                _items.Clear();

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override bool Contains(ICollectionItem item) =>
            item != null && _items.ContainsKey(item.Id);

        public override void CopyTo(ICollectionItem[] array, int arrayIndex)
        {
            _lock.EnterReadLock();
            try
            {
                _items.Values.CopyTo(array, arrayIndex);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void CopyTo(Array array, int index)
        {
            _lock.EnterReadLock();
            try
            {
                ((ICollection)_items.Values).CopyTo(array, index);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override IEnumerator<ICollectionItem> GetEnumerator() =>
            _items.Values.GetEnumerator();

        public override void Dispose()
        {
            _lock.EnterWriteLock();
            try
            {
                var args = new CollectionEventArgs(CollectionEventType.DISPOSE);

                if (!EventsSuspended)
                    CollectionChanging(args);

                _items.Clear();

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
                _lock.Dispose();
            }
        }

        public override bool Equals(CollectionLibrary other)
        {
            _lock.EnterReadLock();
            try
            {
                if (other == null)
                    return false;

                return _items.Equals(other.Items);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override IEnumerable<int> IDs
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Keys.ToList();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public override IEnumerable<object> Values
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.Select(x => x.Value).ToList();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets a strongly-typed value by ID.
        /// </summary>
        /// <param name="id">The ID of the item.</param>
        /// <returns>The value cast to TValue.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the ID is not found.</exception>
        /// <exception cref="InvalidCastException">Thrown when the value cannot be cast to TValue.</exception>
        public TValue GetValueById(int id)
        {
            _lock.EnterReadLock();
            try
            {
                if (_items.TryGetValue(id, out var item))
                {
                    return (TValue)item.Value;
                }
                throw new KeyNotFoundException($"No item found with ID: {id}");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Tries to get an item by its ID.
        /// </summary>
        /// <param name="id">The ID to look for.</param>
        /// <param name="item">The found item, if any.</param>
        /// <returns>True if the item was found, false otherwise.</returns>
        public override bool TryGetItem(int id, out ICollectionItem item)
        {
            _lock.EnterReadLock();
            try
            {
                return _items.TryGetValue(id, out item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Tries to get a value by its ID.
        /// </summary>
        /// <param name="id">The ID to look for.</param>
        /// <param name="value">The found value, if any.</param>
        /// <returns>True if the value was found, false otherwise.</returns>
        public bool TryGetValue(int id, out TValue value)
        {
            _lock.EnterReadLock();
            try
            {
                if (_items.TryGetValue(id, out var item))
                {
                    value = (TValue)item.Value;
                    return true;
                }
                value = default;
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Adds or updates an item in the collection.
        /// </summary>
        /// <param name="item">The item to add or update.</param>
        /// <returns>True if the item was added, false if it was updated.</returns>
        public override bool AddOrUpdate(ICollectionItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _lock.EnterWriteLock();
            try
            {
                bool isNew = !_items.ContainsKey(item.Id);
                var eventType = isNew ? CollectionEventType.ADD : CollectionEventType.UPDATE;
                var args = new CollectionEventArgs(eventType, item, affectedId: item.Id);

                if (!EventsSuspended)
                    CollectionChanging(args);

                _items[item.Id] = item;

                if (!EventsSuspended)
                    CollectionChanged(args);

                return isNew;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets a value by ID or returns a default value if not found.
        /// </summary>
        /// <param name="id">The ID to look for.</param>
        /// <param name="defaultValue">The default value to return if not found.</param>
        /// <returns>The found value or the default value.</returns>
        public TValue GetValueOrDefault(int id, TValue defaultValue = default)
        {
            _lock.EnterReadLock();
            try
            {
                return _items.TryGetValue(id, out var item) ? (TValue)item.Value : defaultValue;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Creates a new collection containing only items that satisfy the predicate.
        /// </summary>
        /// <param name="predicate">The condition to test items against.</param>
        /// <returns>A new filtered collection.</returns>
        public CollectionLibrary<TValue> Where(Func<ICollectionItem, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                var newCollection = new CollectionLibrary<TValue>();
                foreach (var item in _items.Values.Where(predicate))
                {
                    newCollection.Add(item);
                }
                return newCollection;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Determines if any item matches the given predicate.
        /// </summary>
        /// <param name="predicate">The condition to test items against.</param>
        /// <returns>True if any item matches, false otherwise.</returns>
        public bool Any(Func<ICollectionItem, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _items.Values.Any(predicate);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets the first item matching the predicate, or null if none found.
        /// </summary>
        /// <param name="predicate">The condition to test items against.</param>
        /// <returns>The first matching item or null.</returns>
        public ICollectionItem FirstOrDefault(Func<ICollectionItem, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _items.Values.FirstOrDefault(predicate);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Adds a range of items to the collection.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when items is null.</exception>
        public override void AddRange(IEnumerable<ICollectionItem> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _lock.EnterWriteLock();
            try
            {
                var itemsList = items.ToList();
                var args = new CollectionEventArgs(CollectionEventType.BATCH_ADD, items: itemsList);

                if (!EventsSuspended)
                    CollectionChanging(args);

                using (SuspendEvents())
                {
                    foreach (var item in itemsList)
                    {
                        Add(item);
                    }
                }

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void RemoveRange(IEnumerable<ICollectionItem> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _lock.EnterWriteLock();
            try
            {
                var itemsList = items.ToList();
                var args = new CollectionEventArgs(CollectionEventType.BATCH_REMOVE, items: itemsList);

                if (!EventsSuspended)
                    CollectionChanging(args);

                using (SuspendEvents())
                {
                    foreach (var item in itemsList)
                    {
                        Remove(item);
                    }
                }

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Replace(ICollectionItem item, ICollectionItem newItem)
        {
            if (item == null || newItem == null)
                throw new ArgumentNullException(nameof(item));

            _lock.EnterWriteLock();
            try
            {
                var args = new CollectionEventArgs(
                    CollectionEventType.REPLACE,
                    item: newItem,
                    items: new[] { item, newItem },
                    affectedId: item.Id
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                _items[item.Id] = newItem;

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public CollectionLibrary<TValue> Clone()
        {
            _lock.EnterReadLock();
            try
            {
                var clone = new CollectionLibrary<TValue>();
                clone.AddRange(Items);
                return clone;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void RemoveWhere(Func<ICollectionItem, bool> predicate)
        {
            _lock.EnterWriteLock();
            try
            {
                var itemsToRemove = _items.Values.Where(predicate).ToList();
                if (!itemsToRemove.Any())
                    return;

                var args = new CollectionEventArgs(
                    CollectionEventType.BATCH_REMOVE,
                    items: itemsToRemove
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                using (SuspendEvents())
                {
                    foreach (var item in itemsToRemove)
                    {
                        _items.TryRemove(item.Id, out _);
                    }
                }

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Determines if the collection contains an item with the specified ID.
        /// </summary>
        /// <param name="id">The ID to check for.</param>
        /// <returns>True if an item with the specified ID exists, false otherwise.</returns>
        public bool ContainsId(int id)
        {
            _lock.EnterReadLock();
            try
            {
                return _items.ContainsKey(id);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets all items with IDs in the specified range.
        /// </summary>
        /// <param name="startId">The inclusive start ID.</param>
        /// <param name="endId">The inclusive end ID.</param>
        /// <returns>A collection of items within the ID range.</returns>
        public IEnumerable<ICollectionItem> GetItemsInRange(int startId, int endId)
        {
            _lock.EnterReadLock();
            try
            {
                return _items.Values.Where(item => item.Id >= startId && item.Id <= endId).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Sort(IComparer<ICollectionItem> comparer)
        {
            _lock.EnterWriteLock();
            try
            {
                var sortedItems = _items.Values.OrderBy(x => x, comparer).ToList();
                var args = new CollectionEventArgs(CollectionEventType.SORT, items: sortedItems);

                if (!EventsSuspended)
                    CollectionChanging(args);

                _items.Clear();
                foreach (var item in sortedItems)
                {
                    _items[item.Id] = item;
                }

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Replace(int id, ICollectionItem newItem)
        {
            if (newItem == null)
                throw new ArgumentNullException(nameof(newItem));

            _lock.EnterWriteLock();
            try
            {
                if (!_items.TryGetValue(id, out var oldItem))
                    throw new KeyNotFoundException($"No item found with ID: {id}");

                var args = new CollectionEventArgs(
                    CollectionEventType.REPLACE,
                    item: newItem,
                    items: new[] { oldItem, newItem },
                    affectedId: id
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                _items[id] = newItem;

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Update(ICollectionItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _lock.EnterWriteLock();
            try
            {
                if (!_items.ContainsKey(item.Id))
                    throw new KeyNotFoundException($"No item found with ID: {item.Id}");

                var args = new CollectionEventArgs(
                    CollectionEventType.UPDATE,
                    item,
                    affectedId: item.Id
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                _items[item.Id] = item;

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Reset(IEnumerable<ICollectionItem> newItems)
        {
            if (newItems == null)
                throw new ArgumentNullException(nameof(newItems));

            _lock.EnterWriteLock();
            try
            {
                var oldItems = _items.Values.ToList();
                var newItemsList = newItems.ToList();

                var args = new CollectionEventArgs(
                    CollectionEventType.RESET,
                    items: newItemsList,
                    affectedId: null
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                _items.Clear();
                foreach (var item in newItemsList)
                {
                    _items[item.Id] = item;
                }

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RemoveRange(IEnumerable<int> ids)
        {
            _lock.EnterWriteLock();
            try
            {
                var itemsToRemove = ids.Select(id => _items.TryGetValue(id, out var item) ? item : null)
                    .Where(item => item != null)
                    .ToList();

                if (!itemsToRemove.Any())
                    return;

                var args = new CollectionEventArgs(
                    CollectionEventType.BATCH_REMOVE,
                    items: itemsToRemove
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                using (SuspendEvents())
                {
                    foreach (var item in itemsToRemove)
                    {
                        _items.TryRemove(item.Id, out _);
                    }
                }

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void InsertAt(int index, ICollectionItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _lock.EnterWriteLock();
            try
            {
                if (index < 0 || index > Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                var args = new CollectionEventArgs(
                    CollectionEventType.ADD,
                    item: item,
                    affectedId: item.Id,
                    index: index
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                var list = _items.Values.ToList();
                list.Insert(index, item);

                _items.Clear();
                foreach (var existingItem in list)
                {
                    _items[existingItem.Id] = existingItem;
                }

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public override int IndexOf(ICollectionItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _lock.EnterReadLock();
            try
            {
                return _items.Values.ToList().IndexOf(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void Insert(int index, ICollectionItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _lock.EnterWriteLock();
            try
            {
                if (index < 0 || index > Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                var args = new CollectionEventArgs(
                    CollectionEventType.ADD,
                    item: item,
                    affectedId: item.Id,
                    index: index
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                var list = _items.Values.ToList();
                list.Insert(index, item);

                _items.Clear();
                foreach (var existingItem in list)
                {
                    _items[existingItem.Id] = existingItem;
                }

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void RemoveAt(int index)
        {
            _lock.EnterWriteLock();
            try
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                var item = _items.Values.ElementAt(index);
                var args = new CollectionEventArgs(
                    CollectionEventType.REMOVE,
                    item: item,
                    affectedId: item.Id,
                    index: index
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                _items.TryRemove(item.Id, out _);

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
