using System;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;

namespace Unity.Tiny.Core2D
{
    /// <summary>
    ///  A system that has to run in order to update the LocalToParent component.
    ///  Required.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(UpdateWorldTransformSystem))]
    public class UpdateLocalTransformSystem : ComponentSystem
    {
        public UpdateLocalTransformSystem()
        {
            InitEntityQueryCache(20);
        }

        protected override void OnUpdate()
        {
            var mgr = EntityManager;
            EntityCommandBuffer ecb;

            // make sure we have a LocalToParent for every Parent component. also initialize it to identity
            // for nodes that do not have any further transforms
            LocalToParent tlid = new LocalToParent {Value = float4x4.identity};
            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithAll<Parent>()
                .With(EntityQueryOptions.IncludeDisabled)
                .WithNone<LocalToParent>()
                .ForEach(e => ecb.AddComponent(e, tlid));
            ecb.Playback(mgr);
            ecb.Dispose();

            // make sure we have a PrivateTransformData for every Parent component
            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithAll<Parent>()
                .With(EntityQueryOptions.IncludeDisabled)
                .WithNone<PrivateTransformData>()
                .ForEach(e => ecb.AddComponent(e, default(PrivateTransformData))); // zero init ok
            ecb.Playback(mgr);
            ecb.Dispose();

            // remove PrivateTransformStatic if there is no StaticTransform component
            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithAll<PrivateTransformStatic>()
                .With(EntityQueryOptions.IncludeDisabled)
                .WithNone<TransformStatic>()
                .ForEach(e => ecb.RemoveComponent<PrivateTransformStatic>(e));
            ecb.Playback(mgr);
            ecb.Dispose();

            // clean up system state component
            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithAll<PrivateTransformData>()
                .WithNone<Parent>()
                .ForEach(e => ecb.RemoveComponent<PrivateTransformData>(e));
            ecb.Playback(mgr);
            ecb.Dispose();

            // mark all world transforms dirty (unless they are static!)
            Entities
                .WithNone<PrivateTransformStatic>()
                .ForEach((ref PrivateTransformData ptf) =>
                {
                    ptf.inSortingGroup = Entity.Null;
                    ptf.flags = 1; // mark dirty
                });

            // all combinations of TRS

            // If only a LocalToParent is present, it is used as-is.  If any of the LocalToParent* components
            // are present, they are transformed into a LocalToParent.
            Entities
                .WithNone<PrivateTransformStatic>()
                .ForEach((ref Translation localPosition, ref LocalToParent local) =>
                {
#if UNITY_USE_TINYMATH
                    local.Value = tinymath.Translation(localPosition.Value);
#else
                    local.Value = float4x4.Translate(localPosition.Value);
#endif
                });

            Entities
                .WithNone<PrivateTransformStatic>()
                .ForEach((ref Rotation localRotation, ref LocalToParent local) =>
                {
#if UNITY_USE_TINYMATH
                    local.Value = tinymath.Rotation(localRotation.Value);
#else
                    local.Value = new float4x4(localRotation.Value, float3.zero);
#endif
                });

            Entities
                .WithNone<PrivateTransformStatic>()
                .ForEach((ref NonUniformScale localScale, ref LocalToParent local) =>
                {
#if UNITY_USE_TINYMATH
                    local.Value = tinymath.Scale(localScale.Value);
#else
                    local.Value = float4x4.Scale(localScale.Value);
#endif
                });

            Entities
                .WithNone<PrivateTransformStatic>()
                .ForEach((ref Scale localScale, ref LocalToParent local) =>
                {
#if UNITY_USE_TINYMATH
                    local.Value = tinymath.Scale(localScale.Value);
#else
                    local.Value = float4x4.Scale(localScale.Value);
#endif
                });

            Entities
                .WithNone<PrivateTransformStatic>()
                .ForEach((ref Translation localPosition, ref Rotation localRotation, ref LocalToParent local) =>
                {
#if UNITY_USE_TINYMATH
                    local.Value = tinymath.TranslationRotation(localPosition.Value, localRotation.Value);
#else
                    local.Value = new float4x4(localRotation.Value, localPosition.Value);
#endif
                });

            Entities
                .WithNone<PrivateTransformStatic>()
                .ForEach((ref Translation localPosition, ref NonUniformScale localScale, ref LocalToParent local) =>
                {
#if UNITY_USE_TINYMATH
                    local.Value = tinymath.TranslationScale(localPosition.Value, localScale.Value);
#else
                    local.Value = math.mul(float4x4.Translate(localPosition.Value), float4x4.Scale(localScale.Value));
#endif
                });

            Entities
                .WithNone<PrivateTransformStatic>()
                .ForEach((ref Translation localPosition, ref Scale localScale, ref LocalToParent local) =>
                {
#if UNITY_USE_TINYMATH
                    local.Value = tinymath.TranslationScale(localPosition.Value, localScale.Value);
#else
                    local.Value = math.mul(float4x4.Translate(localPosition.Value), float4x4.Scale(localScale.Value));
#endif
                });

            Entities
                .WithNone<PrivateTransformStatic>()
                .ForEach((ref NonUniformScale localScale, ref Rotation localRotation, ref LocalToParent local) =>
                {
#if UNITY_USE_TINYMATH
                    local.Value = tinymath.RotationScale(localRotation.Value, localScale.Value);
#else
                    local.Value = math.mul(new float4x4(localRotation.Value, float3.zero), float4x4.Scale(localScale.Value));
#endif
                });

            Entities
                .WithNone<PrivateTransformStatic>()
                .ForEach((ref Scale localScale, ref Rotation localRotation, ref LocalToParent local) =>
                {
#if UNITY_USE_TINYMATH
                    local.Value = tinymath.RotationScale(localRotation.Value, localScale.Value);
#else
                    local.Value = math.mul(new float4x4(localRotation.Value, float3.zero), float4x4.Scale(localScale.Value));
#endif
                });

            Entities
                .WithNone<PrivateTransformStatic>()
                .ForEach((ref Translation localPosition, ref NonUniformScale localScale, ref Rotation localRotation, ref LocalToParent local) =>
                {
#if UNITY_USE_TINYMATH
                    local.Value = tinymath.TranslationRotationScale(localPosition.Value, localRotation.Value, localScale.Value);
#else
                    local.Value = math.mul(new float4x4(localRotation.Value, localPosition.Value), float4x4.Scale(localScale.Value));
#endif
                });

            Entities
                .WithNone<PrivateTransformStatic>()
                .ForEach((ref Translation localPosition, ref Scale localScale, ref Rotation localRotation, ref LocalToParent local) =>
                {
#if UNITY_USE_TINYMATH
                    local.Value = tinymath.TranslationRotationScale(localPosition.Value, localRotation.Value, localScale.Value);
#else
                    local.Value = math.mul(new float4x4(localRotation.Value, localPosition.Value), float4x4.Scale(localScale.Value));
#endif
                });
        }
    }

