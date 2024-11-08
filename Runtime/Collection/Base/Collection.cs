using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Collection;

namespace Darklight.UnityExt.Collection
{
    /// <summary>
    /// Abstract class for a library.
    /// </summary>
    [Serializable]
    public abstract class CollectionLibrary
        : ICollection<CollectionItem>,
            IEnumerable<CollectionItem>,
            IEnumerable,
            ICollection,
            IList<CollectionItem>,
            IEquatable<CollectionLibrary>,
            IDisposable
    {
        public abstract IEnumerable<CollectionItem> Items { get; }
        public abstract IEnumerable<int> IDs { get; }
        public abstract IEnumerable<object> ObjectValues { get; }
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
        public abstract void Add(CollectionItem item);

        /// <summary>
        /// Determines whether the ICollection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the ICollection.</param>
        /// <returns>true if item is found in the ICollection; otherwise, false.</returns>
        public abstract bool Contains(CollectionItem item);

        /// <summary>
        /// Removes the first occurrence of a specific object from the ICollection.
        /// </summary>
        /// <param name="item">The object to remove from the ICollection.</param>
        /// <returns>true if item was successfully removed from the ICollection; otherwise, false. This method also returns false if item is not found in the original ICollection.</returns>
        public abstract bool Remove(CollectionItem item);

        /// <summary>
        /// Copies the elements of the ICollection to an ILibraryItem[], starting at a particular ILibraryItem[] index.
        /// </summary>
        /// <param name="array">The one-dimensional ILibraryItem[] that is the destination of the elements copied from ICollection.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public abstract void CopyTo(CollectionItem[] array, int arrayIndex);

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
        public abstract IEnumerator<CollectionItem> GetEnumerator();
        #endregion ---- < IEnumerator Implementation > ---------------------------------


        #region ---- < IList Implementation > ---------------------------------
        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The item at the specified index.</returns>

        public abstract CollectionItem this[int index] { get; set; }

        /// <summary>
        /// Gets the index of the specified item in the collection.
        /// </summary>
        /// <param name="item">The item to locate.</param>
        /// <returns>The index of the item if found; otherwise, -1.</returns>
        public abstract int IndexOf(CollectionItem item);

        /// <summary>
        /// Inserts an item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which to insert the item.</param>
        /// <param name="item">The item to insert.</param>
        public abstract void Insert(int index, CollectionItem item);

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
        public abstract bool AddOrUpdate(CollectionItem item);

        /// <summary>
        /// Adds a range of items to the collection.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public abstract void AddRange(IEnumerable<CollectionItem> items);

        /// <summary>
        /// Adds a default item to the collection.
        /// </summary>
        public abstract void AddDefaultItem();

        /// <summary>
        /// Removes a range of items from the collection.
        /// </summary>
        /// <param name="items">The items to remove.</param>
        public abstract void RemoveRange(IEnumerable<CollectionItem> items);

        /// <summary>
        /// Replaces an item in the collection with a new item.
        /// </summary>
        /// <param name="item">The item to replace.</param>
        /// <param name="newItem">The new item to replace the old item with.</param>
        public abstract void Replace(CollectionItem item, CollectionItem newItem);

        /// <summary>
        /// Removes all items from the collection that match the predicate.
        /// </summary>
        /// <param name="predicate">The condition to test items against.</param>
        public abstract void RemoveWhere(Func<CollectionItem, bool> predicate);

        /// <summary>
        /// Tries to get an item by its ID.
        /// </summary>
        /// <param name="id">The ID to look for.</param>
        /// <param name="item">The found item, if any.</param>
        /// <returns>True if the item was found, false otherwise.</returns>
        public abstract bool TryGetItem(int id, out CollectionItem item);

        /// <summary>
        /// Refreshes the collection by updating the internal list of items.
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Generates a hash code for the collection based on its items.
        /// </summary>
        /// <returns>A hash code that represents the current collection state.</returns>
        /// <remarks>
        /// The hash is computed using a combination of all item hashes in the collection.
        /// For consistent hashing behavior, ensure all CollectionItems implement GetHashCode properly.
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
        /// Gets the next available ID in the collection by finding the first gap in the sequence.
        /// </summary>
        /// <param name="id">The next available ID.</param>
        /// <remarks>
        /// This method finds the first missing number in the sorted sequence of IDs.
        /// If no gaps exist, returns the next number after the highest existing ID.
        /// </remarks>
        protected void GetNextAvailableID(out int id)
        {
            if (!IDs.Any())
            {
                id = 0;
                return;
            }

            var sortedIds = IDs.OrderBy(x => x);
            id = Enumerable.Range(0, sortedIds.Max() + 2).Except(sortedIds).First();
        }
    }
}
