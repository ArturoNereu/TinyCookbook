using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;

namespace Unity.Tiny.Core2D {
    /// <summary>
    /// A component that describes the framerate of a list of sprites for animation.
    /// Place in the same entity as a <see cref="Sprite2DSequence"/>.
    /// </summary>
    [IdAlias("95eba163dce5bfff5420a5a8c2845523")]
    public struct Sprite2DSequenceOptions : IComponentData
    {
        /// <summary>
        /// Base frame rate of the sequence, in frames per second.
        /// The default frame rate is 20, if the <see cref="Sprite2DSequence"/> does not exist.
        /// </summary>
        public float frameRate;
    }

    /// <summary>
    /// List of sprite entities, which are required to have <see cref="Sprite2D"/> components.
    /// Add a <see cref="Sprite2DSequence"/> for additional options.
    /// </summary>
    public struct Sprite2DSequence : IBufferElementData
    {
        [EntityWithComponents(typeof(Sprite2D))]
        public Entity e;
    }

    /// <summary>
    ///  A component that is used by the <see cref="SequencePlayerSystem"/> to play
    ///  play back a sequence of sprites when applied to an entity that also has
    ///  the <see cref="Sprite2DRenderer"/> component.
    /// </summary>
    [IdAlias("d2cfd18aa4facc33b1da7ae05205691b")]
     public struct Sprite2DSequencePlayer : IComponentData
     {
        /// <summary> Sequence entity, required to have <see cref="Sprite2DSequence"/> component </summary>
        [EntityWithComponents(typeof(Sprite2DSequence))]
        public Entity sequence;

        /// <summary> Speed multiplier for playback, defaults to 1.</summary>
        public float speed;

        /// <summary> Current time for playback, 0 to infinity.</summary>
        public float time;

        /// <summary> Set to true to pause animation sequence. Set back to false to continue animation. Default false. </summary>
        public bool paused;

        /// <summary>
        /// Sets the looping behavior of the animation. Defaults to Loop.
        /// </summary>
        /// <remarks>
        /// <list>
        ///  <item> Loop: Play the sequence in a loop forever [A][B][C][A][B][C][A][B][C]... </item>
        ///  <item> Once: Play the sequence once [A][B][C] then pause and set time to 0 </item>
        ///  <item> PingPong: Play the sequence in a ping pong loop forever [A][B][C][B][A][B][C][B]... </item>
        ///  <item> PingPongOnce: Play the sequence once forward then back to the start [A][B][C][B][A] then pause and set time to 0 </item>
        ///  <item> ClampForever: Plays back the animation once, [A][B][C]. When it reaches the end, it will keep playing the last frame and never stop playing. </item>
        /// </list>
        /// </remarks>
        public LoopMode loop;
    }

    ///  A system used to drive SequencePlayer components.
    ///  Required to be scheduled in order for sequences to play.
    ///
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    internal class SequencePlayerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var mgr = EntityManager;
            var env = World.TinyEnvironment();
            float dt = env.frameDeltaTime;

            Entities.ForEach((Entity e, ref Sprite2DRenderer r, ref Sprite2DSequencePlayer p) =>
            {
                if (!mgr.Exists(p.sequence))
                {
                    Debug.LogFormat(
                        "The sequence player on entity {0} references a sprite sequence that does not exist.",
                        e);
                    return;
                }

                if (!mgr.HasComponent<Sprite2DSequence>(p.sequence))
                {
                    Debug.LogFormat(
                        "The sequence player on entity {0} references a sprite sequence {1} that does not have a Sprite2DSequence buffer component.",
                        e, p.sequence);
                    return;
                }

                var sq = mgr.GetBuffer<Sprite2DSequence>(p.sequence);
                int ns = (int)sq.Length;
                if (ns <= 0)
                {
                    Debug.LogFormat(
                        "The sprite sequence on entity {0} has an empty sprite sequence, playing it has no effect.",
                        p.sequence);
                    return;
                }

                if (!p.paused)
                    p.time += dt * p.speed;
                if (!(p.time >= 0.0f))
                    p.time = 0.0f;
                float frameRate = 20.0f;
                if (mgr.HasComponent<Sprite2DSequenceOptions>(p.sequence))
                {
                    var so = mgr.GetComponentData<Sprite2DSequenceOptions>(p.sequence);
                    frameRate = so.frameRate;
#if DEBUG
                    if (!(frameRate > 0.0f))
                    {
                        Debug.LogFormat(
                            "The sprite sequence in on entity {0} is assigning a bad ({1}) frame rate. Setting to 20 in DEVELOPMENT build only.",
                            p.sequence, frameRate);
                        frameRate = 20.0f;
                    }
#endif
                }
                int idx = (int) (p.time * frameRate);
                switch (p.loop)
                {
                    case LoopMode.Loop:
                    default:
                        idx %= ns;
                        break;
                    case LoopMode.Once:
                        if (idx >= ns)
                        {
                            idx = ns - 1;
                            p.paused = true;
                            p.time = 0;
                        }

                        break;
                    case LoopMode.PingPong:
                        if (ns == 1)
                        {
                            idx = 0;
                        }
                        else
                        {
                            idx %= ns * 2 - 2;
                            if (idx >= ns)
                                idx = ns * 2 - 2 - idx;
                        }

                        break;
                    case LoopMode.PingPongOnce:
                        if (idx >= ns)
                            idx = ns * 2 - 2 - idx;
                        if (idx < 0)
                        {
                            idx = 0;
                            p.paused = true;
                            p.time = 0;
                        }

                        break;
                    case LoopMode.ClampForever:
                        if (idx >= ns)
                            idx = ns - 1;
                        break;
                }
                r.sprite = sq[idx].e;

#if DEBUG
                if (!mgr.Exists(r.sprite)) {
                    Debug.LogFormat( "The sequence player on entity {0} is assigning a non existing sprite entity. Setting to NONE in DEVELOPMENT build only.",
                        e );
                    r.sprite = Entity.Null;
                } else if (!mgr.HasComponent<Sprite2D>(r.sprite)) {
                    Debug.LogFormat ( "The sequence player on entity {0} is assigning an entity without a Sprite2D component. Setting to NONE in DEVELOPMENT build only.",
                        e );
                    r.sprite = Entity.Null;
                }
#endif
            });
        }
    }
}
