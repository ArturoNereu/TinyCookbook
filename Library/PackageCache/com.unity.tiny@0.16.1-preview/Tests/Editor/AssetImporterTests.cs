using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Authoring;
using Unity.Editor.Assets;
using Unity.Entities;
using Unity.Tiny.Audio;
using Unity.Tiny.Core2D;
using Unity.Tiny.Text;
using Unity.Tiny.Video;

namespace Unity.Editor.Tests
{
    internal class AssetImporterTests
    {
        private static IEnumerable<string> GetAllAssetPaths(Type assetType)
        {
            if (!typeof(UnityEngine.Object).IsAssignableFrom(assetType))
            {
                throw new ArgumentException("Asset type must derive from UnityEngine.Object");
            }
            return UnityEditor.AssetDatabase.FindAssets($"t:{assetType.Name}").Select(UnityEditor.AssetDatabase.GUIDToAssetPath);
        }

        private static IEnumerable<string> GetAllUniqueAssetPaths(Type assetType)
        {
            var assetPaths = GetAllAssetPaths(assetType);
            var uniquePaths = new Dictionary<string, string>();
            foreach (var assetPath in assetPaths)
            {
                var key = assetPath.Split('/').FirstOrDefault() + Path.GetExtension(assetPath);
                if (!uniquePaths.TryGetValue(key, out var value))
                {
                    uniquePaths.Add(key, assetPath);
                }
            }
            if (uniquePaths.Count == 0)
            {
                // In case no assets were found, make sure test case gets at least one
                // param set to null, otherwise we get "No arguments were provided" error.
                return new string[] { null };
            }
            return uniquePaths.Values;
        }

        private void AssertImportAsset<T>(string assetPath, params ComponentType[] componentTypes) where T : UnityEngine.Object
        {
            // Do nothing if TestCaseSource didn't provide any asset to test
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            using (var session = SessionFactory.Create())
            {
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
                Assert.IsTrue(asset != null && asset);

                var assetImporter = new AssetImporter(session);
                Assert.IsTrue(assetImporter.CanImport(asset));

                var entity = assetImporter.Import(asset);
                Assert.AreNotEqual(Entity.Null, entity);

                var worldManager = session.GetManager<IWorldManager>();
                var entityManager = worldManager.EntityManager;
                Assert.IsTrue(entityManager.HasComponent<EntityGuid>(entity));
                Assert.AreNotEqual(Guid.Empty, worldManager.GetEntityGuid(entity));

                foreach (var componentType in componentTypes)
                {
                    Assert.IsTrue(entityManager.HasComponent(entity, componentType));
                }
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TypeManager.Initialize();
        }

        [Test, TestCaseSource(typeof(AssetImporterTests), nameof(GetAllUniqueAssetPaths), new object[] { typeof(UnityEngine.Texture2D) })]
        public void ImportTexture2D(string assetPath)
        {
            AssertImportAsset<UnityEngine.Texture2D>(assetPath, typeof(Image2D), typeof(Image2DLoadFromFile), typeof(Image2DLoadFromFileImageFile));
        }

        [Test, TestCaseSource(typeof(AssetImporterTests), nameof(GetAllUniqueAssetPaths), new object[] { typeof(UnityEngine.Sprite) })]
        public void ImportSprite(string assetPath)
        {
            AssertImportAsset<UnityEngine.Sprite>(assetPath, typeof(Sprite2D));
        }

        [Test, TestCaseSource(typeof(AssetImporterTests), nameof(GetAllUniqueAssetPaths), new object[] { typeof(UnityEngine.U2D.SpriteAtlas) })]
        public void ImportSpriteAtlas(string assetPath)
        {
            AssertImportAsset<UnityEngine.U2D.SpriteAtlas>(assetPath, typeof(SpriteAtlas));
        }

        [Test, TestCaseSource(typeof(AssetImporterTests), nameof(GetAllUniqueAssetPaths), new object[] { typeof(UnityEngine.AudioClip) })]
        public void ImportAudioClip(string assetPath)
        {
            AssertImportAsset<UnityEngine.AudioClip>(assetPath, typeof(AudioClip), typeof(AudioClipLoadFromFile), typeof(AudioClipLoadFromFileAudioFile));
        }

        [Test, TestCaseSource(typeof(AssetImporterTests), nameof(GetAllUniqueAssetPaths), new object[] { typeof(TMPro.TMP_FontAsset) })]
        public void ImportFont(string assetPath)
        {
            AssertImportAsset<TMPro.TMP_FontAsset>(assetPath, typeof(BitmapFont), typeof(CharacterInfoBuffer));
        }

        [Test, TestCaseSource(typeof(AssetImporterTests), nameof(GetAllUniqueAssetPaths), new object[] { typeof(UnityEngine.Video.VideoClip) })]
        public void ImportVideoClip(string assetPath)
        {
            AssertImportAsset<UnityEngine.Video.VideoClip>(assetPath, typeof(VideoClip), typeof(VideoClipLoadFromFile), typeof(VideoClipLoadFromFileName));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TypeManager.Shutdown();
        }
    }
}
