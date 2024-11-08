using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Darklight.UnityExt.Collection
{
    /// <summary>
    /// Generic implementation of CollectionLibrary that supports key-value pairs.
    /// </summary>
    [Serializable]
    public class CollectionDictionary<TKey, TValue> : CollectionLibrary
        where TKey : notnull
        where TValue : notnull
    {
        private readonly ConcurrentDictionary<
            TKey,
            KeyValueCollectionItem<TKey, TValue>
        > _concurrentDict;
        private readonly ReaderWriterLockSlim _lock;
        private bool _isReadOnly;

        [SerializeField]
        private List<KeyValueCollectionItem<TKey, TValue>> _items = new();

        public CollectionDictionary()
        {
            _concurrentDict =
                new ConcurrentDictionary<TKey, KeyValueCollectionItem<TKey, TValue>>();
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _isReadOnly = false;

            OnCollectionChanged += (sender, args) =>
            {
                Debug.Log($"Collection changed: {args.EventType}");
                _items = _concurrentDict.Values.ToList();
            };
        }

        public override int Capacity => _concurrentDict.Count;
        public override int Count => _concurrentDict.Count;

        public override IEnumerable<int> IDs => _concurrentDict.Values.Select(item => item.Id);
        public override bool IsReadOnly => _isReadOnly;
        public override bool IsSynchronized => true;

        public override IEnumerable<CollectionItem> Items =>
            _concurrentDict.Values.Select(item => (CollectionItem)item);
        public override object SyncRoot => _lock;
        public override IEnumerable<object> ObjectValues =>
            _concurrentDict.Values.Select(item => item.Value);

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
                if (value is not KeyValueCollectionItem<TKey, TValue> kvItem)
                    throw new ArgumentException("Invalid item type");

                _lock.EnterWriteLock();
                try
                {
                    var oldItem = _concurrentDict.Values.ElementAt(index);
                    _concurrentDict.TryRemove(oldItem.Key, out _);
                    _concurrentDict.TryAdd(kvItem.Key, kvItem);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                var item = new KeyValueCollectionItem<TKey, TValue>(
                    _concurrentDict.Count,
                    key,
                    value
                );
                Add(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Add(CollectionItem item)
        {
            if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                throw new ArgumentException("Invalid item type");

            _lock.EnterWriteLock();
            try
            {
                if (!_concurrentDict.TryAdd(kvItem.Key, kvItem))
                    throw new ArgumentException($"An item with key {kvItem.Key} already exists");

                var args = new CollectionEventArgs(CollectionEventType.ADD, item);
                CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds a new item or updates an existing item with the specified key and value.
        /// </summary>
        /// <param name="key">The key of the item to add or update.</param>
        /// <param name="value">The value to set.</param>
        /// <remarks>
        /// This operation is thread-safe. If an item with the specified key already exists,
        /// it will be updated with the new value. Otherwise, a new item will be added.
        /// </remarks>
        public void AddOrUpdate(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_concurrentDict.TryGetValue(key, out var existingItem))
                {
                    var updatedItem = new KeyValueCollectionItem<TKey, TValue>(
                        existingItem.Id,
                        key,
                        value
                    );
                    _concurrentDict[key] = updatedItem;
                    var args = new CollectionEventArgs(CollectionEventType.UPDATE, updatedItem);
                    CollectionChanged(args);
                }
                else
                {
                    var newItem = new KeyValueCollectionItem<TKey, TValue>(
                        _concurrentDict.Count,
                        key,
                        value
                    );
                    _concurrentDict[key] = newItem;
                    var args = new CollectionEventArgs(CollectionEventType.ADD, newItem);
                    CollectionChanged(args);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void AddRange(IEnumerable<CollectionItem> items)
        {
            _lock.EnterWriteLock();
            try
            {
                var args = new CollectionEventArgs(
                    CollectionEventType.BATCH_ADD,
                    items: items.ToList()
                );
                CollectionChanging(args);

                foreach (var item in items)
                {
                    if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                        throw new ArgumentException(
                            $"Item must be of type KeyValueCollectionItem<{typeof(TKey).Name}, {typeof(TValue).Name}>"
                        );

                    if (!_concurrentDict.TryAdd(kvItem.Key, kvItem))
                        throw new ArgumentException(
                            $"An item with key {kvItem.Key} already exists"
                        );
                }

                CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override bool Remove(CollectionItem item)
        {
            if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                return false;

            _lock.EnterWriteLock();
            try
            {
                if (_concurrentDict.TryRemove(kvItem.Key, out _))
                {
                    var args = new CollectionEventArgs(CollectionEventType.REMOVE, item);
                    CollectionChanged(args);
                    return true;
                }
                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override bool Contains(CollectionItem item)
        {
            if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                return false;

            _lock.EnterReadLock();
            try
            {
                return _concurrentDict.ContainsKey(kvItem.Key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void CopyTo(CollectionItem[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            _lock.EnterReadLock();
            try
            {
                if (array.Length - arrayIndex < Count)
                    throw new ArgumentException("Destination array is not large enough");

                foreach (var item in _concurrentDict.Values)
                {
                    array[arrayIndex++] = item;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                var items = _concurrentDict.Values.ToList();
                _concurrentDict.Clear();
                var args = new CollectionEventArgs(CollectionEventType.CLEAR, items: items);
                CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            _lock.EnterReadLock();
            try
            {
                if (_concurrentDict.TryGetValue(key, out var item))
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

        public TValue GetValue(TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                if (_concurrentDict.TryGetValue(key, out var item))
                    return (TValue)item.Value;
                throw new KeyNotFoundException($"Key {key} not found in collection");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void Dispose()
        {
            _lock.EnterWriteLock();
            try
            {
                _concurrentDict.Clear();
                var args = new CollectionEventArgs(CollectionEventType.DISPOSE);
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
            if (other is not CollectionDictionary<TKey, TValue> otherDict)
                return false;

            return _concurrentDict.Values.SequenceEqual(otherDict._concurrentDict.Values);
        }

        public override IEnumerator<CollectionItem> GetEnumerator()
        {
            return _concurrentDict.Values.GetEnumerator();
        }

        public override int IndexOf(CollectionItem item)
        {
            if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                return -1;

            return _concurrentDict.Values.ToList().IndexOf(kvItem);
        }

        public override void Insert(int index, CollectionItem item)
        {
            if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                throw new ArgumentException(
                    "Item must be of type KeyValueCollectionItem<TKey, TValue>"
                );

            _lock.EnterWriteLock();
            try
            {
                var itemsList = _concurrentDict.Values.ToList();
                itemsList.Insert(index, kvItem);
                _concurrentDict.Clear();
                foreach (var existingItem in itemsList)
                {
                    _concurrentDict.TryAdd(existingItem.Key, existingItem);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void RemoveAt(int index)
        {
            var item = _concurrentDict.Values.ElementAt(index);
            _lock.EnterWriteLock();
            try
            {
                _concurrentDict.TryRemove(item.Key, out _);
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
                var itemsList = items.ToList(); // Create snapshot
                foreach (var item in itemsList)
                {
                    if (item is KeyValueCollectionItem<TKey, TValue> kvItem)
                    {
                        _concurrentDict.TryRemove(kvItem.Key, out _);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void RemoveWhere(Func<CollectionItem, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            _lock.EnterWriteLock();
            try
            {
                var itemsToRemove = _concurrentDict.Values.Where(predicate).ToList();
                foreach (var item in itemsToRemove)
                {
                    if (item is KeyValueCollectionItem<TKey, TValue> kvItem)
                    {
                        _concurrentDict.TryRemove(kvItem.Key, out _);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Replace(CollectionItem oldItem, CollectionItem newItem)
        {
            if (oldItem == null)
                throw new ArgumentNullException(nameof(oldItem));
            if (newItem == null)
                throw new ArgumentNullException(nameof(newItem));

            if (oldItem is not KeyValueCollectionItem<TKey, TValue> kvOldItem)
                return;
            if (newItem is not KeyValueCollectionItem<TKey, TValue> kvNewItem)
                throw new ArgumentException("Invalid new item type");

            _lock.EnterWriteLock();
            try
            {
                if (_concurrentDict.TryRemove(kvOldItem.Key, out _))
                {
                    _concurrentDict.TryAdd(kvNewItem.Key, kvNewItem);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override bool TryGetItem(int id, out CollectionItem item)
        {
            _lock.EnterReadLock();
            try
            {
                item = _concurrentDict.Values.FirstOrDefault(i => i.Id == id);
                return item != null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            _lock.EnterReadLock();
            try
            {
                Array.Copy(
                    _concurrentDict.Values.ToArray(),
                    0,
                    array,
                    index,
                    _concurrentDict.Count
                );
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override bool AddOrUpdate(CollectionItem item)
        {
            if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                throw new ArgumentException("Invalid item type");

            _lock.EnterWriteLock();
            try
            {
                return _concurrentDict.AddOrUpdate(kvItem.Key, kvItem, (_, _) => kvItem) != null;
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
                Add(
                    new KeyValueCollectionItem<TKey, TValue>(
                        _concurrentDict.Count,
                        default,
                        default
                    )
                );
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
    }
}
