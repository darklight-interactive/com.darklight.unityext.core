using System;
using System.Collections.Generic;
using System.Linq;

namespace Darklight.UnityExt.Collection.Utilities
{
    /// <summary>
    /// Provides utility methods for key generation and management.
    /// </summary>
    public static class KeyGenerationUtils
    {
        /// <summary>
        /// Gets all possible keys for a given type.
        /// </summary>
        public static IEnumerable<TKey> GetAllPossibleKeys<TKey>() where TKey : notnull
        {
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
    }
} 