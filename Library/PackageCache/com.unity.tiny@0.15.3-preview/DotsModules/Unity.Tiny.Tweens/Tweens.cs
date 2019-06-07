using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;


namespace Unity.Tiny.Tweens
{
    /// <summary>
    ///  Enum that lists tweening functions that you can pass to <see cref="TweenSystem.AddTween"/>.
    /// </summary>
    public enum TweenFunc
    {
        // not a tween, but playing an animation
        External,
        // the classics
        Linear,
        Hardstep,
        Smoothstep,
        Cosine,
        // from js version
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,
        InBack,
        OutBack,
        InOutBack,
        InBounce,
        OutBounce,
        InOutBounce,
        // some extra ones
        InCircle,
        OutCircle,
        InOutCircle,
        InExponential,
        OutExponential,
        InOutExponential
    }

    /// <summary>
    /// Structure that describes a single tween. It is part of the TweenComponent, and is
    /// automatically created by <see cref="TweenSystem.AddTween"/>
    /// </summary>
    public struct TweenDesc
    {
        internal int typeIndex;
        internal int byteOffset;

        // Description
        /// <summary>
        ///  Duration of tween, in seconds.
        /// </summary>
        public float duration;

        /// <summary>
        ///  The tweening function to use, as defined by <see cref="TweenFunc"/>
        /// </summary>
        public TweenFunc func;

        /// <summary>
        ///  The looping behavior to use.
        /// </summary>
        public LoopMode loop;

        /// <summary>
        ///  Current time of tween, in seconds.
        /// </summary>
        /// <remarks>
        /// Negative if the tween has not started.
        ///  Setting a negative value delays the start of the tween.
        ///  You can chain multiple tweens together by having them start at different offsets.
        /// </remarks>
        public float time;

        /// <summary>
        ///  If true, destroys the tweening entity (not the target entity) when
        ///  the tweening operation ends.
        /// </summary>
        public bool destroyWhenDone;
    }

    /// <summary>
    ///  Data describing an active tween. Created by <see cref="TweenSystem.AddTween"/>
    ///
    ///  Once the tween is created, you can use callbacks in the Watcher module to
    ///  watch for the current state of the tweening operation.
    ///
    ///  You can dynamically change values inside the TweenComponent at any time.
    ///
    /// </summary>
    public struct TweenComponent : IComponentData
    {
        /// <summary>
        /// Entity target to tween.
        /// </summary>
        public Entity target;

        /// <summary>
        /// Description of the tween, including the non-normalized time.
        /// </summary>
        public TweenDesc desc;

        /// <summary>
        ///  Number of times the tween has looped.
        ///  You can use the Watcher module to watch this, and trigger a callback
        ///  every loop.
        /// </summary>
        public int loopCount;

        /// <summary>
        ///  Current time of tween, normalized to [0..1] and with tween function applied.
        ///  Only valid when the tween is running.
        /// </summary>
        public float normalizedTweenTime;

        /// <summary>
        ///  True if the tween has started playing (t>0)
        ///  This can be watched using the Watcher module.
        /// </summary>
        public bool started;

        /// <summary>
        ///  True if the tween has stopped playing (t>duration).
        /// </summary>
        /// <remarks>
        ///  Never true if looping is continuous.
        ///  You can use the Watcher module to watch this, and trigger a callback
        ///  when the tween is finished playing.
        /// </remarks>
        public bool ended;
    }

    /// <summary>
    /// An active tween of an integer type.
    /// </summary>
    /// <remarks>
    /// The <see cref="TweenSystem.AddTween"/> function on the TweenSystem creates an entity with
    /// the type-specific Tween component and the generic TweenComponent.
    ///
    ///  You can dynamically change values inside the TweenComponent at any time.
    /// </remarks>
    public struct TweenInt : IComponentData
    {
        public int start;
        public int end;
    }

    /// <summary>
    ///  An active tween of a float type.
    /// </summary>
    /// <seealso cref="Unity.Tiny.Tweens.TweenInt"/>
    public struct TweenFloat : IComponentData
    {
        public float start;
        public float end;
    }

    /// <summary>
    ///  An active tween of a float2 type.
    /// </summary>
    /// <seealso cref="Unity.Tiny.Tweens.TweenInt"/>
    public struct TweenFloat2 : IComponentData
    {
        public float2 start;
        public float2 end;
    }

    /// <summary>
    ///  An active tween of a float3 type.
    /// </summary>
    /// <seealso cref="Unity.Tiny.Tweens.TweenInt"/>
    public struct TweenFloat3 : IComponentData
    {
        public float3 start;
        public float3 end;
    }