    /// <summary>
    ///  A system that has to run in order to update the LocalToWorld
    ///  component. Required.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(UpdateLocalTransformSystem))]
    [UpdateBefore(typeof(DisplayListSystem))]
    public class UpdateWorldTransformSystem : ComponentSystem
    {
        const int sMaxHierarchyDepth = 256;

        static Entity ComputeWorldTransformAndSortingGroupRec(Entity e, int maxDepth, EntityManager mgr)
        {
            PrivateTransformData ptf = mgr.GetComponentData<PrivateTransformData>(e);
            if (ptf.flags == 0)
                return ptf.inSortingGroup;

            Entity ep = mgr.GetComponentData<Parent>(e).Value;
            LocalToParent tol = mgr.GetComponentData<LocalToParent>(e);
            LocalToWorld tow = mgr.GetComponentData<LocalToWorld>(e);

            if (ep == Entity.Null) {
                // local and global the same
                ptf.inSortingGroup = Entity.Null;
                tow.Value = tol.Value;
            } else {
                Assert.IsTrue(mgr.Exists(ep), "An entity has a Parent parent set to an entity that was deleted. Did you mean to use TransformHelpers.DestroyTree?");
                Assert.IsTrue(mgr.HasComponent<Parent>(ep), "An entity has a Parent parent set to an entity that is not a Parent.");
                Assert.IsTrue(!mgr.HasComponent<Disabled>(ep), "An entity has a Parent parent that is disabled. The child must be disabled itself to avoid rendering it. Use TransformHelpers.DisableTree to disable a hierarchy.");
                if (maxDepth <= 0) {
                    tow.Value = float4x4.identity;
                    Assert.IsTrue(false,"Transform hierarchy is too deep or has a cycle. Run TransformHelpers.DebugCheckAllNodes to debug.");
                    return Entity.Null;
                }
                // we have a parent!
                Entity ptinsg = ComputeWorldTransformAndSortingGroupRec(ep, maxDepth - 1, mgr);
                float4x4 matp = mgr.GetComponentData<LocalToWorld>(ep).Value;
#if UNITY_USE_TINYMATH
                tow.Value = tinymath.mul(matp, tol.Value);
#else
                tow.Value = math.mul(matp, tol.Value);
#endif
                if (mgr.HasComponent<SortingGroup>(ep)) {
                    // parent is head of a group?
                    ptf.inSortingGroup = ep;
                } else {
                    // take whatever group parent is in
                    ptf.inSortingGroup = ptinsg;
                }
            }

            ptf.flags = 0;
            mgr.SetComponentData<LocalToWorld>(e,tow);
            mgr.SetComponentData<PrivateTransformData>(e,ptf);
            return ptf.inSortingGroup;
        }

