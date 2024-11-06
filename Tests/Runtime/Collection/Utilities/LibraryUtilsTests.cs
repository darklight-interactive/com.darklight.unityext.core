using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Collection;
using System;
using Darklight.UnityExt.Collection.Utilities;

namespace Darklight.Tests.Collection.Utilities
{
    public class LibraryUtilsTests
    {
        public enum TestEnum
        {
            One,
            Two,
            Three
        }

        [Test]
        public void KeyGenerationUtils_GetAllPossibleKeys_Enum_ReturnsAllEnumValues()
        {
            // Act
            var keys = KeyGenerationUtils.GetAllPossibleKeys<TestEnum>().ToList();

            // Assert
            Assert.That(keys.Count, Is.EqualTo(3));
            Assert.That(keys, Contains.Item(TestEnum.One));
            Assert.That(keys, Contains.Item(TestEnum.Two));
            Assert.That(keys, Contains.Item(TestEnum.Three));
        }

        [Test]
        public void KeyGenerationUtils_GetAllPossibleKeys_Int_ReturnsDefaultRange()
        {
            // Act
            var keys = KeyGenerationUtils.GetAllPossibleKeys<int>().ToList();

            // Assert
            Assert.That(keys.Count, Is.EqualTo(10));
            for (int i = 0; i < 10; i++)
            {
                Assert.That(keys, Contains.Item(i));
            }
        }

        [Test]
        public void LibraryValidation_ValidateUnityObject_NullObject_ThrowsException()
        {
            // Arrange
            GameObject obj = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => LibraryValidation.ValidateUnityObject(obj));
        }

        [Test]
        public void LibraryValidation_ValidateUnityObject_DestroyedObject_ThrowsException()
        {
            // Arrange
            var obj = new GameObject();
            UnityEngine.Object.DestroyImmediate(obj);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => LibraryValidation.ValidateUnityObject(obj));
        }
    }
} 