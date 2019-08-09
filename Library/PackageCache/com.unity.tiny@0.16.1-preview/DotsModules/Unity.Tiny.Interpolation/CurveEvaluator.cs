using Unity.Mathematics;
using Unity.Entities;
using Unity.Tiny.Core2D;

namespace Unity.Tiny.Interpolation
{
    public static class CurveEvaluator
    {
        private delegate T LerpFunc<T>(T from, T to, float time);
        private delegate T GetKeyValueDelegate<T>(IKey startKey);
        private delegate T EvaluateSegmentDelegate<T>(float normalizedTime, IKey startKey, IKey endKey);

        public static float EvaluateStepCurve(float time, DynamicBuffer<KeyFloat> keys)
        {
            return Evaluate(time, keys, GetKeyValueFloat, EvaluateSegmentStepFloat);
        }

        public static float2 EvaluateStepCurve(float time, DynamicBuffer<KeyFloat2> keys)
        {
            return Evaluate(time, keys, GetKeyValueFloat2, EvaluateSegmentStepFloat2);
        }

        public static float3 EvaluateStepCurve(float time, DynamicBuffer<KeyFloat3> keys)
        {
            return Evaluate(time, keys, GetKeyValueFloat3, EvaluateSegmentStepFloat3);
        }

        public static quaternion EvaluateStepCurve(float time, DynamicBuffer<KeyQuaternion> keys)
        {
            return Evaluate(time, keys, GetKeyValueQuat, EvaluateSegmentStepQuaternion);
        }

        public static Color EvaluateStepCurve(float time, DynamicBuffer<KeyColor> keys)
        {
            return Evaluate(time, keys, GetKeyValueColor, EvaluateSegmentStepColor);
        }

        public static float EvaluateLinearCurve(float time, DynamicBuffer<KeyFloat> keys)
        {
            return Evaluate(time, keys, GetKeyValueFloat, EvaluateSegmentLinearFloat);
        }

        public static float2 EvaluateLinearCurve(float time, DynamicBuffer<KeyFloat2> keys)
        {
            return Evaluate(time, keys, GetKeyValueFloat2, EvaluateSegmentLinearFloat2);
        }

        public static float3 EvaluateLinearCurve(float time, DynamicBuffer<KeyFloat3> keys)
        {
            return Evaluate(time, keys, GetKeyValueFloat3, EvaluateSegmentLinearFloat3);
        }

        public static quaternion EvaluateLinearCurve(float time, DynamicBuffer<KeyQuaternion> keys)
        {
            return Evaluate(time, keys, GetKeyValueQuat, EvaluateSegmentLinearQuat);
        }

        public static Color EvaluateLinearCurve(float time, DynamicBuffer<KeyColor> keys)
        {
            return Evaluate(time, keys, GetKeyValueColor, EvaluateSegmentLinearColor);
        }

        public static float EvaluateBezierCurve(float time, DynamicBuffer<BezierKeyFloat> keys)
        {
            return Evaluate(time, keys, GetBezierKeyValueFloat, EvaluateSegmentBezierFloat);
        }

        public static float2 EvaluateBezierCurve(float time, DynamicBuffer<BezierKeyFloat2> keys)
        {
            return Evaluate(time, keys, GetBezierKeyValueFloat2, EvaluateSegmentBezierFloat2);
        }

        public static float3 EvaluateBezierCurve(float time, DynamicBuffer<BezierKeyFloat3> keys)
        {
            return Evaluate(time, keys, GetBezierKeyValueFloat3, EvaluateSegmentBezierFloat3);
        }

        public static quaternion EvaluateBezierCurve(float time, DynamicBuffer<BezierKeyQuaternion> keys)
        {
            return Evaluate(time, keys, GetBezierKeyValueQuat, EvaluateSegmentBezierQuat);
        }

        private static T EvaluateLinear<T>(float normalizedTime, T valueStart, T valueEnd, LerpFunc<T> lerp)
        {
            return lerp(valueStart, valueEnd, normalizedTime);
        }

        private static T EvaluateBezier<T>(float normalizedTime, T valueStart, T outValue, T inValue, T valueEnd, LerpFunc<T> lerp)
        {
            T t10 = lerp(valueStart, outValue, normalizedTime);
            T t11 = lerp(outValue, inValue, normalizedTime);
            T t12 = lerp(inValue, valueEnd, normalizedTime);
            T t20 = lerp(t10, t11, normalizedTime);
            T t21 = lerp(t11, t12, normalizedTime);
            return lerp(t20, t21, normalizedTime);
        }

        private static T Evaluate<T, T2>(float time, DynamicBuffer<T2> keys, GetKeyValueDelegate<T> getKeyValue, EvaluateSegmentDelegate<T> evaluateSegment)
            where T2 : struct, IKey
        {
            var keysCount = keys.Length;

            if (keysCount == 0)
                return default;

            // Clamp from the left.
            if (time <= keys[0].GetTime())
                return getKeyValue(keys[0]);

            // Clamp from the right.
            if (time >= keys[keysCount - 1].GetTime())
                return getKeyValue(keys[keysCount - 1]);

            CurveShared.GetNormalizedTimeAndKeyIndex(time, keys, out float normalizedTime, out int keyIndex);

            return evaluateSegment(normalizedTime, keys[keyIndex], keys[keyIndex + 1]);
        }

        #region Lerp methods
        private static float LerpFloat(float from, float to, float time)
        {
            return math.lerp(from, to, time);
        }

