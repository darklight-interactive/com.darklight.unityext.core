using System.Collections.Generic;
using System.Linq;
using Darklight.Collections;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests.Collection
{
    public abstract class LibraryTestBase
    {
        protected const int TEST_OBJECT_COUNT = 100;
        protected List<GameObject> _testObjects;
        protected Dictionary<int, CollectionItem> _testItems;

        [SetUp]
        public virtual void Setup()
        {
            _testObjects = new List<GameObject>(TEST_OBJECT_COUNT);
            _testItems = new Dictionary<int, CollectionItem>(TEST_OBJECT_COUNT);

            for (int i = 0; i < TEST_OBJECT_COUNT; i++)
            {
                var gameObject = new GameObject($"TestObject_{i}");
                _testObjects.Add(gameObject);
                _testItems.Add(i, new TestCollectionItem(i, gameObject));
            }
        }

        [TearDown]
        public virtual void TearDown()
        {
            foreach (var obj in _testObjects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
            _testObjects.Clear();
            _testItems.Clear();
        }

        protected class TestCollectionItem : CollectionItem
        {
            public int Id { get; }
            public object Value { get; }

            public TestCollectionItem(int id, object value)
                : base(id, value)
            {
                Id = id;
                Value = value;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode() ^ (Value?.GetHashCode() ?? 0);
            }

            public override bool Equals(object obj)
            {
                if (obj is not TestCollectionItem other)
                    return false;

                return Id == other.Id && Equals(Value, other.Value);
            }
        }

        protected IEnumerable<CollectionItem> GetTestItems(int startIndex, int count)
        {
            return _testItems.Values.Skip(startIndex).Take(count);
        }

        protected CollectionItem GetTestItem(int index)
        {
            return _testItems[index];
        }

        protected GameObject GetTestObject(int index)
        {
            return _testObjects[index];
        }

        protected IEnumerable<CollectionItem> GetTestItemsInRange(int startId, int endId)
        {
            return _testItems.Values.Where(item => item.Id >= startId && item.Id <= endId);
        }

        protected void AssertCollectionEquals<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            CollectionAssert.AreEqual(expected, actual);
        }

        protected void AssertCollectionContains<T>(IEnumerable<T> collection, T item)
        {
            CollectionAssert.Contains(collection, item);
        }

        protected void AssertCollectionNotContains<T>(IEnumerable<T> collection, T item)
        {
            CollectionAssert.DoesNotContain(collection, item);
        }

        protected void AssertCollectionIsEmpty<T>(IEnumerable<T> collection)
        {
            CollectionAssert.IsEmpty(collection);
        }

        protected void AssertCollectionCount<T>(IEnumerable<T> collection, int expectedCount)
        {
            Assert.That(collection.Count(), Is.EqualTo(expectedCount));
        }

        protected void AssertCollectionOrdered<T>(IEnumerable<T> collection)
        {
            CollectionAssert.IsOrdered(collection);
        }

        protected void AssertCollectionUnique<T>(IEnumerable<T> collection)
        {
            var distinctCount = collection.Distinct().Count();
            Assert.That(distinctCount, Is.EqualTo(collection.Count()));
        }

        protected void AssertItemProperties(
            CollectionItem item,
            int expectedId,
            GameObject expectedValue
        )
        {
            Assert.That(item.Id, Is.EqualTo(expectedId));
            Assert.That(item.Object, Is.EqualTo(expectedValue));
        }

        protected void AssertRangeProperties(
            IEnumerable<CollectionItem> items,
            int expectedStartId,
            int expectedCount
        )
        {
            var itemsList = items.ToList();
            Assert.That(itemsList.Count, Is.EqualTo(expectedCount));
            Assert.That(itemsList.First().Id, Is.EqualTo(expectedStartId));
            Assert.That(itemsList.Last().Id, Is.EqualTo(expectedStartId + expectedCount - 1));
        }
    }
}
