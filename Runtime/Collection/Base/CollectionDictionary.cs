using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<TKey, KeyValueCollectionItem<TKey, TValue>> _items;
        private readonly ReaderWriterLockSlim _lock;
        private bool _isReadOnly;

        public CollectionDictionary()
        {
            _items = new ConcurrentDictionary<TKey, KeyValueCollectionItem<TKey, TValue>>();
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _isReadOnly = false;
        }

        public override int Capacity => _items.Count;
        public override int Count => _items.Count;

        public override IEnumerable<int> IDs => _items.Values.Select(item => item.Id);
        public override bool IsReadOnly => _isReadOnly;
        public override bool IsSynchronized => true;

        public override IEnumerable<ICollectionItem> Items => _items.Values;
        public override object SyncRoot => _lock;
        public override IEnumerable<object> Values => _items.Values.Select(item => item.Value);

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
                if (value is not KeyValueCollectionItem<TKey, TValue> kvItem)
                    throw new ArgumentException("Invalid item type");

                _lock.EnterWriteLock();
                try
                {
                    var oldItem = _items.Values.ElementAt(index);
                    _items.TryRemove(oldItem.Key, out _);
                    _items.TryAdd(kvItem.Key, kvItem);
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
                var item = new KeyValueCollectionItem<TKey, TValue>(_items.Count, key, value);
                Add(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Add(ICollectionItem item)
        {
            if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                throw new ArgumentException("Invalid item type");

            _lock.EnterWriteLock();
            try
            {
                if (!_items.TryAdd(kvItem.Key, kvItem))
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
                if (_items.TryGetValue(key, out var existingItem))
                {
                    var updatedItem = new KeyValueCollectionItem<TKey, TValue>(existingItem.Id, key, value);
                    _items[key] = updatedItem;
                    var args = new CollectionEventArgs(CollectionEventType.UPDATE, updatedItem);
                    CollectionChanged(args);
                }
                else
                {
                    var newItem = new KeyValueCollectionItem<TKey, TValue>(_items.Count, key, value);
                    _items[key] = newItem;
                    var args = new CollectionEventArgs(CollectionEventType.ADD, newItem);
                    CollectionChanged(args);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void AddRange(IEnumerable<ICollectionItem> items)
        {
            _lock.EnterWriteLock();
            try
            {
                var args = new CollectionEventArgs(CollectionEventType.BATCH_ADD, items: items.ToList());
                CollectionChanging(args);

                foreach (var item in items)
                {
                    if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                        throw new ArgumentException($"Item must be of type KeyValueCollectionItem<{typeof(TKey).Name}, {typeof(TValue).Name}>");

                    if (!_items.TryAdd(kvItem.Key, kvItem))
                        throw new ArgumentException($"An item with key {kvItem.Key} already exists");
                }

                CollectionChanged(args);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override bool Remove(ICollectionItem item)
        {
            if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                return false;

            _lock.EnterWriteLock();
            try
            {
                if (_items.TryRemove(kvItem.Key, out _))
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
        public override bool Contains(ICollectionItem item)
        {
            if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                return false;

            _lock.EnterReadLock();
            try
            {
                return _items.ContainsKey(kvItem.Key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void CopyTo(ICollectionItem[] array, int arrayIndex)
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

                foreach (var item in _items.Values)
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
                var items = _items.Values.ToList();
                _items.Clear();
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
                if (_items.TryGetValue(key, out var item))
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
                if (_items.TryGetValue(key, out var item))
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
                _items.Clear();
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

            return _items.Values.SequenceEqual(otherDict._items.Values);
        }

        public override IEnumerator<ICollectionItem> GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }

        public override int IndexOf(ICollectionItem item)
        {
            if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                return -1;

            return _items.Values.ToList().IndexOf(kvItem);
        }

        public override void Insert(int index, ICollectionItem item)
        {
            if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                throw new ArgumentException(
                    "Item must be of type KeyValueCollectionItem<TKey, TValue>"
                );

            _lock.EnterWriteLock();
            try
            {
                var itemsList = _items.Values.ToList();
                itemsList.Insert(index, kvItem);
                _items.Clear();
                foreach (var existingItem in itemsList)
                {
                    _items.TryAdd(existingItem.Key, existingItem);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void RemoveAt(int index)
        {
            var item = _items.Values.ElementAt(index);
            _lock.EnterWriteLock();
            try
            {
                _items.TryRemove(item.Key, out _);
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
                var itemsList = items.ToList(); // Create snapshot
                foreach (var item in itemsList)
                {
                    if (item is KeyValueCollectionItem<TKey, TValue> kvItem)
                    {
                        _items.TryRemove(kvItem.Key, out _);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public override void RemoveWhere(Func<ICollectionItem, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            _lock.EnterWriteLock();
            try
            {
                var itemsToRemove = _items.Values.Where(predicate).ToList();
                foreach (var item in itemsToRemove)
                {
                    if (item is KeyValueCollectionItem<TKey, TValue> kvItem)
                    {
                        _items.TryRemove(kvItem.Key, out _);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Replace(ICollectionItem oldItem, ICollectionItem newItem)
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
                if (_items.TryRemove(kvOldItem.Key, out _))
                {
                    _items.TryAdd(kvNewItem.Key, kvNewItem);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override bool TryGetItem(int id, out ICollectionItem item)
        {
            _lock.EnterReadLock();
            try
            {
                item = _items.Values.FirstOrDefault(i => i.Id == id);
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
                Array.Copy(_items.Values.ToArray(), 0, array, index, _items.Count);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override bool AddOrUpdate(ICollectionItem item)
        {
            if (item is not KeyValueCollectionItem<TKey, TValue> kvItem)
                throw new ArgumentException("Invalid item type");

            _lock.EnterWriteLock();
            try
            {
                return _items.AddOrUpdate(kvItem.Key, kvItem, (_, _) => kvItem) != null;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

    }
}
