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

        public int? AffectedId { get; }
        public CollectionEventType EventType { get; }
        public int? Index { get; }
        public CollectionItem Item { get; }
        public IEnumerable<CollectionItem> Items { get; }
    }

    [Serializable]
    public class CollectionGuiSettings
    {
        public int currentPage = 0;
        public int itemsPerPage = 10;
        public bool readOnlyKey = false;
        public bool readOnlyValue = false;
        public string searchText = string.Empty;
        public bool showFooter = true;
        public bool showHeader = true;
        public bool showPagination = true;
        public bool showSearch = true;
    }

    [Serializable]
    public class CollectionLibrary<TValue> : Collection, IList<TValue>, IEnumerable<TValue>
        where TValue : notnull
    {
        [SerializeField]
        protected CollectionGuiSettings _guiSettings = new();

        [SerializeField]
        protected List<int> _ids = new();

        [SerializeField]
        protected List<CollectionItem<TValue>> _libraryItems = new();
        private readonly ConcurrentDictionary<int, CollectionItem<TValue>> _concurrentDict;
        private readonly ReaderWriterLockSlim _lock;
        private bool _isInitialized;

        public CollectionLibrary()
        {
            _concurrentDict = new ConcurrentDictionary<int, CollectionItem<TValue>>();
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _isInitialized = true;
            CollectionChanged(new CollectionEventArgs(CollectionEventType.INITIALIZE));

            OnCollectionChanged += (sender, args) =>
            {
                Debug.Log($"Collection changed: {args.EventType}");
                Refresh();
            };
        }

        public override int Capacity => _concurrentDict.Count;
        public override int Count => _concurrentDict.Count;

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
        public override bool IsReadOnly => false;
        public override bool IsSynchronized => true;

        public new IEnumerable<CollectionItem<TValue>> Items => _libraryItems;

        public override IEnumerable<object> Objects
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _libraryItems.Select(x => x.Object).ToList();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }
        public override object SyncRoot => _lock;

        public IEnumerable<TValue> Values => _libraryItems.Select(x => x.Value);

        public TValue this[int index]
        {
            get => _libraryItems[index].Value;
            set =>
                _libraryItems[index] = new CollectionItem<TValue>(_libraryItems[index].Id, value);
        }

        public void Add(TValue item)
        {
            _lock.EnterWriteLock();
            try
            {
                var id = _ids.Count > 0 ? _ids.Max() + 1 : 0;
                var collectionItem = new CollectionItem<TValue>(id, item);

                CollectionChanging(
                    new CollectionEventArgs(CollectionEventType.ADD, collectionItem, null, id)
                );

                _ids.Add(id);
                _concurrentDict.TryAdd(id, collectionItem);

                CollectionChanged(
                    new CollectionEventArgs(CollectionEventType.ADD, collectionItem, null, id)
                );
                Refresh();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void AddDefaultItem()
        {
            Add(default);
        }

        public new void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                CollectionChanging(new CollectionEventArgs(CollectionEventType.RESET));
                _ids.Clear();
                _concurrentDict.Clear();
                CollectionChanged(new CollectionEventArgs(CollectionEventType.RESET));
                Refresh();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Contains(TValue item)
        {
            _lock.EnterReadLock();
            try
            {
                return _concurrentDict.Values.Any(x => x.Value.Equals(item));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            _lock.EnterReadLock();
            try
            {
                var values = _concurrentDict.Values.Select(x => x.Value).ToArray();
                Array.Copy(values, 0, array, arrayIndex, values.Length);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public int IndexOf(TValue item)
        {
            _lock.EnterReadLock();
            try
            {
                return _libraryItems.FindIndex(x => x.Value.Equals(item));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Insert(int index, TValue item)
        {
            _lock.EnterWriteLock();
            try
            {
                var id = _ids.Count > 0 ? _ids.Max() + 1 : 0;
                var collectionItem = new CollectionItem<TValue>(id, item);

                CollectionChanging(
                    new CollectionEventArgs(
                        CollectionEventType.ADD,
                        collectionItem,
                        null,
                        id,
                        index
                    )
                );

                _ids.Insert(index, id);
                _concurrentDict.TryAdd(id, collectionItem);

                CollectionChanged(
                    new CollectionEventArgs(
                        CollectionEventType.ADD,
                        collectionItem,
                        null,
                        id,
                        index
                    )
                );
                Refresh();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Refresh()
        {
            _libraryItems = _concurrentDict.Values.ToList();
        }

        public bool Remove(TValue item)
        {
            _lock.EnterWriteLock();
            try
            {
                var entry = _concurrentDict.FirstOrDefault(x => x.Value.Value.Equals(item));
                if (entry.Value != null)
                {
                    var index = _ids.IndexOf(entry.Key);
                    CollectionChanging(
                        new CollectionEventArgs(
                            CollectionEventType.REMOVE,
                            entry.Value,
                            null,
                            entry.Key,
                            index
                        )
                    );
                    _ids.Remove(entry.Key);
                    _concurrentDict.TryRemove(entry.Key, out _);
                    CollectionChanged(
                        new CollectionEventArgs(
                            CollectionEventType.REMOVE,
                            entry.Value,
                            null,
                            entry.Key,
                            index
                        )
                    );
                    Refresh();
                    return true;
                }
                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public new void RemoveAt(int index)
        {
            _lock.EnterWriteLock();
            try
            {
                var id = _ids[index];
                if (_concurrentDict.TryRemove(id, out var item))
                {
                    CollectionChanging(
                        new CollectionEventArgs(CollectionEventType.REMOVE, item, null, id, index)
                    );
                    _ids.RemoveAt(index);
                    CollectionChanged(
                        new CollectionEventArgs(CollectionEventType.REMOVE, item, null, id, index)
                    );
                }
                Refresh();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                return _concurrentDict.Values.Select(x => x.Value).GetEnumerator();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