    /// <summary>
    ///  An active tween of a float4 type.
    /// </summary>
    /// <seealso cref="Unity.Tiny.Tweens.TweenInt"/>
    public struct TweenFloat4 : IComponentData
    {
        public float4 start;
        public float4 end;
    }

    /// <summary>
    ///  An active tween of a quaternion type.
    /// </summary>
    /// <seealso cref="Unity.Tiny.Tweens.TweenInt"/>
    public struct TweenQuaternion : IComponentData
    {
        public quaternion start;
        public quaternion end;
    }

    /// <summary>
    ///  An active tween of a Color type.
    /// </summary>
    /// <seealso cref="Unity.Tiny.Tweens.TweenInt"/>
    public struct TweenColor : IComponentData
    {
        public Color start;
        public Color end;
    }

    /// <summary>
    ///  A lightweight tweening system for component data.
    /// </summary>
    /// <remarks>
    ///  Tweening makes it possible to easily change single values inside components
    ///  over time without using a full animation system.
    /// </remarks>
    public class TweenSystem : ComponentSystem
    {
        // ------------------------------------------------ tween functions -------------------------------------------------
        private static float OutBounce(float t)
        {
            if (t < (1.0f / 2.75f)) {
                return t * t * 7.5625f;
            } else if (t < (2.0f / 2.75f)) {
                t = t - (1.5f / 2.75f);
                return t * t * 7.5625f + 0.75f;
            } else if (t < (2.5f / 2.75f)) {
                t = t - (2.25f / 2.75f);
                return t * t * 7.5625f + 0.9375f;
            }
            t = t - (2.625f / 2.75f);
            return t * t * 7.5625f + 0.984375f;
        }

        private static float InBounce(float t)
        {
            return 1.0f - OutBounce(1.0f - t);
        }

        private static float EaseFunc(float t, TweenFunc f)
        {
            switch (f) {
            case TweenFunc.Smoothstep:
                return t * t * (3.0f - 2.0f * t);
            case TweenFunc.Cosine:
                return 0.5f - math.cos(t * 3.1415f) * 0.5f; // almost the exact same as smoothstep
            case TweenFunc.Hardstep:
                if (t < .5f)
                    return 0;
                return 1.0f;
            default:
            case TweenFunc.Linear:
                return t;
            case TweenFunc.InQuad:
                return t * t;
            case TweenFunc.OutQuad:
                return t * (2.0f - t);
            case TweenFunc.InOutQuad:
                return t < .5f ? 2.0f * t * t : -1.0f + (4.0f - 2.0f * t) * t;
            case TweenFunc.InCubic:
                return t * t * t;
            case TweenFunc.OutCubic:
                t -= 1.0f;
                return t * t * t + 1.0f;
            case TweenFunc.InOutCubic:
                return t < .5f ? 4.0f * t * t * t : (t - 1.0f) * (2.0f * t - 2.0f) * (2.0f * t - 2.0f) + 1.0f;
            case TweenFunc.InQuart:
                return t * t * t * t;
            case TweenFunc.OutQuart:
                t -= 1.0f;
                return 1.0f - t * t * t * t;
            case TweenFunc.InOutQuart:
                if (t < .5f)
                    return 8.0f * t * t * t * t;
                t -= 1.0f;
                return 1.0f - 8.0f * t * t * t * t;
            case TweenFunc.InQuint:
                return t * t * t * t * t;
            case TweenFunc.OutQuint:
                t -= 1.0f;
                return 1.0f + t * t * t * t * t;
            case TweenFunc.InOutQuint:
                if (t < .5f)
                    return 16.0f * t * t * t * t * t;
                t -= 1.0f;
                return 1.0f + 16.0f * t * t * t * t * t;
            case TweenFunc.InBack: {
                const float s = 1.70158f;
                return t * t * ((s + 1.0f) * t - s);
            }
            case TweenFunc.OutBack: {
                const float s = 1.70158f;
                t -= 1.0f;
                return t * t * ((s + 1) * t + s) + 1;
            }
            case TweenFunc.InOutBack: {
                const float s = 1.70158f * 1.525f;
                if ((t *= 2.0f) < 1.0f)
                    return 0.5f * (t * t * ((s + 1.0f) * t - s));
                t -= 2.0f;
                return 0.5f * (t * t * ((s + 1.0f) * t + s) + 2.0f);
            }
            case TweenFunc.InBounce:
                return InBounce(t);
            case TweenFunc.OutBounce:
                return OutBounce(t);
            case TweenFunc.InOutBounce:
                t *= 2.0f;
                if (t < 1.0f)
                    return 0.5f * InBounce(t);
                return 0.5f * OutBounce(t - 1.0f) + 0.5f;
            case TweenFunc.InCircle:
                return -(math.sqrt(1.0f - t * t) - 1.0f);
            case TweenFunc.OutCircle:
                t -= 1.0f;
                return math.sqrt(1.0f - t * t);
            case TweenFunc.InOutCircle:
                t *= 2.0f;
                if (t < 1.0f)
                    return -.5f * (math.sqrt(1.0f - t * t) - 1.0f);
                t -= 2.0f;
                return .5f * (math.sqrt(1.0f - t * t) + 1.0f);
            case TweenFunc.InExponential:
                return math.exp2(10.0f * (t - 1.0f));
            case TweenFunc.OutExponential:
                return -math.exp2(-10.0f * t) + 1.0f;
            case TweenFunc.InOutExponential:
                t *= 2.0f;
                if (t < 1.0f)
                    return .5f * math.exp2(10.0f * (t - 1.0f));
                t -= 1.0f;
                return .5f * (-math.exp2(-10.0f * t) + 2.0f);
            }
        }

