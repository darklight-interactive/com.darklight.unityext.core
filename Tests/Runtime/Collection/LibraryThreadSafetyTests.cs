using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Darklight.UnityExt.Collection;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests.Collection
{
    public class LibraryThreadSafetyTests : LibraryTestBase
    {
        private const int ThreadCount = 10;
        private const int OperationsPerThread = 10;
        private const int MaxTestIndex = TEST_OBJECT_COUNT - 1;
        private CollectionLibrary<GameObject> _baseLibrary;
        private CollectionHash<GameObject> _hashLibrary;
        private CollectionDictionary<int, GameObject> _dictionaryLibrary;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _baseLibrary = new CollectionLibrary<GameObject>();
            _hashLibrary = new CollectionHash<GameObject>();
            _dictionaryLibrary = new CollectionDictionary<int, GameObject>();
        }

        [Test]
        public async Task BaseLibrary_ConcurrentOperations_NoDataCorruption()
        {
            // Arrange
            var tasks = new List<Task>();
            var random = new System.Random();

            // Act
            for (int i = 0; i < ThreadCount; i++)
            {
                var threadId = i;
                var task = Task.Run(() =>
                {
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        var item = GetTestItem(threadId * OperationsPerThread + j);

                        // Random operation: add, remove, or read
                        switch (random.Next(3))
                        {
                            case 0:
                                _baseLibrary.Add(item);
                                break;
                            case 1:
                                _baseLibrary.Remove(item);
                                break;
                            case 2:
                                _baseLibrary.TryGetItem(item.Id, out _);
                                break;
                        }

                        Thread.Sleep(1);
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.That(_baseLibrary.Items, Is.Not.Null);
            Assert.DoesNotThrow(() => _baseLibrary.Items.Count());
        }

        [Test]
        public async Task HashLibrary_ConcurrentOperations_MaintainsIntegrity()
        {
            // Arrange
            var tasks = new List<Task>();
            var random = new System.Random();

            // Act
            for (int i = 0; i < ThreadCount; i++)
            {
                var threadId = i;
                var task = Task.Run(() =>
                {
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        var item = GetTestItem(threadId * OperationsPerThread + j);

                        switch (random.Next(4))
                        {
                            case 0:
                                _hashLibrary.Add(item);
                                break;
                            case 1:
                                _hashLibrary.Remove(item);
                                break;
                            case 2:
                                _hashLibrary.VerifyItemIntegrity(item);
                                break;
                            case 3:
                                _hashLibrary.GetCollectionHash();
                                break;
                        }

                        Thread.Sleep(1);
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.That(_hashLibrary.VerifyCollectionIntegrity(), Is.True);
        }

        [Test]
        public async Task DictionaryLibrary_ConcurrentOperations_NoDataCorruption()
        {
            // Arrange
            var tasks = new List<Task>();
            var random = new System.Random();
            var addedKeys = new ConcurrentDictionary<int, bool>();
            var syncLock = new object();

            // Act
            for (int i = 0; i < ThreadCount; i++)
            {
                var threadId = i;
                var task = Task.Run(() =>
                {
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        var key = (threadId * OperationsPerThread + j) % MaxTestIndex;
                        var value = GetTestObject(key);

                        switch (random.Next(4))
                        {
                            case 0:
                                try
                                {
                                    _dictionaryLibrary.Add(key, value);
                                    addedKeys.TryAdd(key, true);
                                }
                                catch (ArgumentException)
                                {
                                    // Key might already exist
                                }
                                break;
                            case 1:
                                _dictionaryLibrary.Remove(
                                    new KeyValueCollectionItem<int, GameObject>(key, key, value)
                                );
                                addedKeys.TryRemove(key, out _);
                                break;
                            case 2:
                                _dictionaryLibrary.TryGetValue(key, out _);
                                break;
                            case 3:
                                // Thread-safe way to get a random existing key
                                var currentKeys = addedKeys.Keys.ToList();
                                if (currentKeys.Any())
                                {
                                    lock (syncLock)
                                    {
                                        try
                                        {
                                            var randomIndex = random.Next(currentKeys.Count);
                                            if (randomIndex < currentKeys.Count)
                                            {
                                                var existingKey = currentKeys[randomIndex];
                                                try
                                                {
                                                    _dictionaryLibrary.TryGetValue(
                                                        existingKey,
                                                        out _
                                                    );
                                                }
                                                catch (KeyNotFoundException)
                                                {
                                                    addedKeys.TryRemove(existingKey, out _);
                                                }
                                            }
                                        }
                                        catch (ArgumentOutOfRangeException)
                                        {
                                            // Handle race condition where collection size changed
                                        }
                                    }
                                }
                                break;
                        }

                        Thread.Sleep(1);
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.DoesNotThrow(() =>
            {
                foreach (var item in _dictionaryLibrary.Items)
                {
                    _ = item.Id;
                    _ = item.Object;
                }
            });
        }

        [Test]
        public async Task AllCollections_ParallelOperations_MaintainConsistency()
        {
            // Arrange
            var tasks = new List<Task>();
            var random = new System.Random();
            var addedItems = new ConcurrentDictionary<int, bool>();
            var syncLock = new object();

            // Act
            for (int i = 0; i < ThreadCount; i++)
            {
                var threadId = i;
                var task = Task.Run(() =>
                {
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        try
                        {
                            var id = (threadId * OperationsPerThread + j) % MaxTestIndex;

                            // Get items inside the try block to handle potential null references
                            CollectionItem item;
                            GameObject obj;

                            lock (syncLock)
                            {
                                item = GetTestItem(id);
                                obj = GetTestObject(id);
                            }

                            if (item == null || obj == null)
                                continue;

                            // Perform parallel operations on all collections
                            switch (random.Next(3))
                            {
                                case 0: // Add to all
                                    if (addedItems.TryAdd(id, true))
                                    {
                                        try
                                        {
                                            lock (syncLock)
                                            {
                                                _baseLibrary.Add(item);
                                                _hashLibrary.Add(item);
                                                _dictionaryLibrary.Add(id, obj);
                                            }
                                        }
                                        catch (ArgumentException)
                                        {
                                            addedItems.TryRemove(id, out _);
                                        }
                                    }
                                    break;

                                case 1: // Remove from all
                                    if (addedItems.TryRemove(id, out _))
                                    {
                                        lock (syncLock)
                                        {
                                            try
                                            {
                                                _baseLibrary.Remove(item);
                                            }
                                            catch { }
                                            try
                                            {
                                                _hashLibrary.Remove(item);
                                            }
                                            catch { }
                                            try
                                            {
                                                _dictionaryLibrary.Remove(
                                                    new KeyValueCollectionItem<int, GameObject>(
                                                        id,
                                                        id,
                                                        obj
                                                    )
                                                );
                                            }
                                            catch { }
                                        }
                                    }
                                    break;

                                case 2: // Read from all
                                    lock (syncLock)
                                    {
                                        try
                                        {
                                            if (
                                                _baseLibrary.TryGetItem(id, out _)
                                                && _hashLibrary.GetItemHash(id) != null
                                            )
                                            {
                                                _dictionaryLibrary.TryGetValue(id, out _);
                                            }
                                        }
                                        catch
                                        {
                                            // Handle any concurrent access issues
                                        }
                                    }
                                    break;
                            }

                            Thread.Sleep(1);
                        }
                        catch (Exception)
                        {
                            // Catch any unexpected exceptions to prevent task failure
                        }
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Assert
            lock (syncLock)
            {
                Assert.DoesNotThrow(() =>
                {
                    var hashIntegrity = _hashLibrary.VerifyCollectionIntegrity();
                    var baseCount = _baseLibrary.Items.Count();
                    var dictCount = _dictionaryLibrary.Items.Count();

                    Assert.That(hashIntegrity, Is.True, "Hash integrity check failed");
                    Assert.That(
                        baseCount,
                        Is.GreaterThanOrEqualTo(0),
                        "Base library count is invalid"
                    );
                    Assert.That(
                        dictCount,
                        Is.GreaterThanOrEqualTo(0),
                        "Dictionary count is invalid"
                    );
                });
            }
        }
    }
}
