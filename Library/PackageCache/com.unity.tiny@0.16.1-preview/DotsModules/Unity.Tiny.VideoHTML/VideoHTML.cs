using System;
using System.Runtime.InteropServices;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny;
using Unity.Tiny.Core;
using Unity.Tiny.UILayout;
using Unity.Tiny.Core2D;
using Unity.Tiny.Video;
using Unity.Tiny.Debugging;

[assembly: ModuleDescription("Unity.Tiny.VideoHTML", "VideoHTML module")]
[assembly: IncludedPlatform(Platform.Web | Platform.WeChat | Platform.FBInstant)]
namespace Unity.Tiny.HTML
{
    // StateComponent required for implementing IGenericAssetLoader
    [HideInInspector]
    public struct VideoClipLoading : ISystemStateComponentData
    {
#pragma warning disable CS0169
        private int dummy;
#pragma warning restore CS0169
    }

    [HideInInspector]
    public struct VideoClipLoadingState : ISystemStateComponentData
    {
        public int index;
    }

    // Component that caches the mode a video clip has too be played. The video HTML API requires to specify loop mode and show control mode at loading time
    [HideInInspector]
    internal struct VideoClipHTML : ISystemStateComponentData
    {
        public bool loop;
        public bool controls;
        public int index;
        public Rect region;
    }

    static class VideoHTMLNativeCalls
    {
        [DllImport("__Internal")]
        public static extern int js_create_video_element(string src, bool controls, bool loop, int left, int top, int width, int height);

        [DllImport("__Internal")]
        public static extern int js_check_loading(int index);

        [DllImport("__Internal")]
        public static extern void js_initialize();

        [DllImport("__Internal")]
        public static extern double js_getCurrentTime(int index);

        [DllImport("__Internal")]
        public static extern int js_check_isPlaying(int index);

        [DllImport("__Internal")]
        public static extern void js_remove_video_element(int index);
    }

