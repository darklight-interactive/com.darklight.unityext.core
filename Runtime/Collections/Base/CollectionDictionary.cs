using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Collections
{
    /// <summary>
    /// Generic implementation of CollectionLibrary that supports key-value pairs.
    /// </summary>
    [Serializable]
    public class CollectionDictionary<TKey, TValue>
        : Collection,
            IEnumerable<KeyValueCollectionItem<TKey, TValue>>,
            IDictionary<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        #region ---- < FIELDS > ================================================================
        /// <summary>
        /// The concurrent dictionary that stores the items.
        /// </summary>
        private readonly ConcurrentDictionary<
            int,
            KeyValueCollectionItem<TKey, TValue>
        > _concurrentDict;

        /// <summary>
        /// The lock for the concurrent dictionary.
        /// </summary>
        private readonly ReaderWriterLockSlim _lock;

        /// <summary>
        /// Whether the collection is read-only.
        /// </summary>
        private bool _isReadOnly;

        [SerializeField]
        [Tooltip("The list of key-value items in the collection.")]
        private List<KeyValueCollectionItem<TKey, TValue>> _dictionaryItems = new();

        #endregion

        #region ---- < PROPERTIES > ================================================================

        public override int Capacity => _concurrentDict.Count;
        public override int Count => _dictionaryItems.Count;
        public override IEnumerable<int> IDs => _dictionaryItems.Select(x => x.Id);
        public override bool IsReadOnly => _isReadOnly;
        public override bool IsSynchronized => true;
        public override object SyncRoot => _lock;
        public override IEnumerable<CollectionItem> Items =>
            _concurrentDict.Values.Select(item => (CollectionItem)item);
        public IEnumerable<TKey> Keys => _dictionaryItems.Select(x => x.Key);
        public IEnumerable<TValue> Values => _dictionaryItems.Select(x => x.Value);

        public TValue this[TKey key]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _dictionaryItems.FirstOrDefault(x => x.Key.Equals(key)).Value;
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
                    var id =
                        _dictionaryItems.FirstOrDefault(x => x.Key.Equals(key))?.Id
                        ?? _dictionaryItems.Count;
                    var item = new KeyValueCollectionItem<TKey, TValue>(id, key, value);

                    CollectionChanging(
                        new CollectionEventArgs(CollectionEventType.REPLACE, item, null, id)
                    );
                    _concurrentDict[id] = item;
                    CollectionChanged(
                        new CollectionEventArgs(CollectionEventType.REPLACE, item, null, id)
                    );
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        #endregion

        #region < CONSTRUCTOR > ================================================================
        public CollectionDictionary()
        {
            _concurrentDict = new ConcurrentDictionary<int, KeyValueCollectionItem<TKey, TValue>>();
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _isReadOnly = false;

            OnCollectionChanged += (sender, args) =>
            {
                //Debug.Log($"CollectionDictionary changed: {args.EventType}");
                _dictionaryItems = _concurrentDict.Values.ToList();
            };
        }

        public CollectionDictionary(IEnumerable<TKey> keys)
            : this()
        {
            foreach (var key in keys)
            {
                Add(key, default);
            }
        }

        public CollectionDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items)
            : this()
        {
            foreach (var item in items)
            {
                Add(item.Key, item.Value);
            }
        }

        #endregion

        #region ---- < PUBLIC_METHODS > [Collection Management] ================================================================

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                if (ContainsKey(key))
                {
                    throw new ArgumentException(
                        "An item with the same key has already been added."
                    );
                }

                var id = CollectionUtils.GetNextId(IDs);
                var item = new KeyValueCollectionItem<TKey, TValue>(id, key, value);

                CollectionChanging(
                    new CollectionEventArgs(CollectionEventType.ADD, item, null, id)
                );
                bool result = _concurrentDict.TryAdd(id, item);
                if (result)
                {
                    _dictionaryItems.Add(item);
                }

                CollectionChanged(new CollectionEventArgs(CollectionEventType.ADD, item, null, id));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void AddDefaultItem()
        {
            Add(GetNextKey(), default);
        }

        public new void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                CollectionChanging(new CollectionEventArgs(CollectionEventType.RESET));
                _concurrentDict.Clear();
                _dictionaryItems.Clear();
                CollectionChanged(new CollectionEventArgs(CollectionEventType.RESET));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        #endregion

        #region < PUBLIC_METHODS > [[ Getters ]] ================================================================

        /// <summary>
        /// Checks if the collection contains the specified item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the collection contains the item, false otherwise.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionaryItems.Any(x =>
                    x.Key.Equals(item.Key)
                    && EqualityComparer<TValue>.Default.Equals(x.Value, item.Value)
                );
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Get the next key in the sequence.
        /// </summary>
        /// <returns>The next key in the sequence.</returns>
        public TKey GetNextKey()
        {
            _lock.EnterReadLock();
            try
            {
                if (typeof(TKey).IsEnum)
                {
                    var values = Enum.GetValues(typeof(TKey)).Cast<TKey>();
                    foreach (var value in values)
                    {
                        if (!ContainsKey(value))
                            return value;
                    }
                }
                else if (typeof(TKey) == typeof(int))
                {
                    if (!_dictionaryItems.Any())
                        return (TKey)(object)0;
                    var maxKey = _dictionaryItems.Max(x => Convert.ToInt32(x.Key));
                    return (TKey)(object)(maxKey + 1);
                }
                return default;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Tries to get the key for the specified value.
        /// </summary>
        /// <param name="value">The value to get the key for.</param>
        /// <param name="key">The key for the specified value.</param>
        /// <returns>True if the key was found, false otherwise.</returns>
        public bool TryGetKey(TValue value, out TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                foreach (var item in _dictionaryItems)
                {
                    if (EqualityComparer<TValue>.Default.Equals(item.Value, value))
                    {
                        key = item.Key;
                        return true;
                    }
                }
                key = default;
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Tries to get the value for the specified key.
        /// </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <param name="value">The value for the specified key.</param>
        /// <returns>True if the value was found, false otherwise.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            _lock.EnterReadLock();
            try
            {
                var item = _dictionaryItems.FirstOrDefault(x => x.Key.Equals(key));
                if (item != null)
                {
                    value = item.Value;
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
        /// Checks if the collection contains the specified key.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns>True if the collection contains the key, false otherwise.</returns>
        public bool ContainsKey(TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                foreach (var item in _dictionaryItems)
                {
                    if (item.Key.Equals(key))
                        return true;
                }
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Copies the collection to an array.
        /// </summary>
        /// <param name="array">The array to copy the collection to.</param>
        /// <param name="arrayIndex">The index to start copying from.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _lock.EnterReadLock();
            try
            {
                var pairs = _dictionaryItems
                    .Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value))
                    .ToArray();
                Array.Copy(pairs, 0, array, arrayIndex, pairs.Length);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        #endregion


        public override void Refresh()
        {
            _lock.EnterWriteLock();
            try
            {
                _dictionaryItems = _concurrentDict.Values.ToList();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove(TKey key)
        {
            _lock.EnterWriteLock();
            try
            {
                var item = _dictionaryItems.FirstOrDefault(x => x.Key.Equals(key));
                if (item != null)
                {
                    CollectionChanging(
                        new CollectionEventArgs(CollectionEventType.REMOVE, item, null, item.Id)
                    );
                    CollectionChanged(
                        new CollectionEventArgs(CollectionEventType.REMOVE, item, null, item.Id)
                    );
                    return true;
                }
                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            _lock.EnterWriteLock();
            try
            {
                if (Contains(item))
                {
                    return Remove(item.Key);
                }
                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Replace(TKey key, TValue value)
        {
            this[key] = value;
        }

        #region < TYPE_CONVERSIONS > [[ Type Conversions ]] ================================================================
        IEnumerator<KeyValueCollectionItem<TKey, TValue>> IEnumerable<
            KeyValueCollectionItem<TKey, TValue>
        >.GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionaryItems.GetEnumerator();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<
            KeyValuePair<TKey, TValue>
        >.GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionaryItems
                    .Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value))
                    .GetEnumerator();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys =>
            _dictionaryItems.Select(x => x.Key).ToList();
        ICollection<TValue> IDictionary<TKey, TValue>.Values =>
            _dictionaryItems.Select(x => x.Value).ToList();

        #endregion
    }
}
