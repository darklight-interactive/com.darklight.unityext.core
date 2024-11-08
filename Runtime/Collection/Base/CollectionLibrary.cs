using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

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
        public CollectionItem Item { get; }
        public IEnumerable<CollectionItem> Items { get; }
        public int? AffectedId { get; }
        public int? Index { get; }

        public CollectionEventArgs(
            CollectionEventType eventType,
            CollectionItem item = null,
            IEnumerable<CollectionItem> items = null,
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

    [Serializable]
    public class CollectionLibrary<TValue> : CollectionLibrary
    {
        private readonly ConcurrentDictionary<int, CollectionItem<TValue>> _concurrentDict;
        private readonly ReaderWriterLockSlim _lock;
        private bool _isInitialized;

        [SerializeField] protected List<CollectionItem<TValue>> _items = new();
        [SerializeField] protected List<int> _ids = new();
        [SerializeField] protected CollectionGuiSettings _guiSettings = new();

        public CollectionLibrary()
        {
            _concurrentDict = new ConcurrentDictionary<int, CollectionItem<TValue>>();
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _isInitialized = true;
            CollectionChanged(new CollectionEventArgs(CollectionEventType.INITIALIZE));       

            OnCollectionChanged += (sender, args) =>
            {
                Debug.Log($"Collection changed: {args.EventType}");
                _items = _concurrentDict.Values.ToList();
            };
        }

        public override IEnumerable<CollectionItem> Items => _concurrentDict.Values.Cast<CollectionItem>();
        public override int Count => _concurrentDict.Count;
        public override int Capacity => _concurrentDict.Count;
        public override bool IsReadOnly => false;
        public override object SyncRoot => _lock;
        public override bool IsSynchronized => true;

        public override CollectionItem this[int index]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _concurrentDict.Values.ElementAt(index);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                if (!(value is CollectionItem<TValue> typedValue))
                    throw new ArgumentException($"Value must be of type CollectionItem<{typeof(TValue).Name}>");

                _lock.EnterWriteLock();
                try
                {
                    var key = _concurrentDict.Keys.ElementAt(index);
                    _concurrentDict[key] = typedValue;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public override void Add(CollectionItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
        
            if (!(item is CollectionItem<TValue> typedItem))
                throw new ArgumentException($"Item must be of type CollectionItem<{typeof(TValue).Name}>");

            _lock.EnterWriteLock();
            try
            {
                var args = new CollectionEventArgs(
                    CollectionEventType.ADD,
                    item,
                    affectedId: item.Id
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                if (!_concurrentDict.TryAdd(item.Id, typedItem))
                    throw new ArgumentException($"An item with ID {item.Id} already exists");

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void AddDefaultItem()
        {
            _lock.EnterWriteLock();
            try
            {
                int id = _concurrentDict.Count;
                Add(new CollectionItem<TValue>(id, Activator.CreateInstance<TValue>()));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override bool Remove(CollectionItem item)
        {
            if (item == null)
                return false;

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

                bool result = _concurrentDict.TryRemove(item.Id, out _);

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
                var items = _concurrentDict.Values.ToList();
                var args = new CollectionEventArgs(CollectionEventType.CLEAR, items: items);

                if (!EventsSuspended)
                    CollectionChanging(args);

                _concurrentDict.Clear();

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override bool Contains(CollectionItem item) =>
            item != null && _concurrentDict.ContainsKey(item.Id);

        public override void CopyTo(CollectionItem[] array, int arrayIndex)
        {
            _lock.EnterReadLock();
            try
            {
                var values = _concurrentDict.Values.Cast<CollectionItem>().ToArray();
                Array.Copy(values, 0, array, arrayIndex, values.Length);
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
                ((ICollection)_concurrentDict.Values).CopyTo(array, index);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override IEnumerator<CollectionItem> GetEnumerator() =>
            _concurrentDict.Values.Cast<CollectionItem>().GetEnumerator();

        public override void Dispose()
        {
            _lock.EnterWriteLock();
            try
            {
                var args = new CollectionEventArgs(CollectionEventType.DISPOSE);

                if (!EventsSuspended)
                    CollectionChanging(args);

                _concurrentDict.Clear();

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
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!(other is CollectionLibrary<TValue> typedOther)) return false;

            return _concurrentDict.Values.SequenceEqual(typedOther._concurrentDict.Values);
        }

        public override IEnumerable<int> IDs
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _concurrentDict.Keys.ToList();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public override IEnumerable<object> ObjectValues
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _concurrentDict.Values.Select(x => x.Value).ToList();
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
                if (_concurrentDict.TryGetValue(id, out var item))
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
        public override bool TryGetItem(int id, out CollectionItem item)
        {
            _lock.EnterReadLock();
            try
            {
                if (_concurrentDict.TryGetValue(id, out var tempItem))
                {
                    item = tempItem;
                    return true;
                }
                item = null;
                return false;
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
                if (_concurrentDict.TryGetValue(id, out var item))
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
        public override bool AddOrUpdate(CollectionItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _lock.EnterWriteLock();
            try
            {
                bool isNew = !_concurrentDict.ContainsKey(item.Id);
                var eventType = isNew ? CollectionEventType.ADD : CollectionEventType.UPDATE;
                var args = new CollectionEventArgs(eventType, item, affectedId: item.Id);

                if (!EventsSuspended)
                    CollectionChanging(args);

                _concurrentDict[item.Id] = (CollectionItem<TValue>)item;

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
                return _concurrentDict.TryGetValue(id, out var item)
                    ? (TValue)item.Value
                    : defaultValue;
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
        public CollectionLibrary<TValue> Where(Func<CollectionItem<TValue>, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                var newCollection = new CollectionLibrary<TValue>();
                foreach (var item in _concurrentDict.Values.Where(predicate))
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
        public bool Any(Func<CollectionItem<TValue>, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _concurrentDict.Values.Any(predicate);
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
        public CollectionItem<TValue> FirstOrDefault(Func<CollectionItem<TValue>, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _concurrentDict.Values.FirstOrDefault(predicate);
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
        public override void AddRange(IEnumerable<CollectionItem> items)
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

        public override void RemoveRange(IEnumerable<CollectionItem> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _lock.EnterWriteLock();
            try
            {
                var itemsList = items.ToList();
                var args = new CollectionEventArgs(
                    CollectionEventType.BATCH_REMOVE,
                    items: itemsList
                );

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

        public override void Replace(CollectionItem item, CollectionItem newItem)
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

                _concurrentDict[item.Id] = (CollectionItem<TValue>)newItem;

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

        public override void RemoveWhere(Func<CollectionItem, bool> predicate)
        {
            _lock.EnterWriteLock();
            try
            {
                var itemsToRemove = _concurrentDict.Values.Where(predicate).ToList();
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
                        _concurrentDict.TryRemove(item.Id, out _);
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
                return _concurrentDict.ContainsKey(id);
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
        public IEnumerable<CollectionItem<TValue>> GetItemsInRange(int startId, int endId)
        {
            _lock.EnterReadLock();
            try
            {
                return _concurrentDict
                    .Values.Where(item => item.Id >= startId && item.Id <= endId)
                    .ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Sort(IComparer<CollectionItem<TValue>> comparer)
        {
            _lock.EnterWriteLock();
            try
            {
                var sortedItems = _concurrentDict.Values.OrderBy(x => x, comparer).ToList();
                var args = new CollectionEventArgs(CollectionEventType.SORT, items: sortedItems);

                if (!EventsSuspended)
                    CollectionChanging(args);

                _concurrentDict.Clear();
                foreach (var item in sortedItems)
                {
                    _concurrentDict[item.Id] = item;
                }

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Replace(int id, CollectionItem<TValue> newItem)
        {
            if (newItem == null)
                throw new ArgumentNullException(nameof(newItem));

            _lock.EnterWriteLock();
            try
            {
                if (!_concurrentDict.TryGetValue(id, out var oldItem))
                    throw new KeyNotFoundException($"No item found with ID: {id}");

                var args = new CollectionEventArgs(
                    CollectionEventType.REPLACE,
                    item: newItem,
                    items: new[] { oldItem, newItem },
                    affectedId: id
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                _concurrentDict[id] = newItem;

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Update(CollectionItem<TValue> item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _lock.EnterWriteLock();
            try
            {
                if (!_concurrentDict.ContainsKey(item.Id))
                    throw new KeyNotFoundException($"No item found with ID: {item.Id}");

                var args = new CollectionEventArgs(
                    CollectionEventType.UPDATE,
                    item,
                    affectedId: item.Id
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                _concurrentDict[item.Id] = item;

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Reset(IEnumerable<CollectionItem<TValue>> newItems)
        {
            if (newItems == null)
                throw new ArgumentNullException(nameof(newItems));

            _lock.EnterWriteLock();
            try
            {
                var oldItems = _concurrentDict.Values.ToList();
                var newItemsList = newItems.ToList();

                var args = new CollectionEventArgs(
                    CollectionEventType.RESET,
                    items: newItemsList,
                    affectedId: null
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                _concurrentDict.Clear();
                foreach (var item in newItemsList)
                {
                    _concurrentDict[item.Id] = item;
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
                var itemsToRemove = ids.Select(id =>
                        _concurrentDict.TryGetValue(id, out var item) ? item : null
                    )
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
                        _concurrentDict.TryRemove(item.Id, out _);
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

        public void InsertAt(int index, CollectionItem<TValue> item)
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

                var list = _concurrentDict.Values.ToList();
                list.Insert(index, item);

                _concurrentDict.Clear();
                foreach (var existingItem in list)
                {
                    _concurrentDict[existingItem.Id] = existingItem;
                }

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override int IndexOf(CollectionItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _lock.EnterReadLock();
            try
            {
                return _concurrentDict.Values.ToList().IndexOf((CollectionItem<TValue>)item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void Insert(int index, CollectionItem item)
        {
            if (!(item is CollectionItem<TValue> typedItem))
                throw new ArgumentException($"Item must be of type CollectionItem<{typeof(TValue).Name}>");

            InsertAt(index, typedItem);
        }

        public override void RemoveAt(int index)
        {
            _lock.EnterWriteLock();
            try
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                var item = _concurrentDict.Values.ElementAt(index);
                var args = new CollectionEventArgs(
                    CollectionEventType.REMOVE,
                    item: item,
                    affectedId: item.Id,
                    index: index
                );

                if (!EventsSuspended)
                    CollectionChanging(args);

                _concurrentDict.TryRemove(item.Id, out _);

                if (!EventsSuspended)
                    CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Refresh()
        {
            _lock.EnterWriteLock();
            try
            {
                _items = _concurrentDict.Values.ToList();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public CollectionItem<TValue> GetTypedItem(int id)
        {
            _lock.EnterReadLock();
            try
            {
                if (_concurrentDict.TryGetValue(id, out var item))
                    return item;
                throw new KeyNotFoundException($"No item found with ID: {id}");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public CollectionItem<TValue> GetTypedItemAt(int index)
        {
            _lock.EnterReadLock();
            try
            {
                return _concurrentDict.Values.ElementAt(index);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void SetTypedItem(int index, CollectionItem<TValue> item)
        {
            _lock.EnterWriteLock();
            try
            {
                var key = _concurrentDict.Keys.ElementAt(index);
                _concurrentDict[key] = item;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    [Serializable]
    public class CollectionGuiSettings
    {
        public bool showHeader = true;
        public bool showFooter = true;
        public bool showSearch = true;
        public bool showPagination = true;
        public int itemsPerPage = 10;
        public string searchText = string.Empty;
        public int currentPage = 0;
        public bool readOnlyKey = false;
        public bool readOnlyValue = false;
    }
}
