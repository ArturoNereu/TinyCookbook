using Unity.Entities;
using Unity.Tiny;
using Unity.Tiny.Core2D;
using Unity.Collections;
using System;
using Unity.Authoring.Core;

[assembly: ModuleDescription("Unity.Tiny.Video", "HTML5-based Video playback")]
[assembly: IncludedPlatform(Platform.Web | Platform.WeChat | Platform.FBInstant)]
namespace Unity.Tiny.Video
{

    /// <summary>
    ///  Attach this component to an entity with an <see cref="VideoClip"/> and <see cref="VideoClipLoadFromFileName"/>
    ///  component to begin loading a video.
    /// </summary>
    /// <remarks>
    ///  Loading is performed by the VideoSystem.
    ///  Once loading is complete the VideoSystem removes the
    ///  VideoClipLoadFromFile component.
    /// </remarks>
    public struct VideoClipLoadFromFile : IComponentData
    {
#pragma warning disable CS0169
        private int dummy;
#pragma warning restore CS0169
    }

    /// <summary>
    ///  The buffer containing the path to the media source file.
    /// </summary>
    /// <remarks>
    ///  This can be a URL if the file is remote, or
    ///  it can be the path of the source file in the project.
    ///  HTML media types supported: MP4, WebM, Ogg.
    /// </remarks>
    public struct VideoClipLoadFromFileName : IBufferElementData
    {
        public char c;
    }

    /// <summary>
    /// A VideoClip represents a video resource that can be played back
    /// on a <see cref="VideoPlayer"/>.
    /// </summary>
    /// <remarks>
    /// An entity with a VideoClip should also have a <see cref="VideoClipLoadFromFileName"/>
    /// (to provide the path the media source) and then a <see cref="VideoClipLoadFromFile"/>
    /// to initiate the actual load.
    /// </remarks>
    [HideInInspector]
    public struct VideoClip : IComponentData
    {
#pragma warning disable CS0169
        private int dummy;
#pragma warning restore CS0169
    }

    /// <summary>
    ///  Add this component to an entity to create a video player.
    /// </summary>
    /// <remarks>
    ///  This component is required to play a video clip, and to set options such
    ///  as whether or not to display the video player controls, whether to loop the
    ///  video, etc.
    /// </remarks>
    public struct VideoPlayer : IComponentData
    {
        public static VideoPlayer Default { get; } = new VideoPlayer()
        {
            clip = Entity.Null,
            controls = true,
            loop = false,
            currentTime = 0.0f
        };
        /// <summary>
        ///  The clip entity. Attach a <see cref="VideoClip"/> to it to play a video.
        ///  The video is muted by default.
        /// </summary>
        [EntityWithComponents(typeof(VideoClip))]
        public Entity clip;

        /// <summary>
        ///  If true, video controls (Play/Pause/FullScreen/Volume) are displayed.
        /// </summary>
        public bool controls;

        /// <summary>
        ///  If true, the player automatically seeks back to the start of the
        ///  video after reaching the end.
        ///  This attribute is ignored if the entity has a <see cref="VideoPlayerAutoDeleteOnEnd"/> component.
        /// </summary>
        public bool loop;

        /// <summary> Current playback time, in seconds </summary>
        [HideInInspector]
        public double currentTime;
    }

    /// <summary>
    ///  Add this component to an entity with a <see cref="VideoPlayer"/> component to auto-delete
    ///  a video once it reaches the end.
    /// </summary>
    public struct VideoPlayerAutoDeleteOnEnd : IComponentData
    {
    }

    public class VideoSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            EntityManager em = EntityManager;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            //Init: Add Missing transform components
            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities.WithAll<VideoPlayer>().WithNone<Parent>().ForEach(e => {
                ecb.AddComponent(e, default(Parent));
            });
            ecb.Playback(em);
            ecb.Dispose();
            ecb = new EntityCommandBuffer(Allocator.Temp);

            Entities.WithAll<VideoPlayer>().WithNone<Translation>().ForEach(e => {
                ecb.AddComponent(e, default(Translation));
            });
            ecb.Playback(em);
            ecb.Dispose();
        }
    }

}