    class VideoHTMLAssetLoader : IGenericAssetLoader<VideoClip, VideoClipLoadingState, VideoClipLoadFromFile,
        VideoClipLoading>
    {
        
        public void StartLoad(EntityManager em, Entity e, ref VideoClip vp, ref VideoClipLoadingState ps,
            ref VideoClipLoadFromFile clip, ref VideoClipLoading unused)
        {
            if (em.HasComponent<VideoClipHTML>(e))
            {
                if (em.HasComponent<VideoClipLoadFromFileName>(e))
                {
                    var src = em.GetBufferAsString<VideoClipLoadFromFileName>(e);
                    var clipHTML = em.GetComponentData<VideoClipHTML>(e);
                    int index = VideoHTMLNativeCalls.js_create_video_element(src, clipHTML.controls, clipHTML.loop, (int)clipHTML.region.x, (int)clipHTML.region.y, (int)clipHTML.region.width, (int)clipHTML.region.height);
                    clipHTML.index = index;
                    ps.index = index;
                }
                else
                {
                    var s = "Missing VideoClipLoadFromFileName component on entity ";
                    s += e.ToString();
                    Debug.Log(s);
                }
            }
        }

        public LoadResult CheckLoading(IntPtr cppwrapper, EntityManager em, Entity e, ref VideoClip vp, ref VideoClipLoadingState vps,
            ref VideoClipLoadFromFile clip, ref VideoClipLoading unused)
        {
            int res = VideoHTMLNativeCalls.js_check_loading(vps.index);
            if (res == 0) // Error loading
                return LoadResult.failed;
            else if (res == 2)
            {
                var src = em.GetBufferAsString<VideoClipLoadFromFileName>(e);
                var s = "Loaded video: ";
                s += src;
                Debug.Log(s);
                return LoadResult.success;
            }
            return LoadResult.stillWorking; //1 or -1
        }

        public void FreeNative(EntityManager em, Entity e, ref VideoClipLoadingState vps)
        {
            // nothing to do
        }

        public void FinishLoading(EntityManager em, Entity e, ref VideoClip vp, ref VideoClipLoadingState vps, ref VideoClipLoading unsed)
        {
            // nothing to do
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VideoHTMLSystem))]
    public class VideoHTMLAssetSystem : GenericAssetLoader<VideoClip, VideoClipLoadingState, VideoClipLoadFromFile,
        VideoClipLoading>
    {

        protected override void OnUpdate()
        {
            //We need to wait VideoHTMLSystem.Update to have initialized videoclip with options(loop, controls, region) from the player before loading
            if (c == null)
                c = new VideoHTMLAssetLoader();
            base.OnUpdate();
        }
    }

    /// <summary>
    ///  Video system required to play and remove video.
    ///  If the entity has a VideoPlayerAutoDeleteOnEnd component
    ///  this system deletes the video after playback unless player.loop is true
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VideoSystem))]
    public class VideoHTMLSystem : ComponentSystem
    {
        private bool initialized = false;

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            if (initialized)
                return;

            VideoHTMLNativeCalls.js_initialize();
            initialized = true;
        }

        protected override void OnUpdate()
        {
            EntityManager em = EntityManager;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            //Init: Add VideoClipHTML private comp to a clip added to the player
            Entities.ForEach((Entity e, ref VideoPlayer vp) => {
                if (em.Exists(vp.clip))
            {
                if (!em.HasComponent<VideoClipHTML>(vp.clip))
                {
                    float2 videoSize;
                    int left, top, width, height;
                    if (em.HasComponent<RectTransform>(e))
                    {

                        float3 worldPos = TransformHelpers.ComputeWorldPosition(this,e);
                        videoSize = UILayoutService.GetRectTransformSizeOfEntity(this, e);
                        left = (int)(worldPos.x - (videoSize.x / 2.0f));
                        top = (int)(worldPos.y - (videoSize.y / 2.0f));
                    }
                    else
                    {
                        //Without rectT the size of the video will be the screen size;
                        videoSize = UILayoutService.GetScreenSize(this);
                        left = 0;
                        top = 0;
                    }
                    width = (int)videoSize.x;
                    height = (int)videoSize.y;
                    ecb.AddComponent(vp.clip, new VideoClipHTML()
                    {
                        loop = vp.loop,
                        controls = vp.controls,
                        region = new Rect(left, top, width, height)
                    });
                    //Now that we have all info for the loading, restart loading
                    if(em.HasComponent<VideoClipLoading>(vp.clip))
                        ecb.RemoveComponent<VideoClipLoading>(vp.clip);
                }
            }
            });
            ecb.Playback(em);
            ecb.Dispose();
            ecb = new EntityCommandBuffer(Allocator.Temp);

            //Once the video is loaded (VideoClip has been removed), get the current time or remove the video if VideoPlayerAutoDeleteOnEnd has been attached
            Entities.WithAll<VideoPlayer>().ForEach((Entity e) =>
            {
                var vp = em.GetComponentData<VideoPlayer>(e);
                if (em.Exists(vp.clip) && em.HasComponent<VideoClipHTML>(vp.clip))
                {
                    var clipHtml = em.GetComponentData<VideoClipHTML>(vp.clip);

                    int playingStatus = VideoHTMLNativeCalls.js_check_isPlaying(clipHtml.index);
                    if (em.HasComponent<VideoPlayerAutoDeleteOnEnd>(e) && playingStatus == 2 && !vp.loop)
                    {
                        ecb.DestroyEntity(vp.clip);
                    }
                    else if (playingStatus == 1)
                    {
                        vp.currentTime = VideoHTMLNativeCalls.js_getCurrentTime(clipHtml.index);
                    }
                }
            });
            ecb.Playback(em);
            ecb.Dispose();
            ecb = new EntityCommandBuffer(Allocator.Temp);

            //Remove Video HTML element
            Entities.WithNone<VideoClip>().ForEach(
                (Entity e, ref VideoClipHTML v) => {
                    VideoHTMLNativeCalls.js_remove_video_element(v.index);
                });
            ecb.Playback(em);
            ecb.Dispose();

            //Remove VideoClipHTML priv compo on clip entities
            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities.WithAll<VideoClipHTML>().WithNone<VideoClip>().ForEach(e => {
                    ecb.RemoveComponent<VideoClipHTML>(e);
                });
            ecb.Playback(em);
            ecb.Dispose();
        }
    }
}