        private static float2 LerpFloat2(float2 from, float2 to, float time)
        {
            return math.lerp(from, to, time);
        }

        private static float3 LerpFloat3(float3 from, float3 to, float time)
        {
            return math.lerp(from, to, time);
        }

        private static quaternion LerpQuat(quaternion from, quaternion to, float time)
        {
            return math.slerp(from, to, time);
        }

        private static Color LerpColor(Color from, Color to, float time)
        {
            return Color.Lerp(from, to, time);
        }
        #endregion

        #region GetKeyValue methods
        private static float GetKeyValueFloat(IKey key)
        {
            return ((KeyFloat)key).value;
        }

        private static float2 GetKeyValueFloat2(IKey key)
        {
            return ((KeyFloat2)key).value;
        }

        private static float3 GetKeyValueFloat3(IKey key)
        {
            return ((KeyFloat3)key).value;
        }

        private static quaternion GetKeyValueQuat(IKey key)
        {
            return ((KeyQuaternion)key).value;
        }

        private static Color GetKeyValueColor(IKey key)
        {
            return ((KeyColor)key).value;
        }

        private static float GetBezierKeyValueFloat(IKey key)
        {
            return ((BezierKeyFloat)key).value;
        }

        private static float2 GetBezierKeyValueFloat2(IKey key)
        {
            return ((BezierKeyFloat2)key).value;
        }

        private static float3 GetBezierKeyValueFloat3(IKey key)
        {
            return ((BezierKeyFloat3)key).value;
        }

        private static quaternion GetBezierKeyValueQuat(IKey key)
        {
            return ((BezierKeyQuaternion)key).value;
        }
        #endregion

        #region EvaluateSegment methods
        private static float EvaluateSegmentStepFloat(float normalizedTime, IKey startKey, IKey endKey)
        {
            return ((KeyFloat)startKey).value;
        }

        private static float2 EvaluateSegmentStepFloat2(float normalizedTime, IKey startKey, IKey endKey)
        {
            return ((KeyFloat2)startKey).value;
        }

        private static float3 EvaluateSegmentStepFloat3(float normalizedTime, IKey startKey, IKey endKey)
        {
            return ((KeyFloat3)startKey).value;
        }

        private static quaternion EvaluateSegmentStepQuaternion(float normalizedTime, IKey startKey, IKey endKey)
        {
            return ((KeyQuaternion)startKey).value;
        }

        private static Color EvaluateSegmentStepColor(float normalizedTime, IKey startKey, IKey endKey)
        {
            return ((KeyColor)startKey).value;
        }

        private static float EvaluateSegmentLinearFloat(float normalizedTime, IKey startKey, IKey endKey)
        {
            return EvaluateLinear(normalizedTime, ((KeyFloat)startKey).value, ((KeyFloat)endKey).value, LerpFloat);
        }

        private static float2 EvaluateSegmentLinearFloat2(float normalizedTime, IKey startKey, IKey endKey)
        {
            return EvaluateLinear(normalizedTime, ((KeyFloat2)startKey).value, ((KeyFloat2)endKey).value, LerpFloat2);
        }

        private static float3 EvaluateSegmentLinearFloat3(float normalizedTime, IKey startKey, IKey endKey)
        {
            return EvaluateLinear(normalizedTime, ((KeyFloat3)startKey).value, ((KeyFloat3)endKey).value, LerpFloat3);
        }

        private static quaternion EvaluateSegmentLinearQuat(float normalizedTime, IKey startKey, IKey endKey)
        {
            return EvaluateLinear(normalizedTime, ((KeyQuaternion)startKey).value, ((KeyQuaternion)endKey).value, LerpQuat);
        }

        private static Color EvaluateSegmentLinearColor(float normalizedTime, IKey startKey, IKey endKey)
        {
            return EvaluateLinear(normalizedTime, ((KeyColor)startKey).value, ((KeyColor)endKey).value, LerpColor);
        }

        private static float EvaluateSegmentBezierFloat(float normalizedTime, IKey startKey, IKey endKey)
        {
            return EvaluateBezier(normalizedTime, ((BezierKeyFloat)startKey).value, ((BezierKeyFloat)startKey).outValue,
                ((BezierKeyFloat)endKey).inValue, ((BezierKeyFloat)endKey).value, LerpFloat);
        }

        private static float2 EvaluateSegmentBezierFloat2(float normalizedTime, IKey startKey, IKey endKey)
        {
            return EvaluateBezier(normalizedTime, ((BezierKeyFloat2)startKey).value, ((BezierKeyFloat2)startKey).outValue,
                ((BezierKeyFloat2)endKey).inValue, ((BezierKeyFloat2)endKey).value, LerpFloat2);
        }

        private static float3 EvaluateSegmentBezierFloat3(float normalizedTime, IKey startKey, IKey endKey)
        {
            return EvaluateBezier(normalizedTime, ((BezierKeyFloat3)startKey).value, ((BezierKeyFloat3)startKey).outValue,
                ((BezierKeyFloat3)endKey).inValue, ((BezierKeyFloat3)endKey).value, LerpFloat3);
        }

        private static quaternion EvaluateSegmentBezierQuat(float normalizedTime, IKey startKey, IKey endKey)
        {
            return EvaluateBezier(normalizedTime, ((BezierKeyQuaternion)startKey).value, ((BezierKeyQuaternion)startKey).outValue,
                ((BezierKeyQuaternion)endKey).inValue, ((BezierKeyQuaternion)endKey).value, LerpQuat);
        }
        #endregion
    }
}
