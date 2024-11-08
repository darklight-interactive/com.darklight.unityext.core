using NUnit.Framework;
using UnityEngine;
using Darklight.UnityExt.Collection;
using System;
using System.Linq;

namespace Darklight.Tests.Collection
{
    public class CollectionDictionaryTests : LibraryTestBase
    {
        private CollectionDictionary<string, GameObject> _dictionary;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _dictionary = new CollectionDictionary<string, GameObject>();
        }

        [Test]
        public void Add_KeyValuePair_AddsSuccessfully()
        {
            // Arrange
            string key = "test_key";
            var value = GetTestObject(0);

            // Act
            _dictionary.Add(key, value);

            // Assert
            Assert.That(_dictionary.Count, Is.EqualTo(1));
            Assert.That(_dictionary[key], Is.EqualTo(value));
        }

        [Test]
        public void Add_DuplicateKey_ThrowsException()
        {
            // Arrange
            string key = "test_key";
            var value = GetTestObject(0);
            _dictionary.Add(key, value);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _dictionary.Add(key, GetTestObject(1)));
        }

        [Test]
        public void TryGetValue_ExistingKey_ReturnsTrue()
        {
            // Arrange
            string key = "test_key";
            var value = GetTestObject(0);
            _dictionary.Add(key, value);

            // Act
            bool result = _dictionary.TryGetValue(key, out GameObject retrievedValue);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(retrievedValue, Is.EqualTo(value));
        }

        [Test]
        public void TryGetValue_NonExistingKey_ReturnsFalse()
        {
            // Act
            bool result = _dictionary.TryGetValue("non_existing", out GameObject value);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(value, Is.Null);
        }

        [Test]
        public void Remove_ExistingKey_RemovesSuccessfully()
        {
            // Arrange
            string key = "test_key";
            var value = GetTestObject(0);
            var item = new KeyValueCollectionItem<string, GameObject>(0, key, value);
            _dictionary.Add(key, value);

            // Act
            bool result = _dictionary.Remove(item);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_dictionary.Count, Is.EqualTo(0));
        }

        [Test]
        public void Clear_RemovesAllItems()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                _dictionary.Add($"key_{i}", GetTestObject(i));
            }

            // Act
            _dictionary.Clear();

            // Assert
            Assert.That(_dictionary.Count, Is.EqualTo(0));
        }

        [Test]
        public void Replace_ExistingItem_ReplacesSuccessfully()
        {
            // Arrange
            string key = "test_key";
            var oldValue = GetTestObject(0);
            var newValue = GetTestObject(1);
            var oldItem = new KeyValueCollectionItem<string, GameObject>(0, key, oldValue);
            var newItem = new KeyValueCollectionItem<string, GameObject>(0, key, newValue);
            _dictionary.Add(key, oldValue);

            // Act
            _dictionary.Replace(key, newValue);

            // Assert
            Assert.That(_dictionary[key], Is.EqualTo(newValue));
        }

        [Test]
        public void GetEnumerator_ReturnsAllItems()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                _dictionary.Add($"key_{i}", GetTestObject(i));
            }

            // Act
            var items = _dictionary.Items.ToList();

            // Assert
            Assert.That(items.Count, Is.EqualTo(5));
        }

        [Test]
        public void IndexOf_ExistingItem_ReturnsCorrectIndex()
        {
            // Arrange
            string key = "test_key";
            var value = GetTestObject(0);
            var item = new KeyValueCollectionItem<string, GameObject>(0, key, value);
            _dictionary.Add(key, value);

            // Act
            int index = _dictionary.IndexOf(item);

            // Assert
            Assert.That(index, Is.EqualTo(0));
        }
    }
} 