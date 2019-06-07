using Unity.Mathematics;
using Unity.Entities;
using Unity.Tiny;
using Unity.Tiny.Core2D;

/// <summary>
///  Interpolation module for defining and evaluating different types of interpolation curves.
///
///  @module
///  @name Unity.Tiny
/// </summary>
[assembly: ModuleDescription("Unity.Tiny.Interpolation", "Interpolation curve primitives")]
namespace Unity.Tiny.Interpolation
{
    #region Key components
    public interface IKey
    {
        float GetTime();
    }

    public struct KeyFloat : IBufferElementData, IKey
    {
        public float time;
        public float value;

        public KeyFloat(float time, float value)
        {
            this.time = time;
            this.value = value;
        }

        public float GetTime() => time;
    }

    public struct KeyFloat2 : IBufferElementData, IKey
    {
        public float time;
        public float2 value;

        public KeyFloat2(float time, float2 value)
        {
            this.time = time;
            this.value = value;
        }

        public float GetTime() => time;
    }

    public struct KeyFloat3 : IBufferElementData, IKey
    {
        public float time;
        public float3 value;

        public KeyFloat3(float time, float3 value)
        {
            this.time = time;
            this.value = value;
        }

        public float GetTime() => time;
    }

    public struct KeyQuaternion : IBufferElementData, IKey
    {
        public float time;
        public quaternion value;

        public KeyQuaternion(float time, quaternion value)
        {
            this.time = time;
            this.value = value;
        }

        public float GetTime() => time;
    }

    public struct KeyColor : IBufferElementData, IKey
    {
        public float time;
        public Color value;

        public KeyColor(float time, Color value)
        {
            this.time = time;
            this.value = value;
        }

        public float GetTime() => time;
    }

    public struct BezierKeyFloat : IBufferElementData, IKey
    {
        public float time;
        public float value;
        public float inValue;
        public float outValue;

        public BezierKeyFloat(float time, float value, float inValue, float outValue)
        {
            this.time = time;
            this.value = value;
            this.inValue = inValue;
            this.outValue = outValue;
        }

        public float GetTime() => time;
    }

    public struct BezierKeyFloat2 : IBufferElementData, IKey
    {
        public float time;
        public float2 value;
        public float2 inValue;
        public float2 outValue;

        public BezierKeyFloat2(float time, float2 value, float2 inValue, float2 outValue)
        {
            this.time = time;
            this.value = value;
            this.inValue = inValue;
            this.outValue = outValue;
        }

        public float GetTime() => time;
    }

    public struct BezierKeyFloat3 : IBufferElementData, IKey
    {
        public float time;
        public float3 value;
        public float3 inValue;
        public float3 outValue;

        public BezierKeyFloat3(float time, float3 value, float3 inValue, float3 outValue)
        {
            this.time = time;
            this.value = value;
            this.inValue = inValue;
            this.outValue = outValue;
        }

        public float GetTime() => time;
    }

    public struct BezierKeyQuaternion : IBufferElementData, IKey
    {
        public float time;
        public quaternion value;
        public quaternion inValue;
        public quaternion outValue;

        public BezierKeyQuaternion(float time, quaternion value, quaternion inValue, quaternion outValue)
        {
            this.time = time;
            this.value = value;
            this.inValue = inValue;
            this.outValue = outValue;
        }

        public float GetTime() => time;
    }
    #endregion

    #region Curve components
    public interface ICurveComponent
    {
        Entity GetKeysEntity();
    }

