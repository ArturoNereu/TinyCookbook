using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Tiny.HitBox2D
{
    /// <summary>
    ///  Describes a 2D hit box for simple picking.
    /// </summary>
    /// <remarks>
    ///  Hit boxes can be separate from sprite rendering, but should have a
    ///  transform component.
    /// </remarks>
    public struct RectHitBox2D : IComponentData
    {
        public static RectHitBox2D Default { get; } = new RectHitBox2D
        {
            box = new Rect(0f, 0f, 1f, 1f)
        };

        /// <summary>
        ///  Defines the hit-area rectangle used for picking and non-physics
        ///  collision checks. Its pivot point is at coordinates 0,0.
        /// </summary>
        public Rect box;
    }

    /// <summary>
    ///  Describes a 2D hit box for simple picking.
    /// </summary>
    /// <remarks>
    ///  This component behaves the same as a HitBox2D component,
    ///  but a Sprite2DRenderer component defines its size.
    /// </remarks>
    public struct Sprite2DRendererHitBox2D : IComponentData
    {
        /// <summary>
        ///  When true, the Sprite2DRendererHitBox2D component uses pixel-accurate
        ///  hit testing from the sprite. Defaults to false.
        /// </summary>
        /// <remarks>
        ///  Pixel-accurate hit testing requires that the entity have both an Image2D
        ///  component and an Image2DAlphaMask component attached before image loading.
        ///  It ignores hits and overlap where the sprite Alpha is zero.
        ///  Note that pixel-accurate hit testing is computationally intensive.
        /// </remarks>
        public bool pixelAccurate;
    }

    /// <summary>
    ///  When added to an entity, this component describes overlap between that entity
    ///  and another entity with a Sprite2DRendererHitBox2D HitBox2D component attached.
    /// </summary>
    public struct HitBoxOverlap : IBufferElementData
    {
        /// <summary>
        ///  Specifies the "other" entity, which has a Sprite2DRendererHitBox2D
        ///  or HitBox2D component attached.
        /// </summary>
        public Entity otherEntity;

        /// <summary>
        ///  Specifies the camera that sees the overlap.
        /// </summary>
        public Entity camera;
    }

    /// <summary>
    ///  The <see cref="HitBox2DSystem.HitTest"/> function returns this structure.
    /// </summary>
    public struct HitTestResult : IComponentData
    {
        /// <summary>
        ///  Specifies the hit entity, or NONE if no entity is hit.
        /// </summary>
        public Entity entityHit;

        /// <summary>
        ///  Specifies normalized [0..1] coordinates for a hit location
        ///  on a sprite. The coordinate system's origin is the lower-left corner.
        /// </summary>
        public float2 uv;
    }

    /// <summary>
    ///  The <see cref="HitBox2DSystem.RayCast"/> function returns this structure.
    /// </summary>
    public struct RayCastResult : IComponentData // component data so bindgem sees it
    {
        /// <summary>
        ///  Specifies the hit entity, or NONE if no entity is hit.
        /// </summary>
        public Entity entityHit;

        /// <summary>
        ///  Specifies a normalized [0..1] distance along a ray.
        ///  The hit location is: hit = rayStart + (rayEnd-rayStart)*t;
        ///  If the ray cast starts inside a hit box, t is the exit value.
        /// </summary>
        public float t;
    }

    internal class MakeEntryHitBox2D : IExternalDisplayListEntryMaker
    {
        public int GetRendererComponent()
        {
            return TypeManager.GetTypeIndex<RectHitBox2D>();
        }

        public bool DoNotClip()
        {
            return false;
        }

        public void Filter(ref EntityQueryBuilder query)
        {
            query.WithAll<RectHitBox2D>();
            query.WithNone<Sprite2DRenderer, Shape2DRenderer>();
        }

        public bool MakeEntry(EntityManager em, ref DisplayListEntry de)
        {
            Assert.IsTrue(!em.HasComponent<Sprite2DRenderer>(de.e));
            Assert.IsTrue(!em.HasComponent<Shape2DRenderer>(de.e));
            var hb =  em.GetComponentData<RectHitBox2D>(de.e);
            if (hb.box.IsEmpty())
                return false;
            de.inBounds = hb.box;
            de.type = DisplayListEntryType.HitBoxOnly;
            return true;
        }

        public void Update(ComponentSystem cs) { }
    }

    /// <summary>
    ///  This system tests for overlaps between all components under all cameras
    ///  in the world.
    /// </summary>
    /// <remarks>
    ///  It adds HitBoxOverlapResults components to entities that
    ///  overlap others, and removes them from entities that no longer overlap others.
    ///
    ///  Overlaps are only detected between objects that overlap under the same camera.
    /// </remarks>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(DisplayListSystem))]            // needs the display list to be done before valid results can be computed
    public class HitBox2DSystem : ComponentSystem
    {
        protected IntPtr cppWrapper;

        [DllImport("lib_unity_tiny_hitbox2d", EntryPoint = "sweepline_hitbox2d")]
        protected static extern void SweepLineNative(IntPtr emr, Entity e, Camera2D cam);

        [DllImport("lib_unity_tiny_hitbox2d", EntryPoint = "sweepresults_hitbox2d")]
        protected static extern unsafe int GetSweepResultsNative (Entity** e, HitBoxOverlap** outPtr);

        [DllImport("lib_unity_tiny_hitbox2d", EntryPoint = "hittest_hitbox2d")]
        protected static extern void HitTestNative(IntPtr emr, float3 hitPoint, Entity camera, ref HitTestResult result);

        [DllImport("lib_unity_tiny_hitbox2d", EntryPoint = "init_hitbox2d")]
        protected static extern void InitNative();

        [DllImport("lib_unity_tiny_hitbox2d", EntryPoint = "deinit_hitbox2d")]
        protected static extern void DeinitNative();

        [DllImport("lib_unity_tiny_hitbox2d", EntryPoint = "raycast_hitbox2d")]
        protected static extern void RayCastNative(IntPtr emr, float3 startPoint, float3 endPoint, Entity camera, ref RayCastResult result);

        [DllImport("lib_unity_tiny_hitbox2d", EntryPoint = "detailedOverlapInformation_hitbox2d")]
        protected static extern unsafe int DetailedOverlapInformationNative(IntPtr emr, Entity e, HitBoxOverlap overlap, float2* result);

        protected override void OnCreate()
        {
            cppWrapper = CPlusPlus.WrapEntityManager(EntityManager);
            DisplayListSystem.RegisterDisplayListEntryMaker(new MakeEntryHitBox2D());
            InitNative();
        }

        protected override void OnDestroy()
        {
            DisplayListSystem.DeRegisterExternalDisplayListEntryMaker(TypeManager.GetTypeIndex<RectHitBox2D>());
            DeinitNative();
            CPlusPlus.ReleaseHandleForEntityManager(cppWrapper);
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            // make sure we have results ready everywhere

            // remove buffers on things that lost hitbox2d
            Entities.WithNone<Sprite2DRendererHitBox2D, RectHitBox2D>().WithAll<HitBoxOverlap>().ForEach((Entity e) =>
            {
                ecb.RemoveComponent<HitBoxOverlap>(e);
            });

            // clear remaining buffers
            Entities.ForEach((Entity e, DynamicBuffer<HitBoxOverlap> buffer) =>
            {
                buffer.Clear();
            });

            // add missing buffers
            Entities.WithAny<Sprite2DRendererHitBox2D, RectHitBox2D>().WithNone<HitBoxOverlap>().ForEach((Entity e) =>
            {
                ecb.AddBuffer<HitBoxOverlap>(e);
            });

            ecb.Playback(EntityManager);
            ecb.Dispose();

            Entities.WithAll<DisplayListCamera>().ForEach((Entity e, DynamicBuffer<DisplayListEntry> displayList, ref Camera2D cam) =>
            {
                SweepLineNative(cppWrapper, e, cam);
                //var bfe = GetBufferFromEntity<HitBoxOverlap>();
                unsafe
                {
                    Entity* outPtrE = null;
                    HitBoxOverlap* outPtrHBO = null;
                    int outCount = GetSweepResultsNative(&outPtrE, &outPtrHBO);
                    for ( int i=0; i<outCount; i++ )
                    {
                        DynamicBuffer<HitBoxOverlap> buffer = EntityManager.GetBuffer<HitBoxOverlap>(outPtrE[i]);
                        buffer.Add(outPtrHBO[i]);
                        //bfe[outPtrE[i]].Add(outPtrHBO[i]);
                    }
                }
            });

            // remove empty buffers
            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities.ForEach((Entity e, DynamicBuffer<HitBoxOverlap> buffer) =>
            {
                if (buffer.Length == 0)
                    ecb.RemoveComponent<HitBoxOverlap>(e);
            });
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        /// <summary>
        /// Find the entity under <paramref name="hitPoint"/> for the given <paramref name="camera"/>.
        /// </summary>
       public HitTestResult HitTest(float3 hitPoint, Entity camera)
       {
           HitTestResult result = new HitTestResult(){entityHit = Entity.Null, uv = new float2(0,0)};
           if (!EntityManager.Exists(camera) || !EntityManager.HasComponent<DisplayListCamera>(camera))
               return result;

           HitTestNative(cppWrapper, hitPoint, camera, ref result);
           return result;
        }

        /// <summary>
        /// Return an Entity that has a DisplayListCamera that contains point <paramref name="hitPoint"/>, or Null
        /// if none matches.
        /// </summary>
        public Entity HitTestCamera(float3 hitPoint)
        {
            // world to screen
            var env = EntityManager.World.TinyEnvironment();
            DisplayInfo di = env.GetConfigData<DisplayInfo>();
            Unity.Tiny.Core2D.Rect screenRect = new Unity.Tiny.Core2D.Rect(0.0f, 0.0f, (float)di.width, (float)di.height);
            float2 screenSize = new float2((float)di.width, (float)di.height);
            Entity r = Entity.Null;
            float bestDepth = 0;
            Entities.ForEach((Entity e, ref Camera2D c2d, ref DisplayListCamera dlc, ref LocalToWorld t) =>
            {
                float2 window = TransformHelpers.WorldToWindow(this, e, hitPoint, screenSize);
                Unity.Tiny.Core2D.Rect cRect;
                if (c2d.rect.IsEmpty())
                    cRect = new Unity.Tiny.Core2D.Rect(0, 0, 1, 1);
                else
                    cRect = c2d.rect;
                Unity.Tiny.Core2D.Rect camRect = screenRect.Region(cRect);
                if (camRect.Contains(window)) {
                    if (r == Entity.Null || c2d.depth > bestDepth) {
                        r = e;
                        bestDepth = c2d.depth;
                    }
                }
            });
            return r;
        }

        /// <summary>
        ///  Returns a 2D convex polygon outline of the intersection between two
        ///  hit boxes. This can be rendered as a Shape2D component.
        /// </summary>
        public NativeArray<float2> DetailedHitBoxOverlapInformation(Entity e, HitBoxOverlap overlap, Allocator allocator=Allocator.Temp)
        {
            if (e == Entity.Null || overlap.otherEntity == Entity.Null)
                return new NativeArray<float2>(0,allocator);
            if (e == overlap.otherEntity) {
                Assert.IsTrue(false, "DetailedOverlapInformation is trying to check an entity with itself.");
                return new NativeArray<float2>(0,allocator);
            }
            NativeArray<float2> result;
            unsafe
            {
                float* temp = stackalloc float [16];
                int nresult = DetailedOverlapInformationNative(cppWrapper, e, overlap, (float2*)temp);
                result = new NativeArray<float2>(nresult,allocator);
                for ( int i=0; i<nresult; i++ )
                    result[i] = new float2(temp[i*2], temp[i*2+1]);
            }
            return result;
        }

        public RayCastResult RayCast(float3 startPoint, float3 endPoint, Entity camera)
        {
            RayCastResult result = new RayCastResult() { entityHit = Entity.Null, t = 1.0f };
            RayCastNative(cppWrapper, startPoint, endPoint, camera, ref result);
            return result;
        }
    }
}
