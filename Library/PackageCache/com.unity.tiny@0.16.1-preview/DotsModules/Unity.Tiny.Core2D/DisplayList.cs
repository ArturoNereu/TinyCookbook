using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;

namespace Unity.Tiny.Core2D
{
    public enum DisplayListEntryType
    {
        Unknown = 0,
        HitBoxOnly,
        Sprite,
        TiledSprite,
        SlicedSprite,
        Shape,
        GroupOnly,
        Tilemap,
        Text
    }

    [HideInInspector]
    public struct DisplayListEntry : IBufferElementData
    {
        public Entity e;
        public float4x4 finalMatrix;
        public Rect inBounds;
        public DisplayListEntryType type;
        // sorting groups are the id of the first entity that has the SortingGroup component on it, walking up the hierarchy
        public Entity inSortingGroup;
    }

    [HideInInspector]
    public struct DisplayListCamera : ISystemStateComponentData
    {
        public float4x4 world;
        public float4x4 inverseWorld;
        public float4 sortingDot;
        public float2 clip2D;
        public float clipZNear;
        public float clipZFar;
        // Always next to DisplayListCamera:
        // NativeBuffer<DisplayListEntry> displayList;
        // NativeBuffer<SortedEntity> sortedList;
    }

    [HideInInspector]
    public struct SortedEntity : IBufferElementData
    {
        public UInt64 combinedKey; // layer|order|depth
        public int idx;
        public Entity e;

        public void CombineKey(float sortValue, Int16 sortLayer, Int16 sortOrder)
        {
            uint uivalue = math.asuint(sortValue);
            uivalue ^= ((uint)(((int)uivalue) >> 31)) >> 1; // fix up twos complement for negative floats
            combinedKey = ((UInt64)(UInt16)sortLayer << 48 | (UInt64)(UInt16)sortOrder << 32 | (UInt64)uivalue) ^ 0x8000_8000_80000000ul; // pack and fixup signed values for sort
        }

        public void CombineKey(float sortValue) // zero layer, order
        {
            uint uivalue = math.asuint(sortValue);
            uivalue ^= ((uint)(((int)uivalue) >> 31)) >> 1; // fix up twos complement for negative floats
            combinedKey = ((UInt64)uivalue) ^ 0x8000_8000_80000000ul; // fixup signed values for sort
        }
    }

    public interface IExternalDisplayListEntryMaker
    {
        // Component id for this maker
        // This is used as an id only, it needs to be included in filter
        int GetRendererComponent();

        // Do not clip this type of entry, used by helper entries like sorting group heads
        bool DoNotClip();

        // Filter applied during iteration,must include WithAll<RenderComponent>
        void Filter(ref EntityQueryBuilder query);

        void Update(ComponentSystem cs);

        // Callback to create entry
        // DisplayListEntry de is input/output
        //    e = input, the entity being added
        //    finalMatrix = undefined at this point, do not change
        //    inBounds = output, object space bounding rectangle
        //    type = output, type to be used by rendering
        //    inSortingGroup = undefined at this point, do not change
        // return false to discard the entry
        bool MakeEntry(EntityManager mgr, ref DisplayListEntry de);
    }

    // helper to put sorting group heads that on the DL always, regardless of clip or renderability
    class MakeEntrySortingGroup : IExternalDisplayListEntryMaker
    {
        public int GetRendererComponent()
        {
            return TypeManager.GetTypeIndex<SortingGroup>();
        }

        public void Filter(ref EntityQueryBuilder query)
        {
            query.WithAll<SortingGroup>();
        }

        public bool DoNotClip()
        {
            return true;
        }

        public bool MakeEntry(EntityManager mgr, ref DisplayListEntry de)
        {
            de.inBounds = new Rect();
            de.type = DisplayListEntryType.GroupOnly;
            return true;
        }

        public void Update(ComponentSystem cs) { }
    };

