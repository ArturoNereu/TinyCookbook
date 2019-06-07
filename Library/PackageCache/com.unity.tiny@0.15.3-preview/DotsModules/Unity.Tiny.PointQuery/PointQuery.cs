using System;
using System.Runtime.InteropServices;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny;

[assembly:ModuleDescription("Unity.Tiny.PointQuery", "Point query utilities")]
namespace Unity.Tiny.PointQuery
{
    // Component used to mark an entity created by CreatePointQueryStruct.
    [HideInInspector]
    public struct PointQueryTag : IComponentData
    {
    }

    // System state to track the PointQueryStruct, and track/free native resources.
    internal struct PointQuerySystemTag : ISystemStateComponentData
    {
        internal int queryID;
    }

    /// <summary>
    /// A structure returned from PointService queries.
    /// </summary>
    public struct QueryResult : IBufferElementData
    {
        public float distance;
        public Entity e;
    }

    /// <summary>
    ///  A static class to perform a "closest n points query" in 3D.
    /// </summary>
    public static unsafe class PointQueryService
    {
        [DllImport("lib_unity_tiny_pointquery", EntryPoint = "initPointQueryStruct")]
        private static extern int InitPointQueryStruct(int queryID, int expectedSize);

        [DllImport("lib_unity_tiny_pointquery", EntryPoint = "freePointQueryStruct")]
        internal static extern void FreePointQueryStruct(int queryID);

        [DllImport("lib_unity_tiny_pointquery", EntryPoint = "addPointToQueryStruct")]
        private static extern void AddPointToQueryStruct(int queryID, Entity e, float3* vector3);

        [DllImport("lib_unity_tiny_pointquery", EntryPoint = "addPointsToQueryStruct")]
        private static extern void AddPointsToQueryStruct(int queryID, int n, void* e, void* vector3);

        [DllImport("lib_unity_tiny_pointquery", EntryPoint = "queryNearestPoints")]
        private static extern int QueryNearestPoints(int queryID, float3* vector3Array, float maxDist, float minDist, int nDst, void* dst);

        [DllImport("lib_unity_tiny_pointquery", EntryPoint = "numTreesAllocated")]
        private static extern int NumTreesAllocated();