        private static float LoopTime(float t, float length, LoopMode mode, ref bool done, ref int loopCount)
        {
            Assert.IsTrue(t >= 0.0f && length > 0.0f);
            loopCount = (int)(t / length);
            switch (mode) {
                default:
                case LoopMode.Once:
                    if (t > length) {
                        done = true;
                        loopCount = 1;
                        return length;
                    }
                    loopCount = 0;
                    done = false;
                    return t;
                case LoopMode.Loop:
                    done = false;
                    return math.fmod(t, length);
                case LoopMode.PingPongOnce:
                    if (t > length * 2.0f) {
                        done = true;
                        loopCount = 2;
                        return 0.0f;
                    }
                    goto case LoopMode.PingPong;
                case LoopMode.PingPong: {
                    done = false;
                    float l2 = math.fmod(t, length * 2.0f);
                    if (l2 > length)
                        return 2.0f * length - l2;
                    return l2;
                }
                case LoopMode.ClampForever: {
                    done = false;
                    if (t > length) {
                        loopCount = 1;
                        return length;
                    }
                    return t;
                }
            }
        }

        /// <summary>
        ///  Helper function that removes all tween entities that reference target entity.
        /// </summary>
        public void RemoveAllTweens(Entity e)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            // update all tweens
            Entities.ForEach((Entity etw, ref TweenComponent tc) =>
            {
                if (tc.target == e)
                    ecb.DestroyEntity(etw);
            });
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        /// <summary>
        ///  Helper function that removes all tween entities in the world.
        ///  You can use it to clean up after a level.
        /// </summary>
        public void RemoveAllTweensInWorld()
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            // update all tweens
            Entities.ForEach((Entity etw, ref TweenComponent tc) =>
            {
                ecb.DestroyEntity(etw);
            });
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        /// <summary>
        ///  Helper function that removes all tween entities that have stopped playing.
        ///  You can use it to clean up after a level or transition sequence.
        /// </summary>
        public void RemoveAllEndedTweens()
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            // update all tweens
            Entities.ForEach((Entity etw, ref TweenComponent tc) =>
            {
                if (tc.ended)
                    ecb.DestroyEntity(etw);
            });
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private unsafe void WriteInt(Entity target, int typeidx, int offset, int x)
        {
            byte* ptr = EntityManager.GetComponentDataWithTypeRW(target, typeidx) + offset;
            *((int*)ptr) = x;
        }

        private unsafe void WriteFloat(Entity target, int typeidx, int offset, float x)
        {
            byte* ptr = EntityManager.GetComponentDataWithTypeRW(target, typeidx) + offset;
            *((float*)ptr) = x;
        }

        private unsafe void WriteFloat2(Entity target, int typeidx, int offset, float2 x)
        {
            byte* ptr = EntityManager.GetComponentDataWithTypeRW(target, typeidx) + offset;
            *((float2*)ptr) = x;
        }

        private unsafe void WriteFloat3(Entity target, int typeidx, int offset, float3 x)
        {
            byte* ptr = EntityManager.GetComponentDataWithTypeRW(target, typeidx) + offset;
            *((float3*)ptr) = x;
        }

        private unsafe void WriteFloat4(Entity target, int typeidx, int offset, float4 x)
        {
            byte* ptr = EntityManager.GetComponentDataWithTypeRW(target, typeidx) + offset;
            *((float4*)ptr) = x;
        }

