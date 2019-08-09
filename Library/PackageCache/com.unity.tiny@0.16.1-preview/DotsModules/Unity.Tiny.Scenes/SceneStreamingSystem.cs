using System;
using Unity.Authoring;
using Unity.Authoring.Core;
using Unity.Authoring.Hashing;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Tiny.Codec;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;
using Unity.Tiny.IO;
using static Unity.Tiny.IO.AsyncOp;

namespace Unity.Tiny.Scenes
{
    public enum SceneStatus
    {
        NotYetProcessed = 0,
        Loading,
        Loaded,
        FailedToLoad
    }

    /// <summary>
    /// Provides information for a requested scene. This component will be updated as a load request progresses.
    /// </summary>
    [HideInInspector]
    public struct SceneData : IComponentData
    {
        public Scene Scene;
        public SceneStatus Status;
    }

    [HideInInspector]
    internal struct SceneLoadRequest : IComponentData
    {
        internal AsyncOp SceneOpHandle;
    }

    /// <summary>
    ///  Provides common scene helper functions
    /// </summary>
    public class SceneService
    {
        static internal Entity LoadConfigAsync()
        {
            if (World.Active.TinyEnvironment().configEntity != Entity.Null)
                throw new Exception("Configuration already loaded");

            return LoadSceneAsync(new SceneGuid() { Guid = ConfigurationScene.Guid });
        }

        static internal Entity LoadAssetsAsync()
        {
            return LoadSceneAsync(new SceneGuid() { Guid = AssetsScene.Guid });
        }

        /// <summary>
        /// Creates a request to load the scene provided by the passed `SceneReference` argument. 
        /// </summary>
        /// <param name="sceneReference"></param>
        /// <returns>A new Entity with a `SceneData` component which should be stored for use in `GetSceneStatus`</returns>
        static public Entity LoadSceneAsync(SceneReference sceneReference)
        {
            return LoadSceneAsync(new SceneGuid() { Guid = sceneReference.SceneGuid });
        }

        static internal Entity LoadSceneAsync(SceneGuid sceneGuid)
        {
            var em = World.Active.EntityManager;
            var newScene = SceneManager.Create(sceneGuid.Guid);

            var eScene = em.CreateEntity();
            em.AddComponentData(eScene, new SceneData() { Scene = newScene, Status = SceneStatus.NotYetProcessed });
            em.AddComponentData(eScene, new RequestSceneLoaded());
            em.AddSharedComponentData(eScene, newScene.SceneGuid);
            em.AddSharedComponentData(eScene, newScene.SceneInstanceId);

            return eScene;
        }

        /// <summary>
        /// Unloads the scene instance for the provided entity. As such, the entity passed in must belong 
        /// to a scene otherwise this function will throw.
        /// </summary>
        /// <param name="sceneEntity"></param>
        static public void UnloadSceneInstance(Entity sceneEntity)
        {
            var sceneGuid = World.Active.EntityManager.GetSharedComponentData<SceneGuid>(sceneEntity);
            var sceneInstance = World.Active.EntityManager.GetSharedComponentData<SceneInstanceId>(sceneEntity);
            var scene = new Scene(sceneGuid, sceneInstance);
            World.Active.EntityManager.DestroyEntity(scene.GetSceneEntityQueryRO(World.Active.EntityManager));
        }

        /// <summary>
        /// Unloads all scene instances of the same type as the `SceneReference` passed in.
        /// </summary>
        /// <param name="sceneReference"></param>
        static public void UnloadAllSceneInstances(SceneReference sceneReference)
        {
            UnloadAllSceneInstances(new SceneGuid() { Guid = sceneReference.SceneGuid });
        }

        /// <summary>
        /// Unloads all scene instances of the same type as the scene the passed in entity belongs to.
        /// </summary>
        /// <param name="scene"></param>
        static public void UnloadAllSceneInstances(Entity scene)
        {
            var sceneGuid = World.Active.EntityManager.GetSharedComponentData<SceneGuid>(scene);
            UnloadAllSceneInstances(sceneGuid);
        }