    public struct LinearCurveFloat : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }

    public struct LinearCurveFloat2 : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }

    public struct LinearCurveFloat3 : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }

    public struct LinearCurveQuaternion : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }

    public struct LinearCurveColor : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }

    public struct BezierCurveFloat : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }

    public struct BezierCurveFloat2 : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }

    public struct BezierCurveFloat3 : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }

    public struct BezierCurveQuaternion : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }

    public struct StepCurveFloat : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }

    public struct StepCurveFloat2 : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }

    public struct StepCurveFloat3 : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }

    public struct StepCurveQuaternion : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }

    public struct StepCurveColor : IComponentData, ICurveComponent
    {
        public Entity keys;
        public Entity GetKeysEntity() => keys;
    }
    #endregion

    /// <summary>
    ///  Specifies how time wraps when evaluating the curve value. This component
    ///  works with entities that have curve components such as BezierCurveFloat,
    ///  LinearCurveVector3, and so on.
    /// </summary>
    public struct CurveTimeLoopMode : IComponentData
    {
        /// <summary> The type of the time wrapping. </summary>
        public LoopMode loopMode;
    }

    /// <summary>
    ///  Service for evaluating interpolation curves. It works with entities that
    ///  have curve components such as BezierCurveFloat, LinearCurveVector3, and so on.
    /// </summary>
    public static class InterpolationService
    {
        /// <summary>
        ///  Evaluates the Float value of the curve component at the given time.
        ///  curveEntity must contain a float curve component (BezierCurveFloat,
        ///  LinearCurveFloat, StepCurveFloat and so on).
        ///  To specify the type of time wrapping, add the CurveTimeWrapping
        ///  component to curveEntity.
        /// </summary>
        public static float EvaluateCurveFloat(EntityManager mgr, float time, Entity curveEntity)
        {
            if (mgr.HasComponent<BezierCurveFloat>(curveEntity))
                return EvaluateBezierCurveFloat(mgr, time, curveEntity);

            if (mgr.HasComponent<LinearCurveFloat>(curveEntity))
                return EvaluateLinearCurveFloat(mgr, time, curveEntity);

            if (mgr.HasComponent<StepCurveFloat>(curveEntity))
                return EvaluateStepCurveFloat(mgr, time, curveEntity);

            return default;
        }

        public static float2 EvaluateCurveFloat2(EntityManager mgr, float time, Entity curveEntity)
        {
            if (mgr.HasComponent<BezierCurveFloat2>(curveEntity))
                return EvaluateBezierCurveFloat2(mgr, time, curveEntity);

            if (mgr.HasComponent<LinearCurveFloat2>(curveEntity))
                return EvaluateLinearCurveFloat2(mgr, time, curveEntity);

            if (mgr.HasComponent<StepCurveFloat2>(curveEntity))
                return EvaluateStepCurveFloat2(mgr, time, curveEntity);

            return default;
        }

        public static float3 EvaluateCurveFloat3(EntityManager mgr, float time, Entity curveEntity)
        {
            if (mgr.HasComponent<BezierCurveFloat3>(curveEntity))
                return EvaluateBezierCurveFloat3(mgr, time, curveEntity);

            if (mgr.HasComponent<LinearCurveFloat3>(curveEntity))
                return EvaluateLinearCurveFloat3(mgr, time, curveEntity);

            if (mgr.HasComponent<StepCurveFloat3>(curveEntity))
                return EvaluateStepCurveFloat3(mgr, time, curveEntity);

            return default;
        }

        public static quaternion EvaluateCurveQuaternion(EntityManager mgr, float time, Entity curveEntity)
        {
            if (mgr.HasComponent<BezierCurveQuaternion>(curveEntity))
                return EvaluateBezierCurveQuaternion(mgr, time, curveEntity);

            if (mgr.HasComponent<LinearCurveQuaternion>(curveEntity))
                return EvaluateLinearCurveQuaternion(mgr, time, curveEntity);

            if (mgr.HasComponent<StepCurveQuaternion>(curveEntity))
                return EvaluateStepCurveQuaternion(mgr, time, curveEntity);

            return default;
        }

        public static Color EvaluateCurveColor(EntityManager mgr, float time, Entity curveEntity)
        {
            if (mgr.HasComponent<LinearCurveColor>(curveEntity))
                return EvaluateLinearCurveColor(mgr, time, curveEntity);

            if (mgr.HasComponent<StepCurveColor>(curveEntity))
                return EvaluateStepCurveColor(mgr, time, curveEntity);

            return default;
        }

        #region Linear Curves
        public static float EvaluateLinearCurveFloat(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<KeyFloat>();
            if (!ValidateCurveEntity<KeyFloat, LinearCurveFloat>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateLinearCurve(time, keys);
        }

        public static float2 EvaluateLinearCurveFloat2(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<KeyFloat2>();
            if (!ValidateCurveEntity<KeyFloat2, LinearCurveFloat2>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateLinearCurve(time, keys);
        }

        public static float3 EvaluateLinearCurveFloat3(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<KeyFloat3>();
            if (!ValidateCurveEntity<KeyFloat3, LinearCurveFloat3>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateLinearCurve(time, keys);
        }

        public static quaternion EvaluateLinearCurveQuaternion(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<KeyQuaternion>();
            if (!ValidateCurveEntity<KeyQuaternion, LinearCurveQuaternion>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateLinearCurve(time, keys);
        }

        public static Color EvaluateLinearCurveColor(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<KeyColor>();
            if (!ValidateCurveEntity<KeyColor, LinearCurveColor>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateLinearCurve(time, keys);
        }
        #endregion

        #region Bezier curves
        public static float EvaluateBezierCurveFloat(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<BezierKeyFloat>();
            if (!ValidateCurveEntity<BezierKeyFloat, BezierCurveFloat>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateBezierCurve(time, keys);
        }

        public static float2 EvaluateBezierCurveFloat2(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<BezierKeyFloat2>();
            if (!ValidateCurveEntity<BezierKeyFloat2, BezierCurveFloat2>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateBezierCurve(time, keys);
        }

        public static float3 EvaluateBezierCurveFloat3(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<BezierKeyFloat3>();
            if (!ValidateCurveEntity<BezierKeyFloat3, BezierCurveFloat3>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateBezierCurve(time, keys);
        }

        public static quaternion EvaluateBezierCurveQuaternion(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<BezierKeyQuaternion>();
            if (!ValidateCurveEntity<BezierKeyQuaternion, BezierCurveQuaternion>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateBezierCurve(time, keys);
        }
        #endregion

        #region Step curves
        public static float EvaluateStepCurveFloat(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<KeyFloat>();
            if (!ValidateCurveEntity<KeyFloat, StepCurveFloat>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateStepCurve(time, keys);
        }

        public static float2 EvaluateStepCurveFloat2(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<KeyFloat2>();
            if (!ValidateCurveEntity<KeyFloat2, StepCurveFloat2>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateStepCurve(time, keys);
        }

        public static float3 EvaluateStepCurveFloat3(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<KeyFloat3>();
            if (!ValidateCurveEntity<KeyFloat3, StepCurveFloat3>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateStepCurve(time, keys);
        }

        public static quaternion EvaluateStepCurveQuaternion(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<KeyQuaternion>();
            if (!ValidateCurveEntity<KeyQuaternion, StepCurveQuaternion>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateStepCurve(time, keys);
        }

        public static Color EvaluateStepCurveColor(EntityManager mgr, float time, Entity entity)
        {
            var keys = new DynamicBuffer<KeyColor>();
            if (!ValidateCurveEntity<KeyColor, StepCurveColor>(mgr, entity, ref keys))
                return default;

            time = WrapTime(mgr, time, keys[0].time, keys[keys.Length - 1].time, entity);

            return CurveEvaluator.EvaluateStepCurve(time, keys);
        }
        #endregion

        private static bool ValidateCurveEntity<TKeyComponent, TCurveComponent>(EntityManager mgr, Entity entity, ref DynamicBuffer<TKeyComponent> keys)
            where TKeyComponent : struct, IBufferElementData
            where TCurveComponent : struct, IComponentData, ICurveComponent
        {
            if (!mgr.Exists(entity) || !mgr.HasComponent<TCurveComponent>(entity))
                return false;

            var curve = mgr.GetComponentData<TCurveComponent>(entity);
            var keysEntity = curve.GetKeysEntity();
            if (!mgr.Exists(keysEntity) || !mgr.HasComponent<TKeyComponent>(keysEntity))
                return false;

            keys = mgr.GetBuffer<TKeyComponent>(keysEntity);
            if (keys.Length == 0)
                return false;

            return true;
        }

        private static float WrapTime(EntityManager mgr, float time, float startTime, float endTime, Entity entity)
        {
            if (!mgr.HasComponent<CurveTimeLoopMode>(entity))
                return time;

            var curveTimeWrapping = mgr.GetComponentData<CurveTimeLoopMode>(entity);

            return TimeLooper.LoopTime(time, startTime, endTime, curveTimeWrapping.loopMode, out bool dummy);
        }
    }
}