        private unsafe void WriteColor(Entity target, int typeidx, int offset, Color x)
        {
            byte* ptr = EntityManager.GetComponentDataWithTypeRW(target, typeidx) + offset;
            *((Color*)ptr) = x;
        }

        private unsafe void WriteQuaternion(Entity target, int typeidx, int offset, quaternion x)
        {
            byte* ptr = EntityManager.GetComponentDataWithTypeRW(target, typeidx) + offset;
            *((quaternion*)ptr) = x;
        }

        private bool TryAddTween<T>(Entity tweenEntity, object startval, object endval, PrimitiveFieldTypes t)
        {
            if (typeof(T) == typeof(int))
            {
                if (t != PrimitiveFieldTypes.Int)
                    return false;
                EntityManager.AddComponentData(tweenEntity, new TweenFloat {start = (int)startval, end = (int)endval});
                return true;
            }

            if (typeof(T) == typeof(float))
            {
                if (t != PrimitiveFieldTypes.Float)
                    return false;
                EntityManager.AddComponentData(tweenEntity, new TweenFloat {start = (float)startval, end = (float)endval});
                return true;
            }

            if (typeof(T) == typeof(Unity.Mathematics.float2))
            {
                if (t != PrimitiveFieldTypes.Float2)
                    return false;
                EntityManager.AddComponentData(tweenEntity,
                    new TweenFloat2
                        {start = (Unity.Mathematics.float2) startval, end = (float2) endval});
                return true;
            }

            if (typeof(T) == typeof(Unity.Mathematics.float3))
            {
                if (t != PrimitiveFieldTypes.Float3)
                    return false;
                EntityManager.AddComponentData(tweenEntity, new TweenFloat3 {start = (float3)startval, end = (float3)endval});
                return true;
            }

            if (typeof(T) == typeof(Unity.Mathematics.float4))
            {
                if (t != PrimitiveFieldTypes.Float4)
                    return false;
                EntityManager.AddComponentData(tweenEntity, new TweenFloat4 {start = (float4)startval, end = (float4)endval});
                return true;
            }

            if (typeof(T) == typeof(Unity.Mathematics.quaternion))
            {
                if (t != PrimitiveFieldTypes.Quaternion)
                    return false;
                EntityManager.AddComponentData(tweenEntity, new TweenQuaternion {start = (quaternion)startval, end = (quaternion)endval});
                return true;
            }

            if (typeof(T) == typeof(Unity.Tiny.Core2D.Color))
            {
                if (t != PrimitiveFieldTypes.Color)
                    return false;
                EntityManager.AddComponentData(tweenEntity, new TweenColor {start = (Color)startval, end = (Color)endval});
                return true;
            }
            return false;
        }

        private Entity AddTweenInternal<T>(Entity target, T startval, T endval, in TweenDesc desc, PrimitiveFieldTypes ft)
        {
            Entity tweenEntity = EntityManager.CreateEntity();
            if (!TryAddTween<T>(tweenEntity, startval, endval, ft))
            {
                throw new ArgumentException("AddTweenInternal / AddTween did not receive valid arguments.");
            }
            TweenComponent ts = default;
            ts.desc = desc;
            ts.target = target;
            EntityManager.AddComponentData(tweenEntity, ts);
            return tweenEntity;
        }

        /// <summary>
        /// Creates an Entity to tween a value over time.
        /// </summary>
        /// <param name="target">Entity where the component to be tweened is attached.</param>
        /// <param name="fieldName">A string literal (and it must be a literal, not a variable) that specifies
        /// the path to the field. For example: "NonUniformScale.scale.y" or "NonUniformScale.scale"</param>
        /// <param name="startValue">Starting point for the tween.</param>
        /// <param name="endValue">Ending point for the tween.</param>
        /// <param name="duration">Time in seconds.</param>
        /// <param name="func">Tween function to use.</param>
        /// <param name="loop">True to repeat the tween.</param>
        /// <param name="destroyWhenDone">True to cleanup after the tween.</param>
        /// <param name="timeOffset">Set to start from a non-zero time.</param>
        /// <typeparam name="T">Type of the variable to tween.</typeparam>
        /// <returns></returns>
        public Entity AddTween<T>(Entity target,
            TypeManager.FieldInfo info,
            T startValue,
            T endValue,
            float duration,
            TweenFunc func = TweenFunc.Smoothstep,
            LoopMode loop = LoopMode.Once,
            bool destroyWhenDone = true,
            float timeOffset = 0.0f)
        {
            TweenDesc desc = default;
            desc.time = timeOffset;
            desc.duration = duration;
            desc.typeIndex = info.componentTypeIndex;
            desc.byteOffset = info.byteOffsetInComponent;
            desc.func = func;
            desc.destroyWhenDone = destroyWhenDone;
            desc.loop = loop;
            return AddTweenInternal(target, startValue, endValue, desc, info.primitiveType);
        }


