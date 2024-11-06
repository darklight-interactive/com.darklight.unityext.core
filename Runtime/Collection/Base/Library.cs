using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Darklight.UnityExt.Collection
{
    public interface ILibraryObserver
    {
        void OnItemAdded(ILibraryItem item);
        void OnItemRemoved(ILibraryItem item);
        void OnLibraryCleared();
    }

    /// <summary>
    /// Base class for all library collections providing thread-safe operations and collection change notifications.
    /// </summary>
    [Serializable]
    public abstract class Library
        : ISerializationCallbackReceiver,
            ICollection,
            IEnumerable<ILibraryItem>,
            IEnumerator,
            IDisposable,
            IReadOnlyCollection<ILibraryItem>,
            INotifyCollectionChanged
    {
        [SerializeField]
        protected GUISettings _guiSettings;

        [SerializeField]
        protected int _count;
        protected int _freeList = -1;
        protected int _currentIndex = -1;

        /// <summary>
        /// A synchronization object used to synchronize access to the library.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// A reader-writer lock used to synchronize access to the library.
        /// </summary>
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        /// <summary>
        /// A semaphore used to synchronize asynchronous access to the library.
        /// </summary>
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// A concurrent dictionary used to cache values in the library.
        /// </summary>
        private readonly ConcurrentDictionary<int, object> _concurrentCache = new();

        /// <summary>
        /// A list of observers for the library.
        /// </summary>
        private readonly List<ILibraryObserver> _observers = new();

        /// <summary>
        /// A synchronization object used to synchronize access to the observer list.
        /// </summary>
        private readonly object _observerLock = new();

        /// <summary>
        /// A flag indicating whether the library has been disposed.
        /// </summary>
        private volatile bool _isDisposed;

        /// <summary>
        /// A version number used to track modifications to the library.
        /// </summary>
        private long _version;

        /// <summary>
        /// An event handler for collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #region Thread-Safe Properties
        /// <summary>
        /// Gets the number of items in the library.
        /// </summary>
        public int Count => GetPropertyValue(_count);

        /// <summary>
        /// Gets the capacity of the library.
        /// </summary>
        public abstract int Capacity { get; }

        /// <summary>
        /// Indicates whether the library is synchronized.
        /// </summary>
        public bool IsSynchronized => true;

        /// <summary>
        /// Gets the synchronization root for the library.
        /// </summary>
        public object SyncRoot => _syncRoot;

        /// <summary>
        /// Gets the current item during enumeration.
        /// </summary>
        public object Current => GetCurrentItem();
        #endregion

        /// <summary>
        /// Removes all items from the library.
        /// </summary>
        /// <remarks>
        /// This operation resets the free list and clears all items.
        /// </remarks>
        public abstract void Clear();

        /// <summary>
        /// Resets the library to its initial state.
        /// </summary>
        /// <remarks>
        /// This operation reinitializes the library to its default state.
        /// </remarks>
        public abstract void Reset();

        /// <summary>
        /// Refreshes the library, updating internal state and removing invalid entries.
        /// </summary>
        /// <remarks>
        /// This operation may reorganize internal data structures and update indices.
        /// </remarks>
        public abstract void Refresh();

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        /// <remarks>
        /// The removed item's ID will be added to the free list for reuse.
        /// </remarks>
        public abstract void RemoveAt(int index);

        /// <summary>
        /// Called before Unity serializes this object.
        /// </summary>
        public abstract void OnBeforeSerialize();

        /// <summary>
        /// Called after Unity deserializes this object.
        /// </summary>
        public abstract void OnAfterDeserialize();

        /// <summary>
        /// Copies the library items to the specified array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="index">The starting index in the destination array.</param>
        public void CopyTo(Array array, int index)
        {
            try
            {
                _rwLock.EnterReadLock();
                if (array == null)
                    throw new ArgumentNullException(nameof(array));
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                if (array.Length - index < Count)
                    throw new ArgumentException("Array is too small");

                CopyToArray(array, index);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Moves to the next valid item in the library.
        /// </summary>
        /// <returns>True if a valid item was found, false otherwise.</returns>
        public bool MoveNext()
        {
            _currentIndex++;
            while (_currentIndex < GetItemCount())
            {
                if (IsValidId(_currentIndex))
                    return true;
                _currentIndex++;
            }
            return false;
        }

        /// <summary>
        /// Resets the enumerator to the initial position.
        /// </summary>
        public void ResetEnumerator()
        {
            _currentIndex = -1;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the library.
        /// </summary>
        /// <returns>An enumerator for the library.</returns>
        public virtual IEnumerator GetEnumerator() =>
            ((IEnumerable<ILibraryItem>)this).GetEnumerator();

        /// <summary>
        /// Disposes of the library and releases resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Adds an observer to the library.
        /// </summary>
        /// <param name="observer">The observer to add.</param>
        public void AddObserver(ILibraryObserver observer)
        {
            ThrowIfDisposed();
            lock (_observerLock)
            {
                _observers.Add(observer);
            }
        }

        /// <summary>
        /// Removes an observer from the library.
        /// </summary>
        /// <param name="observer">The observer to remove.</param>
        public void RemoveObserver(ILibraryObserver observer)
        {
            ThrowIfDisposed();
            lock (_observerLock)
            {
                _observers.Remove(observer);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the library.
        /// </summary>
        /// <returns>An enumerator for the library.</returns>
        IEnumerator<ILibraryItem> IEnumerable<ILibraryItem>.GetEnumerator()
        {
            return ExecuteRead(() => GetItems().GetEnumerator());
        }

        /// <summary>
        /// Notifies all observers of a change.
        /// </summary>
        /// <param name="notification">The action to perform on each observer.</param>
        protected void NotifyObservers(Action<ILibraryObserver> notification)
        {
            ThrowIfDisposed();
            lock (_observerLock)
            {
                foreach (var observer in _observers)
                {
                    notification(observer);
                }
            }
        }

        #region Thread-Safe Operations
        /// <summary>
        /// Executes an action within a read lock.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        protected void ExecuteRead(Action action)
        {
            ThrowIfDisposed();
            try
            {
                _rwLock.EnterReadLock();
                action();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Executes a function within a read lock.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <returns>The result of the function.</returns>
        protected T ExecuteRead<T>(Func<T> func)
        {
            ThrowIfDisposed();
            try
            {
                _rwLock.EnterReadLock();
                return func();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Executes an action within a write lock.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        protected void ExecuteWrite(Action action)
        {
            ThrowIfDisposed();
            try
            {
                _rwLock.EnterWriteLock();
                action();
                // Increment version after successful write operation to track the modification
                IncrementVersion();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Executes a function within a write lock.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <returns>The result of the function.</returns>
        protected T ExecuteWrite<T>(Func<T> func)
        {
            ThrowIfDisposed();
            try
            {
                _rwLock.EnterWriteLock();
                var result = func();
                IncrementVersion();
                return result;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Executes an asynchronous action within a read lock.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        protected async Task ExecuteReadAsync(Func<Task> action)
        {
            ThrowIfDisposed();
            try
            {
                await _asyncLock.WaitAsync();
                await action();
            }
            finally
            {
                _asyncLock.Release();
            }
        }

        /// <summary>
        /// Executes an asynchronous function within a read lock.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <returns>The result of the function.</returns>
        protected async Task<T> ExecuteReadAsync<T>(Func<Task<T>> func)
        {
            ThrowIfDisposed();
            try
            {
                await _asyncLock.WaitAsync();
                return await func();
            }
            finally
            {
                _asyncLock.Release();
            }
        }

        /// <summary>
        /// Gets a property value within a read lock.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to get.</param>
        /// <returns>The value.</returns>
        protected T GetPropertyValue<T>(T value)
        {
            ThrowIfDisposed();
            try
            {
                _rwLock.EnterReadLock();
                return value;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Increments the version of the library in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// This method is used to track modifications to the collection.
        /// The version number serves multiple purposes:
        /// 1. Detect concurrent modifications during enumeration
        /// 2. Support optimistic concurrency control
        /// 3. Track collection changes for cache invalidation
        ///
        /// Interlocked.Increment ensures atomic operations in multi-threaded scenarios,
        /// preventing race conditions when multiple threads modify the collection simultaneously.
        /// </remarks>
        protected void IncrementVersion()
        {
            Interlocked.Increment(ref _version);
        }

        /// <summary>
        /// Gets all valid items in the library.
        /// </summary>
        /// <remarks>
        /// Implementations should check the _version number before returning items
        /// to ensure consistency during enumeration. If the version changes during
        /// enumeration, it indicates concurrent modification.
        /// </remarks>
        protected void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Library));
        }

        /// <summary>
        /// Gets all valid items in the library.
        /// </summary>
        protected abstract IEnumerable<ILibraryItem> GetItems();
        #endregion

        #region Abstract Methods Documentation
        /// <summary>
        /// Gets the next available ID for a new item.
        /// </summary>
        /// <returns>The next available ID in the sequence.</returns>
        /// <remarks>
        /// This method handles ID recycling through the free list before generating new IDs.
        /// </remarks>
        protected abstract int GetNextId();

        /// <summary>
        /// Releases an ID back to the pool of available IDs.
        /// </summary>
        /// <param name="id">The ID to release.</param>
        /// <remarks>
        /// Released IDs are added to the free list for reuse.
        /// </remarks>
        protected abstract void ReleaseId(int id);

        /// <summary>
        /// Checks if the given ID is valid within the library.
        /// </summary>
        /// <param name="id">The ID to validate.</param>
        /// <returns>True if the ID is valid and occupied, false otherwise.</returns>
        protected abstract bool IsValidId(int id);

        /// <summary>
        /// Ensures the library has enough capacity to store the specified number of items.
        /// </summary>
        /// <param name="capacity">The minimum capacity required.</param>
        /// <remarks>
        /// This method may trigger internal data structure resizing.
        /// </remarks>
        protected abstract void EnsureCapacity(int capacity);

        /// <summary>
        /// Copies the library items to the specified array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="index">The starting index in the destination array.</param>
        /// <remarks>
        /// The array must be large enough to hold all items starting at the specified index.
        /// </remarks>
        protected abstract void CopyToArray(Array array, int index);

        /// <summary>
        /// Gets the current item during enumeration.
        /// </summary>
        /// <returns>The current item, or null if the current position is invalid.</returns>
        protected abstract object GetCurrentItem();

        /// <summary>
        /// Gets the total number of items in the library, including free slots.
        /// </summary>
        /// <returns>The total count of item slots.</returns>
        protected abstract int GetItemCount();

        /// <summary>
        /// Disposes of the library and releases resources.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources, false otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                _rwLock.Dispose();
                _asyncLock.Dispose();
            }

            _isDisposed = true;
        }
        #endregion

        #region Protected Helper Methods
        /// <summary>
        /// Invokes the CollectionChanged event.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="item">The item to include in the event arguments.</param>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item = null)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
        }

        /// <summary>
        /// Tries to get a cached value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key to retrieve the value.</param>
        /// <param name="value">The retrieved value.</param>
        /// <returns>True if the value was found, false otherwise.</returns>
        protected bool TryGetCachedValue<T>(int key, out T value)
        {
            if (_concurrentCache.TryGetValue(key, out var obj) && obj is T typedValue)
            {
                value = typedValue;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Sets a cached value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key to set the value.</param>
        /// <param name="value">The value to set.</param>
        protected void SetCachedValue<T>(int key, T value)
        {
            _concurrentCache.AddOrUpdate(key, value, (_, __) => value);
        }
        #endregion

        /// <summary>
        /// Settings for the GUI.
        /// </summary>
        public class GUISettings
        {
            public bool ReadOnlyKey = false;
            public bool ReadOnlyValue = false;
            public bool ShowCacheStats = false;
        }
    }
}