        static internal void UnloadAllSceneInstances(SceneGuid sceneGuid)
        {
            var em = World.Active.EntityManager;

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            using (var query = em.CreateEntityQuery(ComponentType.ReadOnly<SceneLoadRequest>(), ComponentType.ReadOnly<SceneGuid>()))
            {
                query.SetFilter(sceneGuid);
                using (var issuedRequests = query.ToEntityArray(Allocator.Temp))
                {
                    foreach (var entity in issuedRequests)
                    {
                        var request = em.GetComponentData<SceneLoadRequest>(entity);

                        // Disabled for web builds until https://github.com/emscripten-core/emscripten/issues/8234 is resolved
                        #if !UNITY_WEBGL
                            // Cancels the request if not already complete
                            request.SceneOpHandle.Dispose();
                        #endif
                        ecb.DestroyEntity(entity);
                    }
                }
            }

            using (var query = em.CreateEntityQuery( new EntityQueryDesc()
                {
                    All = new[] { ComponentType.ReadOnly<SceneGuid>() },
                    None = new[] { ComponentType.ReadOnly<SceneLoadRequest>() }
            }))
            {
                query.SetFilter(sceneGuid);
                ecb.DestroyEntity(query);
            }
            ecb.Playback(em);
            ecb.Dispose();
        }

        /// <summary>
        /// Retrieves the status of a scene load request.
        /// </summary>
        /// <param name="scene">Pass in the entity returned from `LoadSceneAsync`</param>
        /// <returns></returns>
        static public SceneStatus GetSceneStatus(Entity scene)
        {
            var sceneData = World.Active.EntityManager.GetComponentData<SceneData>(scene);
            return sceneData.Status;
        }
    }

