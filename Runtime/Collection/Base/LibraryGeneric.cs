using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Collection;
using UnityEngine;

namespace DarkLight.UnityExt.Collection
{
    /// <summary>
    /// Generic implementation of Library that supports key-value pairs with thread-safe operations.
    /// </summary>
    [Serializable]
    public class LibraryGeneric<TKey, TValue>
        : Library,
            IDictionary<TKey, TValue>,
            IEnumerator<KeyValuePair<TKey, TValue>>
        where TKey : notnull
        where TValue : notnull
    {
        /// <summary>
        /// The initial bucket count for the library.
        /// </summary>
        protected const int INITIAL_BUCKET_COUNT = 8;

        #region Serialized Fields

        /// <summary>
        /// A flag indicating whether the library should default to all keys.
        /// </summary>
        [SerializeField]
        protected bool _defaultToAllKeys = false;

        /// <summary>
        /// The list of items in the library.
        /// </summary>
        [SerializeField]
        protected List<LibraryItem<TKey, TValue>> _items = new();

        [SerializeField]
        protected List<LibraryItem<TKey, TValue>> _testList = new();

        /// <summary>
        /// The list of buckets for the library. Each bucket contains an index into the _items list,
        /// forming a hash table structure. A value of -1 indicates an empty bucket. When adding items,
        /// the key's hash code modulo the bucket count determines which bucket the item goes into.
        /// If there's a collision, linear probing is used to find the next available bucket.
        /// /// </summary>
        [SerializeField]
        protected List<int> _buckets = new();

        /// <summary>
        /// The set of required keys for the library.
        /// </summary>
        [SerializeField]
        protected HashSet<TKey> _requiredKeys = new();

        #endregion
        #region Constructors

        public LibraryGeneric()
        {
            _buckets = new List<int>(INITIAL_BUCKET_COUNT);
            for (int i = 0; i < INITIAL_BUCKET_COUNT; i++)
            {
                _buckets.Add(-1);
            }
            InternalReset();
        }

        public LibraryGeneric(bool defaultToAllKeys)
            : this()
        {
            _defaultToAllKeys = defaultToAllKeys;
        }
        #endregion

        #region Events

        /// <summary>
        /// An event handler for when an item is added to the library.
        /// </summary>
        public event Action<TKey, TValue> ItemAdded;

        /// <summary>
        /// An event handler for when an item is removed from the library.
        /// </summary>
        public event Action<TKey> ItemRemoved;

        #endregion
        #region Abstract Implementation
        /// <summary>
        /// Gets the capacity of the library.
        /// </summary>
        public override int Capacity => _items.Capacity;

        /// <summary>
        /// Gets the keys of the library.
        /// </summary>
        public ICollection<TKey> Keys =>
            _items.Where(i => i.IsOccupied).Select(i => i.Key).ToList();

        /// <summary>
        /// Gets the values of the library.
        /// </summary>
        public ICollection<TValue> Values =>
            _items.Where(i => i.IsOccupied).Select(i => i.Value).ToList();

        /// <summary>
        /// Indicates whether the library is read-only.
        /// </summary>
        public bool IsReadOnly => false;
        #endregion

        #region IEnumerator<KeyValuePair<TKey, TValue>> Implementation
        /// <summary>
        /// Gets the current item during enumeration.
        /// </summary>
        KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current =>
            IsValidId(_currentIndex)
                ? new KeyValuePair<TKey, TValue>(
                    _items[_currentIndex].Key,
                    _items[_currentIndex].Value
                )
                : default;
        #endregion

        #region IDictionary Implementation
        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out TValue value))
                    return value;
                throw new KeyNotFoundException($"Key '{key}' not found in library.");
            }
            set => InternalKeyValueAdd(key, value);
        }

        public override void Clear() => InternalReset();

        public override void Reset() => InternalReset();

        public override void Refresh() => InternalRefresh();

        public override void OnBeforeSerialize()
        {
            return;

            Debug.Log($"[{GetType().Name}] OnBeforeSerialize - Items count: {_items.Count}, Valid items: {_items.Count(i => i.IsOccupied)}");
            LogItemsState();
        }

        public override void OnAfterDeserialize()
        {
            return;

            Debug.Log($"[{GetType().Name}] Starting deserialization...");
            Debug.Log($"Before processing - Items count: {_items.Count}, Valid items: {_items.Count(i => i.IsOccupied)}");
            
            ExecuteWrite(() =>
            {
                // Store existing items without clearing
                var validItems = _items.Where(i => i.IsOccupied).ToList();
                Debug.Log($"Valid items found: {validItems.Count}");

                // Don't clear _items here, just update/add as needed
                foreach (var item in validItems)
                {
                    if (!_items.Any(i => EqualityComparer<TKey>.Default.Equals(i.Key, item.Key)))
                    {
                        InternalKeyValueAdd(item.Key, item.Value);
                    }
                }

                // Update count
                _count = _items.Count(i => i.IsOccupied);
                
                // Rebuild buckets if needed
                if (_buckets.Count < INITIAL_BUCKET_COUNT)
                {
                    ResizeBuckets(Math.Max(INITIAL_BUCKET_COUNT, _items.Count * 2));
                }
            });

            Debug.Log($"After processing - Items count: {_items.Count}, Valid items: {_items.Count(i => i.IsOccupied)}");
            LogItemsState();
        }

        public new void Dispose()
        {
            // Cleanup any resources specific to this implementation
            base.Dispose(true);
        }

        /// <summary>
        /// Tries to get the value associated with the specified key.
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;
            var item = _items.FirstOrDefault(i =>
                EqualityComparer<TKey>.Default.Equals(i.Key, key)
            );
            if (item.IsOccupied)
            {
                value = item.Value;
                return true;
            }
            return false;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates the default key for the library.
        /// </summary>
        public virtual TKey CreateDefaultKey() => GetDefaultKey();

        /// <summary>
        /// Creates the default value for the library.
        /// </summary>
        public virtual TValue CreateDefaultValue() => default;

        /// <summary>
        /// Adds a default item to the library.
        /// </summary>
        public virtual void AddDefaultItem() =>
            InternalKeyValueAdd(GetDefaultKey(), CreateDefaultValue());

        /// <summary>
        /// Adds an item with a default value to the library.
        /// </summary>
        public virtual void AddItemWithDefaultValue(TKey key) =>
            InternalKeyValueAdd(key, CreateDefaultValue());

        /// <summary>
        /// Removes an item at the specified index.
        /// </summary>
        public override void RemoveAt(int index)
        {
            if (index >= 0 && index < _items.Count)
                InternalKeyValueRemove(_items[index].Key);
        }

        /// <summary>
        /// Sets the required keys for the library.
        /// </summary>
        public void SetRequiredKeys(IEnumerable<TKey> keys)
        {
            _requiredKeys = new HashSet<TKey>(keys);
            EnsureRequiredKeys();
        }

        /// <summary>
        /// Adds a key-value pair to the library.
        /// </summary>
        public void Add(TKey key, TValue value) => InternalKeyValueAdd(key, value);

        /// <summary>
        /// Removes a key-value pair from the library.
        /// </summary>
        public bool Remove(TKey key) => InternalKeyValueRemove(key);

        /// <summary>
        /// Checks if the library contains a key.
        /// </summary>
        public bool ContainsKey(TKey key) =>
            _items.Any(i => i.IsOccupied && EqualityComparer<TKey>.Default.Equals(i.Key, key));

        /// <summary>
        /// Adds a key-value pair to the library.
        /// </summary>
        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        /// <summary>
        /// Removes a key-value pair from the library.
        /// </summary>
        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        /// <summary>
        /// Checks if the library contains a key-value pair.
        /// </summary>
        public bool Contains(KeyValuePair<TKey, TValue> item) =>
            _items.Any(i =>
                i.IsOccupied
                && EqualityComparer<TKey>.Default.Equals(i.Key, item.Key)
                && EqualityComparer<TValue>.Default.Equals(i.Value, item.Value)
            );

        /// <summary>
        /// Copies the library to an array.
        /// </summary>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("Array is too small");

            foreach (var item in _items.Where(i => i.IsOccupied))
            {
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Gets an enumerator for the library.
        /// </summary>
        public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
            _items
                .Where(i => i.IsOccupied)
                .Select(i => new KeyValuePair<TKey, TValue>(i.Key, i.Value))
                .GetEnumerator();

        #region Unity Object Specific Methods
        /// <summary>
        /// Tries to get a component from the value associated with the specified key.
        /// </summary>
        public virtual TComponent TryGetComponent<TComponent>(TKey key)
            where TComponent : Component
        {
            if (
                typeof(TValue) == typeof(GameObject)
                && TryGetValue(key, out TValue value)
                && value is GameObject gameObject
            )
            {
                return gameObject.GetComponent<TComponent>();
            }
            return null;
        }

        /// <summary>
        /// Gets an enumerator for the library.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected override IEnumerable<ILibraryItem> GetItems() =>
            ExecuteRead(() => _items.Where(i => i.IsOccupied).Cast<ILibraryItem>());

        protected override void CopyToArray(Array array, int index)
        {
            foreach (var item in _items.Where(i => i.IsOccupied))
            {
                array.SetValue(new KeyValuePair<TKey, TValue>(item.Key, item.Value), index++);
            }
        }

        protected override int GetItemCount() => _items.Count;

        protected override int GetNextId()
        {
            if (_freeList >= 0)
            {
                int index = _freeList;
                _freeList = _items[index].Next;
                return index;
            }
            return _items.Count;
        }

        /// <summary>
        /// Releases an ID by marking it as free.
        /// </summary>
        /// <param name="id">The ID to release.</param>
        protected override void ReleaseId(int id)
        {
            if (!IsValidId(id))
                return;

            //_items[id] = LibraryItem<TKey, TValue>.CreateFree(_freeList);
            _freeList = id;
            _count--;
        }

        /// <summary>
        /// Checks if an ID is valid.
        /// </summary>
        /// <param name="id">The ID to check.</param>
        /// <returns>True if the ID is valid, false otherwise.</returns>
        protected override bool IsValidId(int id)
        {
            return id >= 0 && id < _items.Count && _items[id].IsOccupied;
        }

        /// <summary>
        /// Ensures that the library has the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity to ensure.</param>
        protected override void EnsureCapacity(int capacity)
        {
            if (_items.Capacity < capacity)
            {
                _items.Capacity = capacity;
            }

            if (_buckets.Capacity < capacity)
            {
                _buckets.Capacity = capacity;
            }
        }

        /// <summary>
        /// Gets the current item during enumeration.
        /// </summary>
        /// <returns>The current item.</returns>
        protected override object GetCurrentItem() =>
            IsValidId(_currentIndex)
                ? new KeyValuePair<TKey, TValue>(
                    _items[_currentIndex].Key,
                    _items[_currentIndex].Value
                )
                : default;
        #endregion

        #region Protected Methods
        /// <summary>
        /// Adds a value to the library.
        /// </summary>
        /// <param name="value">The value to add.</param>
        protected virtual void InternalAdd(TValue value)
        {
            // Override to prevent direct value adds in key-value library
            throw new InvalidOperationException(
                "Cannot add value directly in key-value library. Use Add(TKey, TValue) instead."
            );
        }

        /// <summary>
        /// Adds a key-value pair to the library.
        /// </summary>
        /// <param name="key">The key to add. If null, uses default key.</param>
        /// <param name="value">The value to add. If null, uses default value.</param>
        protected virtual void InternalKeyValueAdd(TKey key, TValue value)
        {
            // Handle null key
            key ??= CreateDefaultKey();
            
            // Handle null value
            value ??= CreateDefaultValue();

            // Check for existing key
            if (_items.Any(i => i.IsOccupied && EqualityComparer<TKey>.Default.Equals(i.Key, key)))
            {
                Debug.LogWarning($"Key '{key}' already exists in the library. Skipping add.");
                return;
            }

            Debug.Log($"Adding key '{key}' to the {GetType().Name} library with value '{value}'. Current items: {_items.Count}");

            try
            {
                var newItem = new LibraryItem<TKey, TValue>(_items.Count, key, value);
                
                _items.Add(newItem);
                _count++;

                Debug.Log($"Successfully added item with key '{key}'. Total items: {_items.Count}, Occupied items: {_items.Count(i => i.IsOccupied)}");
                LogItemsState();
                ItemAdded?.Invoke(key, value);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to add item with key '{key}': {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a key-value pair from the library.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if the key was removed, false otherwise.</returns>
        protected virtual bool InternalKeyValueRemove(TKey key)
        {
            if (_requiredKeys.Contains(key))
            {
                Debug.LogError($"Cannot remove required key '{key}' from the library.");
                return false;
            }

            int hashCode = key.GetHashCode() & 0x7FFFFFFF;
            int bucket = hashCode % _buckets.Count;
            int last = -1;
            int current = _buckets[bucket];

            while (current >= 0)
            {
                Debug.Log($"Checking item at index {current} with key '{_items[current].Key}'");
                if (EqualityComparer<TKey>.Default.Equals(_items[current].Key, key))
                {
                    if (last < 0)
                    {
                        _buckets[bucket] = _items[current].Next;
                    }
                    else
                    {
                        var lastItem = _items[last].WithNext(_items[current].Next);
                        _items[last] = lastItem;
                    }

                    //var freeItem = LibraryItem<TKey, TValue>.CreateFree(_freeList);
                    //_items[current] = freeItem;
                    _freeList = current;
                    _count--;

                    ItemRemoved?.Invoke(key);

                    Debug.Log($"Confirmed that the key '{key}' was removed from the {GetType().Name} library.");
                    return true;
                }
                last = current;
                current = _items[current].Next;
            }

            return false;
        }

        /// <summary>
        /// Refreshes the library.
        /// </summary>
        protected virtual void InternalRefresh()
        {
            Debug.Log($"[{GetType().Name}] InternalRefresh - Items count: {_items.Count}, Valid items: {_items.Count(i => i.IsOccupied)}");

            HandleTypeSpecificRefresh();

            int index = 0;
            EnsureRequiredKeys();

            var validItems = _items.Where(i => i.IsOccupied).ToList();
            var newItems = new List<LibraryItem<TKey, TValue>>();

            foreach (var item in validItems)
            {
                newItems.Add(new LibraryItem<TKey, TValue>(index, item.Key, item.Value));
                index++;
            }

            _items = newItems;
            _count = _items.Count;

            ResizeBuckets(_buckets.Count);
        }

        /// <summary>
        /// Handles type-specific refresh logic.
        /// </summary>
        protected virtual void HandleTypeSpecificRefresh()
        {
            Debug.Log($"[{GetType().Name}] HandleTypeSpecificRefresh - Default to all keys: {_defaultToAllKeys}");
            if (typeof(TKey).IsEnum && _defaultToAllKeys)
            {
                SetRequiredKeys(GetAllPossibleKeys());
            }
        }

        /// <summary>
        /// Handles type-specific reset logic.
        /// </summary>
        protected virtual void HandleTypeSpecificReset()
        {
            Debug.Log($"[{GetType().Name}] HandleTypeSpecificReset - Default to all keys: {_defaultToAllKeys}");
            if (typeof(TKey) == typeof(int) || typeof(TKey) == typeof(string))
            {
                var keys = GetAllPossibleKeys();
                foreach (var key in keys)
                {
                    Add(key, CreateDefaultValue());
                }
            }
        }

        /// <summary>
        /// Resets the library to its initial state.
        /// </summary>
        protected virtual void InternalReset()
        {
            Debug.Log($"[{GetType().Name}] InternalReset - Items count: {_items.Count}, Valid items: {_items.Count(i => i.IsOccupied)}");
            _items = new List<LibraryItem<TKey, TValue>>();
            _count = 0;
            _freeList = -1;
            InternalRefresh();
            HandleTypeSpecificReset();
            LogItemsState();
        }

        /// <summary>
        /// Gets all possible keys for the library.
        /// </summary>
        /// <returns>An enumerable collection of keys.</returns>
        protected virtual IEnumerable<TKey> GetAllPossibleKeys()
        {
            Debug.Log($"[{GetType().Name}] GetAllPossibleKeys - Type: {typeof(TKey)}");
            if (typeof(TKey).IsEnum)
            {
                return Enum.GetValues(typeof(TKey)).Cast<TKey>();
            }
            else if (typeof(TKey) == typeof(int))
            {
                return Enumerable.Range(0, 10).Cast<object>().Select(x => (TKey)x);
            }
            else if (typeof(TKey) == typeof(string))
            {
                return Enumerable.Range(0, 10).Select(i => (TKey)(object)$"DefaultKey_{i}");
            }

            return Enumerable.Empty<TKey>();
        }

        /// <summary>
        /// Gets the default key for the library.
        /// </summary>
        /// <returns>The default key.</returns>
        protected virtual TKey GetDefaultKey()
        {
            Debug.Log($"[{GetType().Name}] GetDefaultKey - Type: {typeof(TKey)}");
            if (typeof(TKey).IsEnum)
            {
                var allKeys = GetAllPossibleKeys();
                foreach (TKey key in allKeys)
                {
                    if (!ContainsKey(key))
                    {
                        return key;
                    }
                }
            }
            return default;
        }

        /// <summary>
        /// Resizes the buckets of the library.
        /// </summary>
        /// <param name="newSize">The new size of the buckets.</param>
        protected void ResizeBuckets(int newSize)
        {
            Debug.Log($"[{GetType().Name}] ResizeBuckets - New size: {newSize}");
            var newBuckets = new List<int>(newSize);
            for (int i = 0; i < newSize; i++)
            {
                newBuckets.Add(-1);
            }

            // Rehash existing items
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].IsOccupied)
                {
                    int bucket = (_items[i].Key.GetHashCode() & 0x7FFFFFFF) % newSize;
                    _items[i] = _items[i].WithNext(newBuckets[bucket]);
                    newBuckets[bucket] = i;
                }
            }

            _buckets = newBuckets;
        }

        /// <summary>
        /// Validates a value based on its type.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        protected virtual void ValidateValue(TValue value)
        {
            Debug.Log($"[{GetType().Name}] ValidateValue - Value: {value}");
            if (typeof(TValue).IsSubclassOf(typeof(UnityEngine.Object)))
            {
                if (value == null)
                {
                    throw new ArgumentNullException(
                        nameof(value),
                        "Unity Object reference cannot be null"
                    );
                }

                if (value is UnityEngine.Object unityObj && unityObj == null)
                {
                    throw new ArgumentException(
                        "Unity Object reference is destroyed",
                        nameof(value)
                    );
                }
            }
        }
        #endregion

        /// <summary>
        /// Ensures that all required keys are present in the library.
        /// </summary>
        private void EnsureRequiredKeys()
        {
            Debug.Log($"[{GetType().Name}] EnsureRequiredKeys - Required keys count: {(_requiredKeys?.Count ?? 0)}");
            if (_requiredKeys == null || _requiredKeys.Count == 0)
                return;

            foreach (TKey key in _requiredKeys.Where(key => !ContainsKey(key)))
            {
                AddItemWithDefaultValue(key);
            }
        }

        /// <summary>
        /// Debug method to verify serialization
        /// </summary>
        protected void LogItemsState()
        {
            Debug.Log($"[{GetType().Name}] Items count: {_items.Count}");
            foreach (var item in _items.Where(i => i.IsOccupied))
            {
                Debug.Log($"Key: {item.Key}, Value: {item.Value}");
            }
        }
        #endregion
    }

    [Serializable]
    public class LibraryGeneric<TValue> : LibraryGeneric<int, TValue>
    {
        public LibraryGeneric(bool defaultToAllKeys = false)
            : base(defaultToAllKeys) { }
    }
}
