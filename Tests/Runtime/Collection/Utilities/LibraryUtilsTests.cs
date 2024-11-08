using System;
using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Collection;
using NUnit.Framework;
using UnityEngine;

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
            var keys = CollectionUtils.GetAllPossibleKeys<TestEnum>().ToList();

            // Assert
            Assert.That(keys.Count, Is.EqualTo(3));
            Assert.That(keys, Contains.Item(TestEnum.One));
            Assert.That(keys, Contains.Item(TestEnum.Two));
            Assert.That(keys, Contains.Item(TestEnum.Three));
        }
    }
}
