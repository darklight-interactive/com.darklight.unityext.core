using System;

namespace Darklight.UnityExt.Collection
{
    /// <summary>
    /// Defines the base interface for collection items.
    /// </summary>
    public interface ICollectionItem
    {
        int Id { get; }
        object Value { get; }

        // Add hash code contract
        int GetHashCode();
        bool Equals(object obj);
    }
} 