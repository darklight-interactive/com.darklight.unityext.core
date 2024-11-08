using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Darklight.UnityExt.Collection
{
    /// <summary>
    /// Provides a thread-safe implementation of CollectionLibrary with secure hashing capabilities.
    /// </summary>
    /// <typeparam name="TValue">The type of values stored in the collection.</typeparam>
    [Serializable]
    public class CollectionHash<TValue> : CollectionLibrary<TValue>
    {
        private readonly ConcurrentDictionary<int, string> _hashCache;
        private readonly ReaderWriterLockSlim _hashLock;
        private volatile string _collectionHash;
        private readonly object _computeLock = new object();
        private bool _isInitialized;

        /// <summary>
        /// Initializes a new instance of the CollectionHash class.
        /// </summary>
        public CollectionHash() : base()
        {
            _hashCache = new ConcurrentDictionary<int, string>();
            _hashLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _collectionHash = string.Empty;
            _isInitialized = true;
        }

        /// <summary>
        /// Computes a secure hash for an item using SHA256.
        /// </summary>
        /// <param name="item">The item to hash.</param>
        /// <returns>A hex string representation of the hash.</returns>
        protected virtual string ComputeItemHash(CollectionItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            lock (_computeLock)
            {
                using (var sha256 = SHA256.Create())
                {
                    // Thread-safe value string generation
                    string valueString = GenerateValueString(item);
                    var itemData = Encoding.UTF8.GetBytes($"{item.Id}:{valueString}");
                    var hashBytes = sha256.ComputeHash(itemData);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        /// <summary>
        /// Generates a thread-safe string representation of the item's value.
        /// </summary>
        private string GenerateValueString(CollectionItem item)
        {
            if (item.Object is UnityEngine.Object)
            {
                return $"UnityObject_{item.Id}";
            }

            try
            {
                return item.Object?.ToString() ?? "null";
            }
            catch (Exception)
            {
                // Fallback for any thread-safety issues with ToString()
                return $"Object_{item.Id}";
            }
        }

        /// <summary>
        /// Gets the current hash of the entire collection.
        /// </summary>
        public string GetCollectionHash()
        {
            if (!_isInitialized) return string.Empty;
            
            if (!_hashLock.TryEnterReadLock(TimeSpan.FromSeconds(1)))
                return _collectionHash;

            try
            {
                return _collectionHash;
            }
            finally
            {
                _hashLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Updates the collection hash when items change.
        /// </summary>
        protected virtual void UpdateCollectionHash()
        {
            if (!_isInitialized) return;
            
            if (!_hashLock.TryEnterWriteLock(TimeSpan.FromSeconds(1)))
                return;

            try
            {
                List<CollectionItem> snapshot;
                lock (_computeLock)
                {
                    if (!Items.Any())
                    {
                        _collectionHash = string.Empty;
                        return;
                    }

                    // Create a thread-safe copy of the items
                    try
                    {
                        snapshot = new List<CollectionItem>(Items.Count());
                        foreach (var item in Items)
                        {
                            if (item != null)
                            {
                                snapshot.Add(item);
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Collection was modified during copy, retry with a new list
                        snapshot = Items.ToList();
                    }
                }

                using (var sha256 = SHA256.Create())
                {
                    var orderedHashes = new List<string>(snapshot.Count);
                    
                    foreach (var item in snapshot)
                    {
                        if (item != null)
                        {
                            orderedHashes.Add(_hashCache.GetOrAdd(item.Id, _ => ComputeItemHash(item)));
                        }
                    }

                    orderedHashes.Sort(); // Sort in place instead of using OrderBy

                    var combinedHash = string.Join("", orderedHashes);
                    var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedHash));
                    _collectionHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (Exception)
            {
                // If anything goes wrong, set empty hash and retry on next update
                _collectionHash = string.Empty;
            }
            finally
            {
                _hashLock.ExitWriteLock();
            }
        }
        public bool VerifyItemIntegrity(CollectionItem item)
        {
            if (!_isInitialized) return false;
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (!_hashCache.TryGetValue(item.Id, out var cachedHash))
                return false;

            var currentHash = ComputeItemHash(item);
            return string.Equals(cachedHash, currentHash, StringComparison.OrdinalIgnoreCase);
        }


        public string GetItemHash(int id)
        {
            if (!_isInitialized) return null;
            return _hashCache.TryGetValue(id, out var hash) ? hash : null;
        }

        public bool VerifyCollectionIntegrity()
        {
            if (!_isInitialized) return false;
            var snapshot = Items.ToList(); // Create a snapshot for thread-safe enumeration
            return snapshot.All(VerifyItemIntegrity);
        }

        protected override void CollectionChanged(CollectionEventArgs args)
        {
            if (!_isInitialized) return;
            base.CollectionChanged(args);
            UpdateCollectionHash(); // Ensure hash is updated after collection changes
        }
    }
} 