        protected override void OnUpdate()
        {
            var mgr = EntityManager;
            EntityCommandBuffer ecb;

            // Make sure everything that has a Parent has a LocalToWorld
            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithAll<Parent>()
                .With(EntityQueryOptions.IncludeDisabled)
                .WithNone<LocalToWorld>()
                .ForEach(e => ecb.AddComponent(e, default(LocalToWorld)));
            ecb.Playback(mgr);
            ecb.Dispose();

            // main recursive eval
            Entities
                .WithNone<PrivateTransformStatic>()
                .ForEach((Entity e, ref PrivateTransformData ptf, ref Parent node, ref LocalToWorld tow,
                    ref LocalToParent tol) =>
                {
                    // short cut all trivial cases
                    if (ptf.flags == 0)
                        return;
                    if (node.Value == Entity.Null) {
                        ptf.inSortingGroup = Entity.Null;
                        ptf.flags = 0;
                        tow.Value = tol.Value;
                        return;
                    }
                    // have parent - update parent first
                    Entity ptinsg = ComputeWorldTransformAndSortingGroupRec( node.Value, sMaxHierarchyDepth, mgr);
                    float4x4 matp = mgr.GetComponentData<LocalToWorld>(node.Value).Value;
#if UNITY_USE_TINYMATH
                    tow.Value = tinymath.mul(matp,tol.Value);
#else
                    tow.Value = math.mul(matp,tol.Value);
#endif
                    if (mgr.HasComponent<SortingGroup>(node.Value)) {
                        // parent is head of a group?
                        ptf.inSortingGroup = node.Value;
                    } else {
                        // take whatever group parent is in
                        ptf.inSortingGroup = ptinsg;
                    }
                    ptf.flags = 0;
                });

            // add PrivateTransformStatic to all TransformStatic so they are not touched ever again
            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithAll<TransformStatic>()
                .WithNone<PrivateTransformStatic>()
                .ForEach(e => ecb.AddComponent(e, default(PrivateTransformStatic)));
            ecb.Playback(mgr);
            ecb.Dispose();
        }
    }

}
