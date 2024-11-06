using System;
using UnityEngine;

namespace Darklight.UnityExt.Collection
{
    public interface ILibraryItem
    {
        int Id { get; }
        int Next { get; }
        bool IsOccupied { get; }
        object Value { get; }
    }

    [Serializable]
    public class LibraryItem<TKey, TValue> : ILibraryItem
        where TKey : notnull
        where TValue : notnull
    {
        [SerializeField]
        int _id;
        [SerializeField]
        TKey _key;
        [SerializeField]
        TValue _value;

        [SerializeField]
        int _next;
        [SerializeField]
        bool _isOccupied;


        public int Id => _id;
        public int Next => _next;
        public bool IsOccupied => _isOccupied;
        public TKey Key => _key;
        public TValue Value => _value;

        object ILibraryItem.Value => Value;

        public LibraryItem(int id, TKey key, TValue value, int next = -1, bool isOccupied = true)
        {
            _id = id;
            _next = next;
            _isOccupied = isOccupied;
            _key = key;
            _value = value;
        }

        public LibraryItem<TKey, TValue> WithNext(int next) =>
            new(Id, Key, Value, next, IsOccupied);

        public LibraryItem<TKey, TValue> WithId(int id) =>
            new(id, Key, Value, Next, IsOccupied);
    }

    [Serializable]
    public readonly struct LibraryItem<TValue> : ILibraryItem
        where TValue : notnull
    {
        private readonly LibraryItem<int, TValue> _item;

        public int Id => _item.Id;
        public int Next => _item.Next;
        public bool IsOccupied => _item.IsOccupied;
        public TValue Value => _item.Value;
        object ILibraryItem.Value => Value;

        public LibraryItem(int id, TValue value, int next = -1, bool isOccupied = true)
        {
            _item = new LibraryItem<int, TValue>(id, id, value, next, isOccupied);
        }

        public LibraryItem<TValue> WithNext(int next) =>
            new(Id, Value, next, IsOccupied);

        public LibraryItem<TValue> WithId(int id) =>
            new(id, Value, Next, IsOccupied);

        public static LibraryItem<TValue> CreateFree(int next) =>
            new(0, default, next, false);

        public static implicit operator LibraryItem<int, TValue>(LibraryItem<TValue> item) =>
            item._item;
    }

    // Extension methods for conversions
    public static class LibraryItemExtensions
    {
        public static LibraryItem<TValue> ToSingleValueItem<TKey, TValue>(
            this LibraryItem<TKey, TValue> item)
            where TKey : notnull
            where TValue : notnull =>
            new(item.Id, item.Value, item.Next, item.IsOccupied);

        public static LibraryItem<TKey, TValue> WithValue<TKey, TValue>(
            this LibraryItem<TKey, TValue> item, TValue value)
            where TKey : notnull
            where TValue : notnull =>
            new(item.Id, item.Key, value, item.Next, item.IsOccupied);

        public static LibraryItem<TValue> WithValue<TValue>(
            this LibraryItem<TValue> item, TValue value)
            where TValue : notnull =>
            new(item.Id, value, item.Next, item.IsOccupied);
    }
}
