using System;
using UnityEngine;

namespace Darklight.Collection
{
    /// <summary>
    /// Defines the base interface for collection items.
    /// </summary>
    public interface ICollectionItem
    {
        int Id { get; }
        object Object { get; }

        // Add hash code contract
        int GetHashCode();
        bool Equals(object obj);
    }

    [Serializable]
    public class CollectionItem : ICollectionItem
    {
        [SerializeField]
        protected int _id;

        [SerializeField]
        protected object _object;

        public int Id
        {
            get => _id;
            protected set => _id = value;
        }
        public virtual object Object
        {
            get => _object;
            protected set => _object = value;
        }

        public override int GetHashCode() => _id.GetHashCode();

        public override bool Equals(object obj) => obj is CollectionItem item && _id == item._id;

        public CollectionItem(int id, object value)
        {
            _id = id;
            _object = value;
        }
    }

    [Serializable]
    public class CollectionItem<TValue> : CollectionItem
        where TValue : notnull
    {
        [SerializeField]
        private TValue _value;

        public TValue Value
        {
            get => _value;
            protected set
            {
                _value = value;
                _object = value;
            }
        }

        public CollectionItem(int id, TValue value)
            : base(id, value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Represents a key-value pair item in a collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [Serializable]
    public class KeyValueCollectionItem<TKey, TValue> : CollectionItem<TValue>
        where TKey : notnull
        where TValue : notnull
    {
        [SerializeField]
        private TKey _key;

        /// <summary>
        /// Gets the key of the item.
        /// </summary>
        public TKey Key
        {
            get => _key;
            protected set => _key = value;
        }

        /// <summary>
        /// Initializes a new instance of KeyValueCollectionItem.
        /// </summary>
        /// <param name="id">The ID of the item.</param>
        /// <param name="key">The key of the item.</param>
        /// <param name="value">The value of the item.</param>
        public KeyValueCollectionItem(int id, TKey key, TValue value)
            : base(id, value)
        {
            Key = key;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + Key.GetHashCode();
                hash = hash * 23 + (Value?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is KeyValueCollectionItem<TKey, TValue> other)
            {
                return Id == other.Id
                    && Key.Equals(other.Key)
                    && (Value?.Equals(other.Value) ?? other.Value == null);
            }
            return false;
        }
    }
}
