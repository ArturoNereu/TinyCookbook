using System.Linq;
using NUnit.Framework;
using Unity.Authoring.Tests;
using Unity.Collections;
using Unity.Entities;
using UnityEditor.VersionControl;

namespace Unity.Authoring.ChangeTracking.Tests
{
    [TestFixture]
    internal sealed class WorldChangeTrackerTests
    {
        private World m_World;
        private EntityManager m_EntityManager;
        private WorldChangeTracker m_ChangeTracker;

        [SetUp]
        public void SetUp()
        {
            m_World = new World(nameof(WorldChangeTrackerTests));
            m_EntityManager = m_World.EntityManager;
            m_ChangeTracker = new WorldChangeTracker(m_World, Allocator.TempJob);
        }

        [TearDown]
        public void TearDown()
        {
            m_ChangeTracker.Dispose();
            m_World.Dispose();
        }

        [Test]
        public void WorldChangeTracker_NoChanges()
        {
            Assert.IsFalse(m_ChangeTracker.TryGetChanges(out _));
        }

        [Test]
        public void WorldChangeTracker_CreateEntity()
        {
            m_EntityManager.CreateEntity(typeof(EntityGuid));

            Assert.IsTrue(m_ChangeTracker.TryGetChanges(out var changes));

            try
            {
                Assert.IsTrue(changes.EntitiesWereCreated);
                Assert.AreEqual(1, changes.WorldDiff.NewEntityCount);
                Assert.AreEqual(1, changes.InverseDiff.DeletedEntityCount);
            }
            finally
            {
                changes.Dispose();
            }
        }

        [Test]
        public void WorldChangeTracker_DestroyEntity()
        {
            var entity = m_EntityManager.CreateEntity(typeof(EntityGuid));

            UpdateWithoutChanges();

            m_EntityManager.DestroyEntity(entity);

            Assert.IsTrue(m_ChangeTracker.TryGetChanges(out var changes));

            try
            {
                Assert.IsTrue(changes.EntitiesWereDeleted);
                Assert.AreEqual(1, changes.WorldDiff.DeletedEntityCount);
                Assert.AreEqual(1, changes.InverseDiff.NewEntityCount);
            }
            finally
            {
                changes.Dispose();
            }
        }

        [Test]
        public void WorldChangeTracker_DestroyEntityAndRestore()
        {
            var entity = m_EntityManager.CreateEntity(typeof(EntityGuid));

            UpdateWithoutChanges();

            m_EntityManager.DestroyEntity(entity);

            Assert.IsTrue(m_ChangeTracker.TryGetChanges(out var changes));

            try
            {
                Assert.IsTrue(changes.EntitiesWereDeleted);
                Assert.AreEqual(1, changes.WorldDiff.DeletedEntityCount);
                Assert.AreEqual(1, changes.InverseDiff.NewEntityCount);

                WorldDiffer.ApplyDiff(m_World, changes.InverseDiff);

                using (var entities = m_EntityManager.GetAllEntities(Allocator.Temp))
                {
                    Assert.AreEqual(1, entities.Length);
                }
            }
            finally
            {
                changes.Dispose();
            }
        }

        [Test]
        public void WorldChangeTracker_DuplicateEntityGuid()
        {
            m_EntityManager.CreateEntity(typeof(EntityGuid));
            m_EntityManager.CreateEntity(typeof(EntityGuid));

            Assert.IsTrue(m_ChangeTracker.TryGetChanges(out var changes));

            try
            {
                Assert.AreEqual(2, changes.WorldDiff.NewEntityCount);
                Assert.AreEqual(2, changes.CreatedEntities().Count());
            }
            finally
            {
                changes.Dispose();
            }
        }

        private void UpdateWithoutChanges()
        {
            if (m_ChangeTracker.TryGetChanges(out var changes))
            {
                changes.Dispose();
            }
        }
    }
}
