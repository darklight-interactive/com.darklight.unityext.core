using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.UnityExt.Collection
{
    /// <summary>
    /// Generic implementation of CollectionLibrary that supports key-value pairs.
    /// </summary>
    [Serializable]
    public class CollectionDictionary<TKey, TValue> : Collection,
        IEnumerable<KeyValueCollectionItem<TKey, TValue>>,
        IDictionary<TKey, TValue>
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
        public ICollection<TKey> Keys
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

        public ICollection<TValue> Values
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

        public override object SyncRoot => _lock;

        public TValue this[TKey key]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _concurrentDict[key].Value;
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
                    var id = _concurrentDict.ContainsKey(key) ? _concurrentDict[key].Id : _concurrentDict.Count;
                    var item = new KeyValueCollectionItem<TKey, TValue>(id, key, value);
                    
                    CollectionChanging(new CollectionEventArgs(CollectionEventType.REPLACE, item, null, id));
                    _concurrentDict[key] = item;
                    CollectionChanged(new CollectionEventArgs(CollectionEventType.REPLACE, item, null, id));
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        IEnumerator<KeyValueCollectionItem<TKey, TValue>> IEnumerable<KeyValueCollectionItem<TKey, TValue>>.GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                return _concurrentDict.Values.GetEnumerator();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool ContainsKey(TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                return _concurrentDict.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Remove(TKey key)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_concurrentDict.TryRemove(key, out var item))
                {
                    CollectionChanging(new CollectionEventArgs(CollectionEventType.REMOVE, item, null, item.Id));
                    CollectionChanged(new CollectionEventArgs(CollectionEventType.REMOVE, item, null, item.Id));
                    return true;
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

        public bool TryGetValue(TKey key, out TValue value)
        {
            _lock.EnterReadLock();
            try
            {
                if (_concurrentDict.TryGetValue(key, out var item))
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

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public new void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                CollectionChanging(new CollectionEventArgs(CollectionEventType.RESET));
                _concurrentDict.Clear();
                CollectionChanged(new CollectionEventArgs(CollectionEventType.RESET));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            _lock.EnterReadLock();
            try
            {
                return _concurrentDict.TryGetValue(item.Key, out var existingItem) &&
                       EqualityComparer<TValue>.Default.Equals(existingItem.Value, item.Value);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _lock.EnterReadLock();
            try
            {
                var pairs = _concurrentDict.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value)).ToArray();
                Array.Copy(pairs, 0, array, arrayIndex, pairs.Length);
            }
            finally
            {
                _lock.ExitReadLock();
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

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                return _concurrentDict.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value)).GetEnumerator();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Add(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_concurrentDict.ContainsKey(key))
                {
                    throw new ArgumentException("An item with the same key has already been added.");
                }

                var id = _concurrentDict.Count;
                var item = new KeyValueCollectionItem<TKey, TValue>(id, key, value);
                
                CollectionChanging(new CollectionEventArgs(CollectionEventType.ADD, item, null, id));
                _concurrentDict.TryAdd(key, item);
                CollectionChanged(new CollectionEventArgs(CollectionEventType.ADD, item, null, id));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
