using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Collection;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests.Collection
{
    public class CollectionHashTests : LibraryTestBase
    {
        private CollectionHash<GameObject> _hashLibrary;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _hashLibrary = new CollectionHash<GameObject>();
        }

        [Test]
        public void Add_Item_UpdatesCollectionHash()
        {
            // Arrange
            var item = GetTestItem(0);
            var initialHash = _hashLibrary.GetCollectionHash();

            // Act
            _hashLibrary.Add(item);
            var newHash = _hashLibrary.GetCollectionHash();

            // Assert
            Assert.That(newHash, Is.Not.EqualTo(initialHash));
            Assert.That(newHash, Is.Not.Empty);
        }

        [Test]
        public void Remove_Item_UpdatesCollectionHash()
        {
            // Arrange
            var item = GetTestItem(0);
            _hashLibrary.Add(item);
            var hashWithItem = _hashLibrary.GetCollectionHash();

            // Act
            bool removed = _hashLibrary.Remove(item);
            var hashAfterRemoval = _hashLibrary.GetCollectionHash();

            // Assert
            Assert.That(removed, Is.True);
            Assert.That(hashAfterRemoval, Is.Not.EqualTo(hashWithItem));
            Assert.That(hashAfterRemoval, Is.EqualTo(string.Empty));
        }

        [Test]
        public void VerifyItemIntegrity_ValidItem_ReturnsTrue()
        {
            // Arrange
            var item = GetTestItem(0);
            _hashLibrary.Add(item);

            // Act
            bool isValid = _hashLibrary.VerifyItemIntegrity((CollectionItem<GameObject>)item);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void VerifyCollectionIntegrity_ValidCollection_ReturnsTrue()
        {
            // Arrange
            var items = GetTestItems(0, 10);
            _hashLibrary.AddRange(items);

            // Act
            bool isValid = _hashLibrary.VerifyCollectionIntegrity();

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void GetItemHash_ExistingItem_ReturnsValidHash()
        {
            // Arrange
            var item = GetTestItem(0);
            _hashLibrary.Add(item);

            // Act
            string hash = _hashLibrary.GetItemHash(item.Id);

            // Assert
            Assert.That(hash, Is.Not.Null);
            Assert.That(hash, Is.Not.Empty);
        }

        [Test]
        public void Clear_RemovesAllHashesAndItems()
        {
            // Arrange
            var items = GetTestItems(0, 5);
            _hashLibrary.AddRange(items);

            // Act
            _hashLibrary.Clear();

            // Assert
            Assert.That(_hashLibrary.GetCollectionHash(), Is.EqualTo(string.Empty));
            AssertCollectionIsEmpty(_hashLibrary.Items);
        }

        [Test]
        public void MultipleOperations_MaintainsHashIntegrity()
        {
            // Arrange
            var items = GetTestItems(0, 5).ToList();
            _hashLibrary.AddRange(items);
            var initialHash = _hashLibrary.GetCollectionHash();

            // Act
            _hashLibrary.Remove(items[0]);
            var hashAfterRemove = _hashLibrary.GetCollectionHash();

            _hashLibrary.Add(items[0]);
            var hashAfterReadding = _hashLibrary.GetCollectionHash();

            // Assert
            Assert.That(hashAfterRemove, Is.Not.EqualTo(initialHash));
            Assert.That(hashAfterReadding, Is.EqualTo(initialHash));
        }
    }
}
