using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Editor.Extensions;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Scenes;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Editor
{
    internal static class ConfigurationUtility
    {
        /// <summary>
        /// Retrieve all the scenes referenced by the configuration entity.
        /// </summary>
        /// <param name="entityManager"><see cref="EntityManager"/> of the config entity.</param>
        /// <param name="configEntity">The config entity.</param>
        /// <param name="allocator">The allocator used to hold the result.</param>
        /// <returns>Array of scenes referenced in the configuration.</returns>
        public static NativeArray<SceneReference> GetScenes(EntityManager entityManager, Entity configEntity, Allocator allocator = Allocator.Temp)
        {
            Assert.AreNotEqual(Entity.Null, configEntity);
            Assert.IsTrue(entityManager.HasComponent<ConfigurationTag>(configEntity));
            Assert.IsTrue(entityManager.HasComponent<Scenes>(configEntity));
            return entityManager.GetBufferRO<Scenes>(configEntity).Reinterpret<SceneReference>().ToNativeArray(allocator);
        }

        /// <summary>
        /// Add a scene in configuration entity scene references.
        /// </summary>
        /// <param name="entityManager"><see cref="EntityManager"/> of the config entity.</param>
        /// <param name="configEntity">The config entity.</param>
        /// <param name="sceneReference">The scene reference to add.</param>
        public static void AddScene(EntityManager entityManager, Entity configEntity, SceneReference sceneReference)
        {
            Assert.AreNotEqual(Entity.Null, configEntity);
            Assert.IsTrue(entityManager.HasComponent<ConfigurationTag>(configEntity));
            Assert.IsTrue(entityManager.HasComponent<Scenes>(configEntity));
            var scenes = entityManager.GetBuffer<Scenes>(configEntity).Reinterpret<SceneReference>();
            if (!scenes.Contains(sceneReference))
            {
                scenes.Add(sceneReference);
            }
        }

        /// <summary>
        /// Remove a scene in configuration entity scene references.
        /// The scene will be removed from both <see cref="Scenes"/> and <see cref="StartupScenes"/> lists.
        /// </summary>
        /// <param name="entityManager"><see cref="EntityManager"/> of the config entity.</param>
        /// <param name="configEntity">The config entity.</param>
        /// <param name="sceneReference">The scene reference to remove.</param>
        public static void RemoveScene(EntityManager entityManager, Entity configEntity, SceneReference sceneReference)
        {
            Assert.AreNotEqual(Entity.Null, configEntity);
            Assert.IsTrue(entityManager.HasComponent<ConfigurationTag>(configEntity));
            Assert.IsTrue(entityManager.HasComponent<Scenes>(configEntity));
            entityManager.GetBuffer<Scenes>(configEntity).Reinterpret<SceneReference>().Remove(sceneReference);
            Assert.IsTrue(entityManager.HasComponent<StartupScenes>(configEntity));
            entityManager.GetBuffer<StartupScenes>(configEntity).Reinterpret<SceneReference>().Remove(sceneReference);
        }

        /// <summary>
        /// Retrieve all the startup scenes referenced by the configuration entity.
        /// </summary>
        /// <param name="entityManager"><see cref="EntityManager"/> of the config entity.</param>
        /// <param name="configEntity">The config entity.</param>
        /// <param name="allocator">The allocator used to hold the result.</param>
        /// <returns>Array of startup scenes referenced in the configuration.</returns>
        public static NativeArray<SceneReference> GetStartupScenes(EntityManager entityManager, Entity configEntity, Allocator allocator = Allocator.Temp)
        {
            Assert.AreNotEqual(Entity.Null, configEntity);
            Assert.IsTrue(entityManager.HasComponent<ConfigurationTag>(configEntity));
            Assert.IsTrue(entityManager.HasComponent<StartupScenes>(configEntity));
            return entityManager.GetBufferRO<StartupScenes>(configEntity).Reinterpret<SceneReference>().ToNativeArray(allocator);
        }

        /// <summary>
        /// Add a startup scene in configuration entity scene references.
        /// The scene must be in the <see cref="Scenes"/> list before it can be added to <see cref="StartupScenes"/> list.
        /// </summary>
        /// <param name="entityManager"><see cref="EntityManager"/> of the config entity.</param>
        /// <param name="configEntity">The config entity.</param>
        /// <param name="sceneReference">The startup scene reference to add.</param>
        public static void AddStartupScene(EntityManager entityManager, Entity configEntity, SceneReference sceneReference)
        {
            Assert.AreNotEqual(Entity.Null, configEntity);
            Assert.IsTrue(entityManager.HasComponent<ConfigurationTag>(configEntity));
            Assert.IsTrue(entityManager.HasComponent<Scenes>(configEntity));
            Assert.IsTrue(entityManager.GetBuffer<Scenes>(configEntity).Reinterpret<SceneReference>().Contains(sceneReference));
            Assert.IsTrue(entityManager.HasComponent<StartupScenes>(configEntity));
            var startupScenes = entityManager.GetBuffer<StartupScenes>(configEntity).Reinterpret<SceneReference>();
            if (!startupScenes.Contains(sceneReference))
            {
                startupScenes.Add(sceneReference);
            }
        }

        /// <summary>
        /// Remove a startup scene in configuration entity scene references.
        /// </summary>
        /// <param name="entityManager"><see cref="EntityManager"/> of the config entity.</param>
        /// <param name="configEntity">The config entity.</param>
        /// <param name="sceneReference">The scene reference to remove.</param>
        public static void RemoveStartupScene(EntityManager entityManager, Entity configEntity, SceneReference sceneReference)
        {
            Assert.AreNotEqual(Entity.Null, configEntity);
            Assert.IsTrue(entityManager.HasComponent<ConfigurationTag>(configEntity));
            Assert.IsTrue(entityManager.HasComponent<StartupScenes>(configEntity));
            entityManager.GetBuffer<StartupScenes>(configEntity).Reinterpret<SceneReference>().Remove(sceneReference);
        }
    }
}