    /// <summary>
    /// System for handling scene load requests, and instantiating the requested scene entities into `World.Active`
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class SceneStreamingSystem : ComponentSystem
    {
        static public readonly string DataRootPath = "Data/";

        private World m_LoadingWorld;
        private EntityQuery m_PendingRequestsQuery;

        protected override void OnCreate()
        {
            m_LoadingWorld = new World("Loading World");
            m_PendingRequestsQuery = EntityManager.CreateEntityQuery(
                ComponentType.ReadWrite<SceneData>(),
                ComponentType.ReadWrite<SceneLoadRequest>());

            // Ensure these systems are created
            EntityManager.World.GetOrCreateSystem<EntityReferenceRemapSystem>();
            EntityManager.World.GetOrCreateSystem<RemoveRemapInformationSystem>();
        }

        protected override void OnDestroy()
        {
            m_LoadingWorld.Dispose();
            m_PendingRequestsQuery.Dispose();
        }

        protected override unsafe void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithAll<SceneData, RequestSceneLoaded>()
                .WithNone<SceneLoadRequest>()
                .ForEach((Entity e) =>
                {
                    var sceneData = EntityManager.GetComponentData<SceneData>(e);
                    var sceneGuid = sceneData.Scene.SceneGuid;
                    var path = DataRootPath + sceneGuid.Guid.ToString("N");

                    // Fire async reads for scene data
                    SceneLoadRequest request = new SceneLoadRequest();
                    request.SceneOpHandle = IOService.RequestAsyncRead(path);
                    ecb.AddComponent(e, request);
                    ecb.RemoveComponent<RequestSceneLoaded>(e);

                    sceneData.Status = SceneStatus.Loading;
                    ecb.SetComponent(e, sceneData);
                });
            ecb.Playback(EntityManager);
            ecb.Dispose();

            var pendingRequests = m_PendingRequestsQuery.ToEntityArray(Allocator.Temp);
            ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var requestEntity in pendingRequests)
            {
                SceneData sceneData = EntityManager.GetComponentData<SceneData>(requestEntity);
                SceneLoadRequest request = EntityManager.GetComponentData<SceneLoadRequest>(requestEntity);

                var opStatus = request.SceneOpHandle.GetStatus();

                if (opStatus <= Status.InProgress)
                {
                    continue;
                }

                if (opStatus == Status.Failure)
                {
                    request.SceneOpHandle.Dispose();
                    ecb.RemoveComponent<SceneLoadRequest>(requestEntity);

                    sceneData.Status = SceneStatus.FailedToLoad;
                    ecb.SetComponent(requestEntity, sceneData);
                    continue;
                }
                Assert.IsTrue(opStatus == Status.Success);

                request.SceneOpHandle.GetData(out var data, out var sceneDataSize);
                SceneHeader header = *(SceneHeader*)data;
                int headerSize = UnsafeUtility.SizeOf<SceneHeader>();
                if (header.Version != SceneHeader.CurrentVersion)
                {
                    throw new Exception($"Scene serialization version mismatch. Reading version '{header.Version}', expected '{SceneHeader.CurrentVersion}'");
                }

                byte* decompressedScene = data + headerSize;
                if (header.Codec != Codec.Codec.None)
                {
                    decompressedScene = (byte*)UnsafeUtility.Malloc(header.DecompressedSize, 16, Allocator.Temp);

                    if (!CodecService.Decompress(header.Codec, data + headerSize, sceneDataSize - headerSize, decompressedScene, header.DecompressedSize))
                    {
                        throw new Exception($"Failed to decompress compressed scene using codec '{header.Codec}'");
                    }
                }

                using (var sceneReader = new MemoryBinaryReader(decompressedScene))
                {
                    var loadingEM = m_LoadingWorld.EntityManager;
                    var transaction = loadingEM.BeginExclusiveEntityTransaction();
                    if (header.SharedComponentCount > 0)
                    {
                        int numSharedComponentsLoaded = SerializeUtility.DeserializeSharedComponents(loadingEM, sceneReader);

                        // Chunks have now taken over ownership of the shared components (reference counts have been added)
                        // so remove the ref that was added on deserialization
                        for (int i = 0; i < numSharedComponentsLoaded; ++i)
                        {
                            transaction.ManagedComponentStore.RemoveReference(i + 1);
                        }
                        Assert.IsTrue(numSharedComponentsLoaded == header.SharedComponentCount,
                            $"Number of loaded SharedComponents for '{sceneData.Scene.SceneGuid.Guid}' loaded ({numSharedComponentsLoaded }) does not match what we expect ({header.SharedComponentCount})");
                    }

                    SerializeUtility.DeserializeWorld(transaction, sceneReader, header.SharedComponentCount);
                    loadingEM.EndExclusiveEntityTransaction();
                }

                var scene = sceneData.Scene;
                var activeEM = World.Active.EntityManager;
                activeEM.MoveEntitiesFrom(out var movedEntities, m_LoadingWorld.EntityManager);
                foreach (var e in movedEntities)
                {
                    ecb.AddSharedComponent(e, scene.SceneGuid);
                    ecb.AddSharedComponent(e, scene.SceneInstanceId);
                }

                // Fixup Entity references now that the entities have moved
                EntityManager.World.GetExistingSystem<EntityReferenceRemapSystem>().Update();
                EntityManager.World.GetExistingSystem<RemoveRemapInformationSystem>().Update();

                if (header.Codec != Codec.Codec.None)
                {
                    UnsafeUtility.Free(decompressedScene, Allocator.Temp);
                }

                sceneData.Status = SceneStatus.Loaded;
                ecb.SetComponent(requestEntity, sceneData);

                request.SceneOpHandle.Dispose();
                ecb.RemoveComponent<SceneLoadRequest>(requestEntity);
                
				m_LoadingWorld.EntityManager.PrepareForDeserialize();
                movedEntities.Dispose();
            }
            ecb.Playback(EntityManager);
            ecb.Dispose();
            pendingRequests.Dispose();
        }
    }
}