        [DllImport("lib_unity_tiny_pointquery", EntryPoint = "testKDTree")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool TestKDTree();

        /// <summary>
        ///  Creates a new entity that has a point query structure, and can be passed
        ///  to other functions in this service.
        /// </summary>
        /// <remarks>
        ///  Calls to the PointQueryService are
        ///  synchronous and immediate; you don't have wait for a System or World update.
        ///
        ///  The created entity has an internal hidden component, and an external
        ///  PointQueryStructTag component.
        ///
        ///  To free the allocated memory used by the structure, destroy the entity.
        /// </remarks>
        public static Entity CreatePointQueryStruct(EntityManager mgr)
        {
            if (mgr.World.GetExistingSystem(typeof(PointQuerySystem)) == null)
            {
                mgr.World.AddSystem(new PointQuerySystem());
            }

            Entity e = mgr.CreateEntity();
            int queryID = InitPointQueryStruct(0, 0);
            PointQuerySystemTag queryTag = new PointQuerySystemTag()
            {
                queryID = queryID
            };
            mgr.AddComponent(e, typeof(PointQuerySystemTag));
            mgr.SetComponentData(e, queryTag);
            mgr.AddComponent(e, typeof(PointQueryTag));
            return e;
        }

        /// <summary>
        ///  Prepares and resets a point query structure.
        ///  You can use this to clear a query structure, or optimize allocations
        ///  when the number of items is known ahead of time.
        /// </summary>
        /// <param name="eQuery">Entity created via createPointQueryStruct</param>
        /// <param name="numExpectedPoints">Number of points expected. This is an
        /// optional hint for performance and does not need to match exactly. </param>
        public static void ResetPointQueryStruct(EntityManager mgr, Entity eQuery, int numExpectedPoints)
        {
            PointQuerySystemTag tag = mgr.GetComponentData<PointQuerySystemTag>(eQuery);
            InitPointQueryStruct(tag.queryID, numExpectedPoints);
        }

        /// <summary>
        ///  Adds a single point to the query structure.
        /// </summary>
        /// <remarks>
        ///  The id Entity does not have to be a valid entity, and can be treated
        ///  as an integer id.
        ///  The only disallowed id is the <see cref="Entity.Null"/>, which is used internally.
        /// </remarks>
        public static void AddPointToQueryStruct(EntityManager mgr, Entity eQuery, float3 point, Entity id)
        {
            PointQuerySystemTag tag = mgr.GetComponentData<PointQuerySystemTag>(eQuery);
            AddPointToQueryStruct(tag.queryID, id, &point);
        }

        /// <summary>
        ///  Adds a group of points to the query structure.
        /// </summary>
        /// <remarks>
        ///  The id Entity does not have to be a valid entity, and can be treated
        ///  as an integer id.
        ///  The only disallowed id is the <see cref="Entity.Null"/>, which is used internally.
        /// </remarks>
        /// <param name="points">List of points to add. Length must match <paramref name="ids"/>.</param>
        /// <param name="ids">List of entities to add. Length must match <paramref name="points"/>.</param>
        public static void AddPointsToQueryStruct(EntityManager mgr, Entity eQuery, NativeList<float3> points, NativeList<Entity> ids)
        {
            if (points.Length != ids.Length)
            {
                throw new ArgumentException();    // FIXME Does this even work? What should I use?
            }
            PointQuerySystemTag tag = mgr.GetComponentData<PointQuerySystemTag>(eQuery);
            AddPointsToQueryStruct(tag.queryID, points.Length, ids.GetUnsafePtr<Entity>(), points.GetUnsafePtr<float3>());
        }

        /// <summary>
        ///  Query for the closest Entity to <paramref name="point"/>. (When the Entity was added to this structure.)
        ///</summary>
        /// <remarks>
        ///  Closeness is based on Euclidean distance. This query only considers
        ///  points inside the hull around the point, which is described by <paramref name="maxDist"/> (exclusive)
        ///  and <paramref name="minDist"/> (inclusive).
        /// </remarks>
        public static Entity QueryClosestPoint(EntityManager mgr, Entity eQuery, float3 point, float maxDist,
            float minDist)
        {
            PointQuerySystemTag tag = mgr.GetComponentData<PointQuerySystemTag>(eQuery);
            QueryResult dst;
            int n = QueryNearestPoints(tag.queryID, &point, maxDist, minDist, 1, &dst);
            return dst.e;
        }

        /// <summary>
        ///  Query for the <paramref name="n"/> closest points to <paramref name="point"/> in in the query structure.
        /// </summary>
        /// <remarks>
        ///  Closeness is based on Euclidean distance. This query only considers
        ///  points inside the hull around the point, which is described by <paramref name="maxDist"/> (exclusive)
        ///  and <paramref name="minDist"/> (inclusive).
        ///  Returns a list of ids and distances of the closest points, sorted by
        ///  most distant first.
        ///  This is slower than single-point queries. For best results, <paramref name="n"/> should
        ///  not be too large.
        /// </remarks>
        public static NativeList<QueryResult> QueryNClosestPoints(EntityManager mgr, Entity eQuery, float3 point, float maxDist, float minDist,
            int n)
        {
            var result = new NativeList<QueryResult>(Allocator.Temp);
            result.ResizeUninitialized(n);
            PointQuerySystemTag tag = mgr.GetComponentData<PointQuerySystemTag>(eQuery);
            int newN = QueryNearestPoints(tag.queryID, &point, maxDist, minDist, n, result.GetUnsafePtr<QueryResult>());
            result.ResizeUninitialized(newN);
            return result;
        }

        // Resource tracking and debugging.
        public static int NumberOfQueryStructsAllocated()
        {
            return NumTreesAllocated();
        }

    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    internal class PointQuerySystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithNone<PointQueryTag>()
                .ForEach((Entity e, ref PointQuerySystemTag tag) =>
            {
                PointQueryService.FreePointQueryStruct(tag.queryID);
                ecb.RemoveComponent<PointQuerySystemTag>(e);
            });
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
