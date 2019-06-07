using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Authoring;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Editor.Extensions;
using Unity.Editor.Persistence;
using Unity.Entities;
using Unity.Properties;
using Unity.Tiny.Scenes;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Editor.Assets
{
    internal interface IAssetEnumerator
    {
        AssetInfo GetAssetInfo(UnityEngine.Object obj);
    }

    internal class AssetEnumerator : IAssetEnumerator
    {
        private readonly Dictionary<Type, IUnityObjectAssetEnumerator> m_AssetEnumerators = new Dictionary<Type, IUnityObjectAssetEnumerator>();
        private readonly Dictionary<UnityEngine.Object, AssetInfo> m_Assets = new Dictionary<UnityEngine.Object, AssetInfo>();

        private IWorldManager WorldManager { get; }
        private IPersistenceManager PersistenceManager { get; }

        internal AssetEnumerator(Session session)
        {
            WorldManager = session.GetManager<IWorldManager>();
            Assert.IsNotNull(WorldManager);

            PersistenceManager = session.GetManager<IPersistenceManager>();
            Assert.IsNotNull(PersistenceManager);

            foreach (var pair in DomainCache.UnityObjectAssetTypes)
            {
                m_AssetEnumerators[pair.Key] = (IUnityObjectAssetEnumerator)Activator.CreateInstance(pair.Value);
            }
        }

        internal static IReadOnlyCollection<AssetInfo> GetAllReferencedAssets(Project project)
        {
            return new AssetEnumerator(project.Session).EnumerateReferencedAssets(project);
        }

        private IReadOnlyCollection<AssetInfo> EnumerateReferencedAssets(Project project)
        {
            var collector = new HashSet<AssetInfo>();
            var allReferencedAssets = GetAssetInfoFromNonLoadedScenes(project)
                                      .Concat(GetAssetsFromCurrentlyLoadedEntities());

            foreach (var asset in allReferencedAssets)
            {
                collector.Add(asset);
                CollectAssetParents(asset, collector);
                CollectAssetChildren(asset, collector);
            }

            return collector;
        }

        private IEnumerable<AssetInfo> GetAssetInfoFromNonLoadedScenes(Project project)
        {
            foreach (var sceneReference in project.GetScenes())
            {
                var sceneManager = project.Session.GetManager<IEditorSceneManagerInternal>();

                // ignoring the currently loaded scene to avoid reading stale data from scene file
                if (sceneManager.IsAnyInstanceOfSceneLoaded(sceneReference.SceneGuid))
                {
                    continue;
                }

                foreach (var assetReference in PersistenceManager.GetAssetReferencesWithoutLoadingThem(sceneReference.SceneGuid))
                {
                    var assetInfo = GetAssetInfo(assetReference);
                    if (assetInfo != null)
                    {
                        yield return assetInfo;
                    }
                }
            }
        }

        private IEnumerable<AssetInfo> GetAssetsFromCurrentlyLoadedEntities()
        {
            var queryDesc = new EntityQueryDesc
            {
                None = new[] { ComponentType.ReadOnly<AssetReference>() },
                Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled
            };

            using (var query = WorldManager.EntityManager.CreateEntityQuery(queryDesc))
            using (var entities = query.ToEntityArray(Allocator.TempJob))
            {
                var assetReferenceVisitor = new AssetReferenceVisitor(WorldManager.EntityManager);

                foreach (var entity in entities)
                {
                    PropertyContainer.Visit(new EntityContainer(WorldManager.EntityManager, entity), assetReferenceVisitor);
                }

                foreach (var assetReference in assetReferenceVisitor.AssetReferences)
                {
                    var assetInfo = GetAssetInfo(assetReference);
                    if (assetInfo != null)
                    {
                        yield return assetInfo;
                    }

                }
            }
        }

        private void CollectAssetParents(AssetInfo assetInfo, HashSet<AssetInfo> collector)
        {
            var parent = assetInfo.Parent;
            while (parent != null)
            {
                collector.Add(parent);
                parent = parent.Parent;
            }
        }

        private void CollectAssetChildren(AssetInfo assetInfo, HashSet<AssetInfo> collector)
        {
            foreach(var children in assetInfo.Children)
            {
                collector.Add(children);
                CollectAssetChildren(children, collector);
            }
        }

        #region IAssetEnumerator

        public AssetInfo GetAssetInfo(UnityEngine.Object obj)
        {
            if (obj == null || !obj)
            {
                return null;
            }

            if (m_Assets.TryGetValue(obj, out var assetInfo))
            {
                return assetInfo;
            }

            IUnityObjectAssetEnumerator assetEnumerator = null;
            var assetEnumerators = m_AssetEnumerators.Where(x => x.Key.IsInstanceOfType(obj));
            if (assetEnumerators.Count() > 1)
            {
                assetEnumerator = assetEnumerators.Aggregate((c, n) => c.Key.IsAssignableFrom(n.Key) ? c : n).Value;
            }
            else
            {
                assetEnumerator = assetEnumerators.FirstOrDefault().Value;
            }

            if (assetEnumerator != null)
            {
                assetInfo = assetEnumerator.GetAssetInfo(this, obj);
            }

            m_Assets[obj] = assetInfo;

            if (assetInfo != null && !UnityEditor.AssetDatabase.IsSubAsset(obj))
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path))
                {
                    var subAssets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
                    foreach (var subAsset in subAssets)
                    {
                        var subAssetInfo = GetAssetInfo(subAsset);
                        if (subAssetInfo != null)
                        {
                            subAssetInfo.Parent = assetInfo;
                        }
                    }
                }
            }

            return assetInfo;
        }

        #endregion
    }

    internal interface IUnityObjectAssetEnumerator
    {
        AssetInfo GetAssetInfo(IAssetEnumerator ctx, UnityEngine.Object obj);
    }

    internal abstract class UnityObjectAsset<T> : IUnityObjectAssetEnumerator
        where T : UnityEngine.Object
    {
        public AssetInfo GetAssetInfo(IAssetEnumerator ctx, UnityEngine.Object obj)
        {
            return GetAssetInfo(ctx, obj as T);
        }

        public virtual AssetInfo GetAssetInfo(IAssetEnumerator ctx, T obj)
        {
            return new AssetInfo(obj, obj.name);
        }
    }
}