        protected override void OnUpdate()
        {
            var env = World.GetExistingSystem<TinyEnvironment>();
            float dt = (float) env.frameDeltaTime;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            EntityCommandBuffer ecbNow = new EntityCommandBuffer(Allocator.Temp);
            // update all tweens
            Entities.ForEach((Entity e, ref TweenComponent tc) =>
            {
                tc.desc.time += dt;
                if (tc.desc.time < 0.0f)
                {
                    // not yet started, waiting
                    tc.started = false;
                    tc.ended = false;
                    tc.loopCount = 0;
                    return;
                }
                tc.started = true;
                float tl = LoopTime(tc.desc.time, tc.desc.duration, tc.desc.loop, ref tc.ended, ref tc.loopCount) /
                           tc.desc.duration;
                Assert.IsTrue(tl >= 0.0f && tl <= 1.0f);
                tc.normalizedTweenTime = EaseFunc(tl, tc.desc.func);
                if (tc.ended && tc.desc.destroyWhenDone)
                    ecb.DestroyEntity(e);
                else
                {
                    if (!EntityManager.Exists(tc.target)) // be helpful: if the target is gone, destroy the tween
                        ecbNow.DestroyEntity(e);
                    else // be helpful: if the target does not have the tween target component warn here and remove tween
                    {
                        if (!EntityManager.HasComponentRaw(tc.target, tc.desc.typeIndex))
                        {
                            Debug.LogFormat(
                                "A tween targeting entity {0} was removed because the target does not have a component with type {1}.",
                                tc.target, TypeManager.GetTypeInfo(tc.desc.typeIndex));
                            ecbNow.DestroyEntity(e);
                        }
                    }
                }
            });
            ecbNow.Playback(EntityManager);
            ecbNow.Dispose();

            // eval & writeback by type (could also do a switch in the earlier loop..)
            Entities.ForEach((ref TweenComponent tc, ref TweenInt tf) =>
            {
                if (!tc.started)
                    return;
                float val = math.lerp((float)tf.start, (float)tf.end, tc.normalizedTweenTime);
                WriteInt(tc.target, tc.desc.typeIndex, tc.desc.byteOffset, (int)Math.Round(val));
            });

            Entities.ForEach((ref TweenComponent tc, ref TweenFloat tf) =>
            {
                if (!tc.started)
                    return;
                float val = math.lerp(tf.start, tf.end, tc.normalizedTweenTime);
                WriteFloat(tc.target, tc.desc.typeIndex, tc.desc.byteOffset, val);
            });

            Entities.ForEach((ref TweenComponent tc, ref TweenColor tf) =>
            {
                if (!tc.started)
                    return;
                Color val = Color.Lerp(tf.start, tf.end, tc.normalizedTweenTime);
                WriteColor(tc.target, tc.desc.typeIndex, tc.desc.byteOffset, val);
            });

            Entities.ForEach((ref TweenComponent tc, ref TweenQuaternion tf) =>
            {
                if (!tc.started)
                    return;
                quaternion val = math.normalize(math.slerp(tf.start, tf.end, tc.normalizedTweenTime));
                WriteQuaternion(tc.target, tc.desc.typeIndex, tc.desc.byteOffset, val);
            });

            Entities.ForEach((ref TweenComponent tc, ref TweenFloat2 tf) =>
            {
                if (!tc.started)
                    return;
                float2 val = math.lerp(tf.start, tf.end, tc.normalizedTweenTime);
                WriteFloat2(tc.target, tc.desc.typeIndex, tc.desc.byteOffset, val);
            });


            Entities.ForEach((ref TweenComponent tc, ref TweenFloat3 tf) =>
            {
                if (!tc.started)
                    return;
                float3 val = math.lerp(tf.start, tf.end, tc.normalizedTweenTime);
                WriteFloat3(tc.target, tc.desc.typeIndex, tc.desc.byteOffset, val);
            });

            Entities.ForEach((ref TweenComponent tc, ref TweenFloat4 tf) =>
            {
                if (!tc.started)
                    return;
                float4 val = math.lerp(tf.start, tf.end, tc.normalizedTweenTime);
                WriteFloat4(tc.target, tc.desc.typeIndex, tc.desc.byteOffset, val);
            });


            // add all other types we want to tween

            // now delete destroyed ones (wait until after write-back so all tweens get set to their ending value)
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
