using System.Linq;
using Darklight.Collection;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests.Collection
{
    public class CollectionLibraryTests : LibraryTestBase
    {
        private CollectionLibrary<GameObject> _library;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _library = new CollectionLibrary<GameObject>();
        }

        [Test]
        public void Add_ValidItem_AddsSuccessfully()
        {
            // Arrange
            var item = GetTestItem(0);

            // Act
            _library.Add(item);

            // Assert
            AssertCollectionContains(_library.Items, item);
            AssertCollectionCount(_library.Items, 1);
        }

        [Test]
        public void Remove_ExistingItem_RemovesSuccessfully()
        {
            // Arrange
            var item = GetTestItem(0);
            _library.Add(item);

            // Act
            bool result = _library.Remove(item);

            // Assert
            Assert.That(result, Is.True);
            AssertCollectionIsEmpty(_library.Items);
        }

        [Test]
        public void TryGetItem_ExistingId_ReturnsTrue()
        {
            // Arrange
            var item = GetTestItem(0);
            _library.Add(item);

            // Act
            bool result = _library.TryGetItem(0, out CollectionItem retrievedItem);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(retrievedItem, Is.EqualTo(item));
        }

        [Test]
        public void GetValueById_ExistingId_ReturnsCorrectValue()
        {
            // Arrange
            var item = GetTestItem(0);
            _library.Add(item);

            // Act
            var value = _library.GetItemById(0);

            // Assert
            Assert.That(value, Is.EqualTo(GetTestObject(0)));
        }

        [Test]
        public void Clear_RemovesAllItems()
        {
            // Arrange
            var items = GetTestItems(0, 10);
            _library.AddRange(items);

            // Act
            _library.Clear();

            // Assert
            AssertCollectionIsEmpty(_library.Items);
        }

        [Test]
        public void AddRange_MultipleItems_AddsAllSuccessfully()
        {
            // Arrange
            var items = GetTestItems(0, 50);

            // Act
            _library.AddRange(items);

            // Assert
            AssertCollectionCount(_library.Items, 50);
            AssertRangeProperties(_library.Items, 0, 50);
        }

        [Test]
        public void Events_ItemAddedAndRemoved_FiresCorrectly()
        {
            // Arrange
            int addedCount = 0;
            int removedCount = 0;
            var item = GetTestItem(0);

            _library.OnCollectionChanged += (sender, args) =>
            {
                if (args.EventType == CollectionEventType.ADD)
                    addedCount++;
                else if (args.EventType == CollectionEventType.REMOVE)
                    removedCount++;
            };

            // Act
            _library.Add(item);
            _library.Remove(item);

            // Assert
            Assert.That(addedCount, Is.EqualTo(1));
            Assert.That(removedCount, Is.EqualTo(1));
        }

        [Test]
        public void Where_FilterItems_ReturnsFilteredCollection()
        {
            // Arrange
            var items = GetTestItems(0, 100);
            _library.AddRange(items);

            // Act
            var filtered = _library.Where(item => item.Id >= 50);

            // Assert
            AssertCollectionCount(filtered, 50);
            AssertRangeProperties(filtered, 50, 50);
        }

        [Test]
        public void Clone_CreatesExactCopy()
        {
            // Arrange
            var items = GetTestItems(0, 25);
            _library.AddRange(items);

            // Act
            var clone = _library.Clone();

            // Assert
            AssertCollectionCount(clone.Items, 25);
            AssertCollectionEquals(clone.Items, _library.Items);
            Assert.That(clone.GetHashCode(), Is.EqualTo(_library.GetHashCode()));
        }

        [Test]
        public void RemoveWhere_RemovesMatchingItems()
        {
            // Arrange
            var items = GetTestItems(0, 100);
            _library.AddRange(items);

            // Act
            _library.RemoveWhere(item => item.Id >= 50);

            // Assert
            AssertCollectionCount(_library.Items, 50);
            AssertRangeProperties(_library.Items, 0, 50);
        }

        [Test]
        public void GetItemsInRange_ReturnsCorrectItems()
        {
            // Arrange
            var items = GetTestItems(0, 100);
            _library.AddRange(items);

            // Act
            var rangeItems = _library.GetItemsInRange(25, 74);

            // Assert
            AssertCollectionCount(rangeItems, 50);
            AssertRangeProperties(rangeItems, 25, 50);
        }

        [Test]
        public void BatchOperations_HandlesLargeDataSets()
        {
            // Arrange
            var items = GetTestItems(0, 100);

            // Act & Assert - Add
            _library.AddRange(items);
            AssertCollectionCount(_library.Items, 100);

            // Act & Assert - Filter
            var filtered = _library.Where(item => item.Id % 2 == 0);
            AssertCollectionCount(filtered, 50);

            // Act & Assert - Remove
            _library.RemoveWhere(item => item.Id % 3 == 0);
            AssertCollectionCount(_library.Items, 66); // Corrected: 100 - 34 items divisible by 3
        }

        [Test]
        public void Collection_MaintainsOrder()
        {
            // Arrange
            var items = GetTestItems(0, 10);
            _library.AddRange(items);

            // Assert
            var orderedIds = _library.Items.Select(item => item.Id);
            AssertCollectionOrdered(orderedIds);
        }

        [Test]
        public void Collection_MaintainsUniqueness()
        {
            // Arrange
            var items = GetTestItems(0, 50);
            _library.AddRange(items);

            // Assert
            AssertCollectionUnique(_library.Items);
            AssertCollectionUnique(_library.IDs);
        }
    }
}
