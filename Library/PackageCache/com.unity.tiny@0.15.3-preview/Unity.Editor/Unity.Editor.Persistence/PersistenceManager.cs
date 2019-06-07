using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Unity.Authoring;
using Unity.Authoring.ChangeTracking;
using Unity.Authoring.Core;
using Unity.Editor.Extensions;
using Unity.Editor.Serialization;
using Unity.Entities;
using Unity.Entities.Reflection;
using Unity.Serialization;
using Unity.Serialization.Json;
using Unity.Tiny.Scenes;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Unity.Editor.Persistence
{
    internal interface IPersistenceManager : ISessionManager
    {
        void SaveScene(EntityManager entityManager, Scene scene, string path);
        Scene LoadScene(EntityManager entityManager, string path, bool removeRemapInfo = true);
        Scene LoadScene(EntityManager entityManager, Guid sceneGuid, bool removeRemapInfo = true);

        void LoadSceneAssetsOnly(string path);
        IEnumerable<Object> GetAssetReferencesWithoutLoadingThem(Guid sceneGuid);

        IEnumerable<string> FindAllAssetGuid<T>() where T : Object;
        IEnumerable<string> FindAllAssetGuid(Type type);

        string GetSceneAssetName(Scene scene);
        string GetSceneAssetName(Guid sceneGuid);
        string GetSceneAssetPath(Scene scene);
        string GetSceneAssetPath(Guid sceneGuid);
    }

    [UsedImplicitly]
    internal class PersistenceManager : SessionManager, IPersistenceManager
    {
        private const string AssetReferencesPropertyName = "AssetReferences";

        private readonly Dictionary<string, SceneGuid> m_AssetGuidToSceneGuid = new Dictionary<string, SceneGuid>();
        private readonly Dictionary<SceneGuid, string> m_SceneGuidToAssetGuid = new Dictionary<SceneGuid, string>();

        public PersistenceManager(Session session) : base(session)
        {
        }

        public override void Load()
        {
            foreach (var guid in FindAllAssetGuid<SceneAsset>())
            {
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(guid));
                var sceneGuid = new SceneGuid { Guid = new Guid(asset.Guid) };
                m_AssetGuidToSceneGuid.Add(guid, sceneGuid);
                m_SceneGuidToAssetGuid.Add(sceneGuid, guid);
            }

            AssetPostprocessorCallbacks.RegisterAssetImportedHandlerForType<SceneAsset>(HandleImportSceneAsset);
            AssetPostprocessorCallbacks.RegisterAssetDeletedHandler(HandleDeleteAsset);
        }

        public override void Unload()
        {
            AssetPostprocessorCallbacks.UnregisterAssetImportedHandlerForType<SceneAsset>(HandleImportSceneAsset);
            AssetPostprocessorCallbacks.UnregisterAssetDeletedHandler(HandleDeleteAsset);
        }

        private void HandleImportSceneAsset(SceneAsset asset, PostprocessEventArgs args)
        {
            var assetGuid = args.AssetGuid;
            var sceneGuid = new SceneGuid { Guid = new Guid(asset.Guid) };

            m_AssetGuidToSceneGuid.TryAdd(assetGuid, sceneGuid);
            m_SceneGuidToAssetGuid.TryAdd(sceneGuid, assetGuid);
        }

        private void HandleDeleteAsset(PostprocessEventArgs args)
        {
            var assetGuid = args.AssetGuid;

            // Handle deleting scene
            if (m_AssetGuidToSceneGuid.TryGetValue(assetGuid, out var sceneGuid))
            {
                m_AssetGuidToSceneGuid.Remove(assetGuid);
                m_SceneGuidToAssetGuid.Remove(sceneGuid);

                // Remove scene from project
                var wm = Session.GetManager<IWorldManager>();
                Project.RemoveScene(wm.EntityManager, wm.GetConfigEntity(), new SceneReference { SceneGuid = sceneGuid.Guid });

                // Called last as this will invoke OnScene unload listeners so we need to ensure 
                // we've updated all state before they are called
                Session.GetManager<IEditorSceneManagerInternal>().UnloadAllScenesByGuid(sceneGuid);
            }

            // Handle deleting project
            var project = Project.Projects.FirstOrDefault(p => p.Guid == new Guid(args.AssetGuid));
            if (project != null)
            {
                if (project == Application.AuthoringProject)
                {
                    EditorApplication.delayCall += () => { Application.SetAuthoringProject(null); };
                }
                else
                {
                    project.Dispose();
                }
            }
        }

        public void SaveScene(EntityManager entityManager, Scene scene, string path)
        {
            SaveToFile(entityManager, scene, path);

            using (Session.GetManager<IEditorSceneManagerInternal>().IgnoreSceneImport())
            {
                AssetDatabase.ImportAsset(AssetDatabaseUtility.GetPathRelativeToProjectPath(path), ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUncompressedImport);
            }

            var assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabaseUtility.GetPathRelativeToProjectPath(path));

            if (string.IsNullOrEmpty(assetGuid))
            {
                throw new Exception("Failed to save!");
            }
        }

        public Scene LoadScene(EntityManager entityManager, string path, bool removeRemapInfo)
        {
            var assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabaseUtility.GetPathRelativeToProjectPath(path));
            var world = new World(assetGuid);
            try
            {
                var scene = LoadFromFile(world.EntityManager, path, Session.GetManager<IAssetManager>());

                entityManager.MoveEntitiesFrom(world.EntityManager);
                entityManager.World.GetOrCreateSystem<EntityReferenceRemapSystem>().Update();
                if (removeRemapInfo)
                {
                    entityManager.World.GetOrCreateSystem<RemoveRemapInformationSystem>().Update();
                }

                return scene;
            }
            finally
            {
                world.Dispose();
            }
        }

        public Scene LoadScene(EntityManager entityManager, Guid sceneGuid, bool removeRemapInfo)
        {
            var assetPath = GetSceneAssetPath(sceneGuid);
            if (!string.IsNullOrEmpty(assetPath))
            {
                return LoadScene(entityManager, assetPath, removeRemapInfo);
            }
            return Scene.Null;
        }

        public void LoadSceneAssetsOnly(string path)
        {
            using (var reader = ReadSceneFile(path, out var sceneHeader))
            {
                if (!sceneHeader.TryGetValue(AssetReferencesPropertyName, out var assetReferences))
                    return;

                LoadAssetReferences(assetReferences.AsArrayView(), Session.GetManager<IAssetManager>());
            }
        }

        public IEnumerable<Object> GetAssetReferencesWithoutLoadingThem(Guid sceneGuid)
        {
            var scenePath = GetSceneAssetPath(sceneGuid);
            if (string.IsNullOrEmpty(scenePath))
            {
                return Array.Empty<Object>();
            }

            using (var reader = ReadSceneFile(scenePath, out var headerInformations))
            {
                if (!headerInformations.TryGetValue(AssetReferencesPropertyName, out var assetReferences))
                {
                    return Array.Empty<Object>();
                }

                return GetAssetReferencesFromArrayView(assetReferences.AsArrayView()).Select(x => x.ToUnityObject()).ToArray();
            }
        }

        public static void SaveToFile(EntityManager entityManager, Scene scene, string path)
        {
            File.WriteAllText(path, JsonSerialization.Serialize(new SceneContainer(entityManager, scene), new SceneVisitor(entityManager)));
        }

        public IEnumerable<string> FindAllAssetGuid<T>()
            where T : Object
            => FindAllAssetGuid(typeof(T));

        public IEnumerable<string> FindAllAssetGuid(Type type)
        {
            return AssetDatabase.FindAssets($"t:{type.FullName}");
        }

        public string GetSceneAssetName(Guid sceneGuid)
        {
            if (!m_SceneGuidToAssetGuid.TryGetValue(new SceneGuid { Guid = sceneGuid }, out var assetGuid))
            {
                return null;
            }

            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);

            return !asset ? null : asset.name;
        }

        public string GetSceneAssetName(Scene scene)
        {
            return GetSceneAssetName(scene.SceneGuid.Guid);
        }

        public string GetSceneAssetPath(Guid sceneGuid)
        {
            if (!m_SceneGuidToAssetGuid.TryGetValue(new SceneGuid { Guid = sceneGuid }, out var assetGuid))
            {
                return null;
            }

            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            return assetPath;
        }

        public string GetSceneAssetPath(Scene scene)
        {
            return GetSceneAssetPath(scene.SceneGuid.Guid);
        }

        private static SerializedObjectReader ReadSceneFile(string path, out SerializedObjectView headerInformations)
        {
            var config = new SerializedObjectReaderConfiguration
            {
                UseReadAsync = false,
                BlockBufferSize = 256 << 10, // 256 KB
                TokenBufferSize = 1024,
                NodeBufferSize = JsonFrontEnd.EntityBatchSize,
                ValidationType = JsonValidationType.Standard,
                OutputBufferSize = 1024 << 10 // 1 MB
            };

            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, config.BlockBufferSize);
            var reader = new SerializedObjectReader(stream, config, leaveInputOpen: false);

            try
            {
                if (reader.Step(out var root) != NodeType.BeginObject)
                {
                    throw new Exception();
                }

                // ASSUMPTION: All members are written BEFORE the blob of entities.
                // Read until we hit "Entities":
                NodeType node;

                while ((node = reader.Step(out var view)) != NodeType.None)
                {
                    if ((node & NodeType.ObjectKey) == 0)
                    {
                        continue;
                    }

                    if (view.AsStringView().Equals("Entities"))
                    {
                        break;
                    }
                }

                // Unpack header information from whats been read so far.
                headerInformations = root.AsObjectView();
                return reader;
            }
            catch
            {
                reader.Dispose();
                throw;
            }
        }

        private static Scene LoadFromFile(EntityManager entityManager, string path, IAssetManager assetManager)
        {
            using (var reader = ReadSceneFile(path, out var sceneHeader))
            {
                var guid = sceneHeader["Guid"].AsStringView().ToString();

                if (sceneHeader.TryGetValue(AssetReferencesPropertyName, out var assetReferences))
                {
                    LoadAssetReferences(assetReferences.AsArrayView(), assetManager);
                }

                JsonFrontEnd.Accept(entityManager, reader);

                var scene = SceneManager.Create(new Guid(guid));

                using (var entities = entityManager.GetAllEntities())
                {
                    for (var i = 0; i < entities.Length; i++)
                    {
                        scene.AddEntityReference(entityManager, entities[i]);
                    }
                }

                return scene;
            }
        }

        private static void LoadAssetReferences(SerializedArrayView assetReferences, IAssetManager assetManager)
        {
            foreach (var assetReference in GetAssetReferencesFromArrayView(assetReferences))
            {
                assetManager.GetEntity(assetReference.ToUnityObject());
            }
        }

        private static IEnumerable<AssetReference> GetAssetReferencesFromArrayView(SerializedArrayView assetReferences)
        {
            foreach (var element in assetReferences)
            {
                var reference = element.AsObjectView();

                if (reference.TryGetValue("Guid", out var guid) &&
                    reference.TryGetValue("FileId", out var fileId) &&
                    reference.TryGetValue("Type", out var type))
                {
                    yield return new AssetReference
                    {
                        Guid = new Guid(guid.AsStringView().ToString()),
                        FileId = fileId.AsInt64(),
                        Type = (int)type.AsInt64()
                    };
                }
            }
        }

        private class SceneVisitor : EntityJsonVisitor
        {
            public SceneVisitor(EntityManager entityManager) : base(entityManager)
            {
                AddAdapter(new UnityObjectAdapter(this));
            }

            public override bool IsExcluded<TProperty, TContainer, TValue>(TProperty property, ref TContainer container)
            {
                return AttributeCache<TValue>.HasAttribute<NonSerializedForPersistenceAttribute>() || base.IsExcluded<TProperty, TContainer, TValue>(property, ref container);
            }
        }
    }
}