    /// <summary>
    ///  Collects and sorts entities by camera, and readies them for rendering.
    ///  Rendering and hit testing do not consider changes made to entities after
    ///  the display list is created.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class DisplayListSystem : ComponentSystem
    {
        const uint groupCacheLength = 256; // must be power of 2 

        protected EntityQuery groupMain;

        // TODO: make non static, access via world
        protected static List<IExternalDisplayListEntryMaker> dlMakerReg = new List<IExternalDisplayListEntryMaker>(32);

        static public void RegisterDisplayListEntryMaker(IExternalDisplayListEntryMaker m)
        {
            for (int i = 0; i < dlMakerReg.Count; i++)
            {
                if (dlMakerReg[i].GetRendererComponent() == m.GetRendererComponent())
                {
                    Assert.IsTrue(false,"Duplicate entry.");
                    return;
                }
            }
            dlMakerReg.Add(m);
        }

        static public void DeRegisterExternalDisplayListEntryMaker(int typeIndex)
        {
            int indexToRemove = -1;
            for (int i = 0; i < dlMakerReg.Count; i++)
            {
                if (dlMakerReg[i].GetRendererComponent() == typeIndex)
                    indexToRemove = i;
            }
            if(indexToRemove > -1)
                dlMakerReg.RemoveAt(indexToRemove);
            else
                Assert.IsTrue(false,"Try to remove an unregistered display list entry for component id");
        }

        protected override void OnCreate()
        {
            var mgr = EntityManager;
            groupMain = mgr.CreateEntityQuery(ComponentType.ReadWrite<DisplayListEntry>(),
                ComponentType.ReadWrite<SortedEntity>(),
                ComponentType.ReadWrite<LocalToWorld>(),
                ComponentType.ReadWrite<Camera2D>(),
                ComponentType.ReadWrite<DisplayListCamera>());
            RegisterDisplayListEntryMaker(new MakeEntrySortingGroup());

            // default layers
            layersToComponentType[0] = ComponentType.ReadOnly<CameraLayer0>();
            layersToComponentType[1] = ComponentType.ReadOnly<CameraLayer1>();
            layersToComponentType[2] = ComponentType.ReadOnly<CameraLayer2>();
            layersToComponentType[3] = ComponentType.ReadOnly<CameraLayer3>();
            layersToComponentType[4] = ComponentType.ReadOnly<CameraLayer4>();
            layersToComponentType[5] = ComponentType.ReadOnly<CameraLayer5>();
            layersToComponentType[6] = ComponentType.ReadOnly<CameraLayer6>();
            layersToComponentType[7] = ComponentType.ReadOnly<CameraLayer7>();
            layersToComponentType[8] = ComponentType.ReadOnly<CameraLayer8>();
            layersToComponentType[9] = ComponentType.ReadOnly<CameraLayer9>();
            layersToComponentType[10] = ComponentType.ReadOnly<CameraLayer10>();
            layersToComponentType[11] = ComponentType.ReadOnly<CameraLayer11>();
            layersToComponentType[12] = ComponentType.ReadOnly<CameraLayer12>();
            layersToComponentType[13] = ComponentType.ReadOnly<CameraLayer13>();
            layersToComponentType[14] = ComponentType.ReadOnly<CameraLayer14>();
            layersToComponentType[15] = ComponentType.ReadOnly<CameraLayer15>();
            for (int i = 15; i < 32; i++)
                layersToComponentType[i] = default;
        }

        public void SetCameraLayerComponent<T>(int layerIndex)
        {
            if (layerIndex <= 15 || layerIndex >= 32)
                throw new ArgumentException("Only camera layers 16 through 31 can be set to custom components.");
            layersToComponentType[layerIndex] = ComponentType.ReadOnly<T>();
        }

        protected override void OnDestroy()
        {
            DeRegisterExternalDisplayListEntryMaker(TypeManager.GetTypeIndex<SortingGroup>());
            base.OnDestroy();
        }

        protected static int ClipBitsNorm(float2 p, float2 unitSize)
        {
            int r = 0;
            if (p.x < -unitSize.x)
                r |= 1;
            if (p.x > unitSize.x)
                r |= 2;
            if (p.y < -unitSize.y)
                r |= 4;
            if (p.y > unitSize.y)
                r |= 8;
            return r;
        }

        public static bool BoundsAreOutside(Rect r, float4x4 m, float2 unitSize)
        {
            // top left
            float2 p = new float2(r.x, r.y);
            float2 pc;
#if UNITY_USE_TINYMATH
            pc = tinymath.transform(m, p);
#else
            pc = math.transform(m, new float3(p.x, p.y, 0.0f)).xy;
#endif
            int clipbits = ClipBitsNorm(pc, unitSize);
            if (clipbits == 0)
                return false;
            // top right
            p = new float2(r.x + r.width, r.y);
#if UNITY_USE_TINYMATH
            pc = tinymath.transform(m, p);
#else
            pc = math.transform(m, new float3(p.x, p.y, 0.0f)).xy;
#endif
            clipbits &= ClipBitsNorm(pc, unitSize);
            if (clipbits == 0)
                return false;
            // bottom right
            p = new float2(r.x + r.width, r.y + r.height);
#if UNITY_USE_TINYMATH
            pc = tinymath.transform(m, p);
#else
            pc = math.transform(m, new float3(p.x, p.y, 0.0f)).xy;
#endif
            clipbits &= ClipBitsNorm(pc, unitSize);
            if (clipbits == 0)
                return false;
            // bottom left
            p = new float2(r.x, r.y + r.height);
#if UNITY_USE_TINYMATH
            pc = tinymath.transform(m, p);
#else
            pc = math.transform(m, new float3(p.x, p.y, 0.0f)).xy;
#endif
            clipbits &= ClipBitsNorm(pc, unitSize);
            if (clipbits == 0)
                return false;
            return true;
        }

        protected bool FinishDisplayListEntry(ref DisplayListEntry de, ref DisplayListCamera dlc,
            ref LocalToWorld tx, ref PrivateTransformData ptd, bool doNotClip)
        {
            de.inSortingGroup = ptd.inSortingGroup;
#if UNITY_USE_TINYMATH
            de.finalMatrix = tinymath.mul(dlc.inverseWorld, tx.Value);
#else
            de.finalMatrix = math.mul(dlc.inverseWorld, tx.Value);
#endif
            if (doNotClip)
                return true;
            // z-clip
            float z = de.finalMatrix[2][3];
            if (z < dlc.clipZNear || z > dlc.clipZFar)
                return false;
            // bounds clip
#if DEBUG
            if (!(de.inBounds.width > 0.0f) || !(de.inBounds.height > 0.0f)) {
                Debug.LogFormat("Entity {0} has zero or negative size ({1},{2})! This is checked in DEVELOPMENT builds only!", de.e, de.inBounds.width, de.inBounds.height);
                return false;
            }
#endif
            return true;
        }

        static uint PopCount(uint x)
        {
            uint r = 0;
            for (int i = 0; i < 32; i++)
                r += (x >> i) & 1;
            return r;
        }

        protected void AddLayerFilter(ref EntityQueryBuilder query, Camera2D c2d)
        {
            // temporary check until we support more in ForEach 
            Assert.IsTrue(PopCount(c2d.cullingMask) <= 14, "No more than 14 layer flags at the same time are supported per camera.");
            switch (c2d.cullingMode) {
                case CameraCullingMode.NoCulling:
                    Assert.IsTrue(c2d.cullingMask == 0, "Camera has NoCulling culling but mask bits set.");
                    break;
                case CameraCullingMode.All:
                    Assert.IsTrue(c2d.cullingMask != 0, "Camera has All culling but no mask bits are set.");
                    for ( int i=0; i<32; i++ )
                        if (((c2d.cullingMask>>i)&1)==1) query.WithAll(layersToComponentType[i]);
                    break;
                case CameraCullingMode.Any:
                    Assert.IsTrue(c2d.cullingMask != 0, "Camera has Any culling but no mask bits are set.");
                    for ( int i=0; i<32; i++ )
                        if (((c2d.cullingMask>>i)&1)==1) query.WithAny(layersToComponentType[i]);
                    break;
                case CameraCullingMode.None:
                    Assert.IsTrue(c2d.cullingMask != 0, "Camera has None culling but no mask bits are set.");
                    for ( int i=0; i<32; i++ )
                        if (((c2d.cullingMask>>i)&1)==1) query.WithNone(layersToComponentType[i]);
                    break;
            }
        }

        protected void AddItemsToListByType(IExternalDisplayListEntryMaker dlm, DisplayListCamera dlc, Camera2D c2d,
            DynamicBuffer<DisplayListEntry> dest, DynamicBuffer<SortedEntity> destSorted)
        {
            var mgr = EntityManager;
            dlm.Update(this);
            var cachedGetLayerSorting = GetComponentDataFromEntity<LayerSorting>(true);
            bool doNotClip = dlm.DoNotClip();
            var query = Entities;

            // build the query: Add base filter and dlm, then add per - camera filter
            dlm.Filter(ref query);
            // add camera 'layer' filter
            AddLayerFilter(ref query, c2d);

            // run query
            query.ForEach((Entity e, ref PrivateTransformData ptd, ref LocalToWorld tx) => 
            {
                DisplayListEntry de = default;
                de.e = e;
                if (!dlm.MakeEntry(mgr, ref de))
                    return;
                if (!FinishDisplayListEntry(ref de, ref dlc, ref tx, ref ptd, doNotClip))
                    return;
                float z = math.dot(de.finalMatrix[3], dlc.sortingDot); // = Dot(de.finalMatrix.GetColumn(3), dlc.camSortingDot); TODO CHECK
                SortedEntity se = new SortedEntity { 
                    e = de.e, 
                    idx = dest.Length 
                };
                if (cachedGetLayerSorting.Exists(de.e)) {
                    var sortEx = cachedGetLayerSorting[de.e];
                    se.CombineKey(z, sortEx.layer, sortEx.order);
                } else {
                    se.CombineKey(z);
                }
                se.e = de.e;
                se.idx = dest.Length;
                dest.Add(de);
                destSorted.Add(se);
            });
        }

        static float MaxAbsScale(float4x4 m)
        {
            float3 s = new float3(
                math.lengthsq(m.c0.xyz),
                math.lengthsq(m.c1.xyz),
                math.lengthsq(m.c2.xyz));
            return math.sqrt(math.cmax(s));
        }

        protected int UpdateOneDisplayListCamera(Entity e, ref Camera2D cam, ref DisplayListCamera dlcCam, ref LocalToWorld tx, float primaryAspect)
        {
            var mgr = EntityManager;

            // get list and sorted list buffers
            DynamicBuffer<DisplayListEntry> dest = mgr.GetBuffer<DisplayListEntry>(e);
            DynamicBuffer<SortedEntity> destSorted = mgr.GetBuffer<SortedEntity>(e);
            dest.Clear();
            destSorted.Clear();
#if DEBUG
            if (!(cam.rect.x >= 0.0f && cam.rect.y >= 0.0f && cam.rect.x + cam.rect.width <= 1.0f &&
                  cam.rect.y + cam.rect.height <= 1.0f)) {
                Debug.LogFormat("The camera {0} has an invalid rect field ({1},{2},{3},{4}). Fixing by clamping it to the unit rectangle (0,0,1,1) in DEVELOPMENT build only.",
                           e, cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                cam.rect.Clamp(new Rect(0, 0, 1, 1));
            }
            if (cam.rect.IsEmpty()) {
                Debug.LogFormat("The camera {0} has an empty rect field. Fixing by setting it to identity in DEVELOPMENT build only.", e);
                cam.rect = new Rect(0, 0, 1, 1);
            }
            if (cam.halfVerticalSize <= 0) {
                Debug.LogFormat("The camera {0} has an invalid halfVerticalSize size of {1}. Nothing will render for it.", e, cam.halfVerticalSize);
                return 0;
            }
            float mas = MaxAbsScale(tx.Value);
            if (!(mas > .99f && mas < 1.01f)) {
                Debug.LogFormat("The entity {0} with a Camera2D has a maximum absolute scaling factor of {1}. Cameras can not be scaled for rendering. Rendering and picking with this camera will likely be wrong.", 
                    e, mas);
            }
#endif
            // get camera sorting axis
            if (mgr.HasComponent<Camera2DAxisSort>(e)) {
                var axissort = mgr.GetComponentData<Camera2DAxisSort>(e);
                dlcCam.sortingDot.xyz = -axissort.axis;
                dlcCam.sortingDot.w = 0;
            } else {
                dlcCam.sortingDot.x = 0.0f;
                dlcCam.sortingDot.y = 0.0f;
                dlcCam.sortingDot.z = -1.0f;
                dlcCam.sortingDot.w = 0.0f;
            }
            // copy transform matrix
            dlcCam.world = tx.Value;
            dlcCam.inverseWorld = math.inverse(dlcCam.world);

            // initialize 2d clipping
            if (mgr.HasComponent<Camera2DRenderToTexture>(e))
            {
                var rtt = mgr.GetComponentData<Camera2DRenderToTexture>(e);
                float localAspect = (float)rtt.width / (float)rtt.height;
                dlcCam.clip2D.x = cam.halfVerticalSize * localAspect;
                dlcCam.clip2D.y = cam.halfVerticalSize;
            } else {
                dlcCam.clip2D.x = cam.halfVerticalSize * primaryAspect;
                dlcCam.clip2D.y = cam.halfVerticalSize;
            }

            // initialize near/far clipping
            if (mgr.HasComponent<Camera2DClippingPlanes>(e)) {
                var clipz = mgr.GetComponentData<Camera2DClippingPlanes>(e);
                dlcCam.clipZNear = clipz.near;
                dlcCam.clipZFar = clipz.far;
            } else {
                dlcCam.clipZNear = float.MinValue;
                dlcCam.clipZFar = float.MaxValue;
            }

#if DEBUG
            if (dlcCam.clipZNear >=  dlcCam.clipZFar) {
                Debug.LogFormat("The camera {0} has an invalid z clip range [{1}...{2}]. Nothing will render for it.", e, dlcCam.clipZNear, dlcCam.clipZFar);
                return 0;
            }
#endif

            // add all items
            for (int i = 0; i < dlMakerReg.Count; i++)
                AddItemsToListByType(dlMakerReg[i], dlcCam, cam, dest, destSorted);

            // sort in c++
            unsafe {
                SortExternal(dest.GetUnsafePtr(), destSorted.GetUnsafePtr(), dest.Length);
            }

            return dest.Length;
        }

        [DllImport("lib_unity_tiny_core2d", EntryPoint = "sortexternal")]
        static protected extern unsafe void SortExternal(void* sortedEntities, void* groups, int n);

        ComponentType [] layersToComponentType = new ComponentType[32];

        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb;
            var mgr = EntityManager;

            var env = World.TinyEnvironment();
            var config = env.GetConfigData<DisplayInfo>();
            float primaryAspect = (float)config.width / (float)config.height;

            // for every Camera2D that doesn't have a DisplayListCamera, add one, and the buffers it needs
            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithAll<Camera2D, LocalToWorld>()
                .WithNone<DisplayListCamera>()
                .ForEach(e =>
            {
                ecb.AddComponent(e, default(DisplayListCamera));
                ecb.AddBuffer<DisplayListEntry>(e);
                ecb.AddBuffer<SortedEntity>(e);
            });
            ecb.Playback(mgr);
            ecb.Dispose();

            // for every DisplayListCamera without a Camera2D, remove the DisplayListCamera
            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithAll<DisplayListCamera>()
                .WithNone<Camera2D>()
                .ForEach(e =>
            {
                ecb.RemoveComponent<DisplayListCamera>(e);
            });
            ecb.Playback(mgr);
            ecb.Dispose();
            // for every DisplayListCamera without a LocalToWorld, remove the DisplayListCamera
            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithAll<DisplayListCamera>()
                .WithNone<LocalToWorld>()
                .ForEach(e =>
            {
                ecb.RemoveComponent<DisplayListCamera>(e);
            });
            ecb.Playback(mgr);
            ecb.Dispose();

            // if we lost DisplayListCamera, also remove the buffers 
            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithAll<DisplayListEntry, SortedEntity>()
                .WithNone<DisplayListCamera>()
                .ForEach(e =>
            {
                ecb.RemoveComponent<DisplayListEntry>(e);
                ecb.RemoveComponent<SortedEntity>(e);
            });
            ecb.Playback(mgr);
            ecb.Dispose();

            // for each camera, build a sorted list of items to render
            int ncam = 0;
            int ndisp = 0;
            Entities.ForEach((Entity e, ref Camera2D c2d, ref DisplayListCamera dlc, ref LocalToWorld tx) => {
                ndisp += UpdateOneDisplayListCamera(e, ref c2d, ref dlc, ref tx, primaryAspect);
                ncam++;
            });
#if DEBUG
            if (ncam==0)
                Debug.Log("No camera entities found in world. There will be no visible rendering.");
            else if (ndisp==0)
                Debug.Log("No display entities found in world. There will be no visible rendering.");
#endif
        }
    }
}
