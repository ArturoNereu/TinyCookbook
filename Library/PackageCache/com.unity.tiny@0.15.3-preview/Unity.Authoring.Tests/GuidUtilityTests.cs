using NUnit.Framework;
using System;
using Unity.Authoring.Hashing;
using Unity.Entities;

namespace Unity.Authoring.Tests
{
    internal class GuidExtensionsTests
    {
        [Test]
        public void NewGuid()
        {
            var guid = GuidUtility.NewGuid("Configuration");
            Assert.AreEqual("46b433b264c69cbd39f04ad2e5d12be8", guid.ToString("N"));
        }

        [Test]
        public void ToEntityGuid()
        {
            var guid = new Guid("fedcba9876543210fedcba9876543210");
            Assert.AreEqual(guid.ToString("N"), guid.ToEntityGuid().ToString());
        }

        [Test]
        public void ToGuid()
        {
            var entityGuid = new EntityGuid { a = 81985529216486895, b = 81985529216486895 };
            Assert.AreEqual(entityGuid.ToString(), entityGuid.ToGuid().ToString("N"));
        }
    }
}
