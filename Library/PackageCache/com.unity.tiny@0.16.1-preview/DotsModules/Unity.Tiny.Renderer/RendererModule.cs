using System;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using System.Runtime.InteropServices;

/**
 * @module
 * @name Unity.Tiny
 */
namespace Unity.Tiny.Rendering
{
    internal struct SortedCameraEntity : IComparable<SortedCameraEntity>
    {
        public Entity e;
        public float depth;

        public int CompareTo(SortedCameraEntity rhs)
        {
            if (depth == rhs.depth) return 0;
            return depth > rhs.depth ? 1 : -1;
        }
    }

    public abstract class RendererComponentSystem : ComponentSystem
    {
        // implement
        protected abstract void BeginScene(float2 targetSize);
        protected abstract void EndScene();

        protected abstract void BeginCamera(Entity eCam, float2 targetSize);
        protected abstract void EndCamera(Entity eCam);

        protected abstract void BeginRTT(Entity eCam, ref Camera2DRenderToTexture rtt);
        protected abstract void EndRTT(Entity eCam, ref Camera2DRenderToTexture rtt);

        protected abstract unsafe void RenderSpriteBatch(int n, DisplayListEntry* batch);

        // generic rendering code
        public const int sMaxCameras = 256;
        private NativeArray<SortedCameraEntity> sortedCameras;
        public const int sMaxBatchSize = 4096;
        private NativeArray<DisplayListEntry> sortedBatchBuffer;

        protected bool CanBatchWith(ref DisplayListEntry e0, ref DisplayListEntry ei)
        {
            var mgr = EntityManager;

            DisplayListEntryType det0 = e0.type;
            DisplayListEntryType deti = ei.type;
            if (det0 != deti)
                return false;

            // special case tiled and sliced for now, they never batch (can check others as well!)
            switch (det0) {
                default:
                case DisplayListEntryType.TiledSprite:
                case DisplayListEntryType.SlicedSprite:
                case DisplayListEntryType.Shape:
                case DisplayListEntryType.GroupOnly:
                case DisplayListEntryType.HitBoxOnly:
                case DisplayListEntryType.Tilemap:
                case DisplayListEntryType.Text:
                    return false;
                case DisplayListEntryType.Sprite: {
                    // check blend mode
                    Sprite2DRenderer sr0 = cachedGetSprite2DRenderer[e0.e];
                    Sprite2DRenderer sri = cachedGetSprite2DRenderer[ei.e];
                    if (sr0.blending != sri.blending)
                        return false;
                    // if we are depending on a texture, check that those match
                    Sprite2D s0 = cachedGetSprite2D[sr0.sprite];
                    Sprite2D si = cachedGetSprite2D[sri.sprite];
                    if (s0.image != si.image)
                        return false;
                    // good!
                    return true;
                }
            }
        }

        private ComponentDataFromEntity<Sprite2D> cachedGetSprite2D;
        private ComponentDataFromEntity<Sprite2DRenderer> cachedGetSprite2DRenderer;

        protected override void OnCreate()
        {
            base.OnCreate();
            sortedCameras = new NativeArray<SortedCameraEntity>(sMaxCameras, Allocator.Persistent);
            sortedBatchBuffer = new NativeArray<DisplayListEntry>(sMaxBatchSize, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            sortedCameras.Dispose();
            sortedBatchBuffer.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var mgr = EntityManager;

            var env = World.TinyEnvironment();
            var config = env.GetConfigData<DisplayInfo>();
            float2 targetSize = new float2(config.framebufferWidth, config.framebufferHeight);

            if (targetSize.x <= 0.0f || targetSize.y <= 0.0f)
                return;

            BeginScene(targetSize);

            // gather all cameras
            int nSortedCamera = 0;
            Entities.ForEach((Entity e, ref Camera2D cam, ref DisplayListCamera dlc) =>
                {
                    if (nSortedCamera<sMaxCameras)
                        sortedCameras[nSortedCamera++] = new SortedCameraEntity {e = e, depth = cam.depth};
                });

            if (nSortedCamera > 1)
            {
                var slice = new NativeSlice<SortedCameraEntity>(sortedCameras, 0, nSortedCamera);
                slice.Sort();
            }

            cachedGetSprite2DRenderer = GetComponentDataFromEntity<Sprite2DRenderer>();
            cachedGetSprite2D = GetComponentDataFromEntity<Sprite2D>();

            for (int j = 0; j < nSortedCamera; j++)
            {
                var cam = sortedCameras[j];
                bool inRtt;
                Camera2DRenderToTexture rtt;
                if (mgr.HasComponent<Camera2DRenderToTexture>(cam.e))
                {
                    rtt = mgr.GetComponentData<Camera2DRenderToTexture>(cam.e);
                    // fixup none rtt target to self
                    if (rtt.target == Entity.Null)
                        rtt.target = cam.e;

                    inRtt = true;
                    BeginRTT(cam.e, ref rtt);
                    BeginCamera(cam.e, new float2(rtt.width, rtt.height));
                }
                else
                {
                    rtt = default(Camera2DRenderToTexture);
                    inRtt = false;
                    BeginCamera(cam.e, targetSize);
                }
                // gather items
                var dc = mgr.GetComponentData<DisplayListCamera>(cam.e);
                var displayList = mgr.GetBuffer<DisplayListEntry>(cam.e);
                var sorted = mgr.GetBuffer<SortedEntity>(cam.e);

                if (sorted.Length>0) {
                    unsafe
                    {
                        DisplayListEntry *displayListArray = (DisplayListEntry *)displayList.AsNativeArray().GetUnsafeReadOnlyPtr();
                        SortedEntity *sortedArray = (SortedEntity *)sorted.AsNativeArray().GetUnsafeReadOnlyPtr();
                        int nSorted = sorted.Length;
                        DisplayListEntry *sortedBatch = (DisplayListEntry *)sortedBatchBuffer.GetUnsafePtr();

                        // render batches
                        int n = 1;
                        sortedBatch[0] = displayListArray[sortedArray[0].idx];
                        //Assert(verifyDisplayListEntry(man, mSortedbatch[0].e, mSortedbatch[0].type));
                        for (int i = 1; i < nSorted; i++)
                        {
                            DisplayListEntry denew = displayList[sorted[i].idx];
                            //Assert(verifyDisplayListEntry(man, denew.e, denew.type));
                            //Assert(sorted[i].e == denew.e);
                            if (n < sMaxBatchSize && CanBatchWith(ref sortedBatch[0], ref denew))
                            {
                                sortedBatch[n] = denew;
                                n++;
                            }
                            else
                            {
                                // draw batch, all entries have the same DisplayEntryType and passed a canBatchWith test
                                RenderSpriteBatch(n, sortedBatch);
                                sortedBatch[0] = denew;
                                n = 1;
                            }
                        }
                        if (n > 0) // draw final batch
                            RenderSpriteBatch(n, sortedBatch);
                    }
                }
                EndCamera(cam.e);
                if ( inRtt)
                    EndRTT(cam.e, ref rtt);
            }
            EndScene();
        }
    }

}
