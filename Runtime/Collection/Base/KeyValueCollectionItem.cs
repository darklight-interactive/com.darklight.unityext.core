using System;

namespace Darklight.UnityExt.Collection
{
    /// <summary>
    /// Represents a key-value pair item in a collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [Serializable]
    public class KeyValueCollectionItem<TKey, TValue> : ICollectionItem
        where TKey : notnull
        where TValue : notnull
    {
        private readonly int _id;
        private readonly TKey _key;
        private readonly TValue _value;

        /// <summary>
        /// Gets the ID of the item.
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// Gets the value of the item.
        /// </summary>
        public object Value => _value;

        /// <summary>
        /// Gets the key of the item.
        /// </summary>
        public TKey Key => _key;

        /// <summary>
        /// Gets the typed value of the item.
        /// </summary>
        public TValue TypedValue => _value;

        /// <summary>
        /// Initializes a new instance of KeyValueCollectionItem.
        /// </summary>
        /// <param name="id">The ID of the item.</param>
        /// <param name="key">The key of the item.</param>
        /// <param name="value">The value of the item.</param>
        public KeyValueCollectionItem(int id, TKey key, TValue value)
        {
            _id = id;
            _key = key;
            _value = value;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _id.GetHashCode();
                hash = hash * 23 + _key.GetHashCode();
                hash = hash * 23 + (_value?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is KeyValueCollectionItem<TKey, TValue> other)
            {
                return _id == other._id && 
                       _key.Equals(other._key) && 
                       (_value?.Equals(other._value) ?? other._value == null);
            }
            return false;
        }
    }
} 