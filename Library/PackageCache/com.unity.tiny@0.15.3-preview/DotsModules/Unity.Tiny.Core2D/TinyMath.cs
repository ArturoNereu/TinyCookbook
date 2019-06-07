using Unity.Mathematics;

#if UNITY_USE_TINYMATH

namespace Unity.Tiny.Core2D
{
    public static class tinymath
    {
        public static bool approxEqual(float a, float b, float eps = 0.001f)
        {
            float d = a - b;
            return d > -eps && d < eps;
        }

        public static bool approxEqual(in float4 a, in float4 b, float eps = 0.001f)
        {
            return approxEqual(a.x, b.x, eps) &&
                   approxEqual(a.y, b.y, eps) &&
                   approxEqual(a.z, b.z, eps) &&
                   approxEqual(a.w, b.w, eps);
        }

        public static bool approxEqual(in float4x4 a, in float4x4 b, float eps=0.001f)
        {
            return approxEqual(a.c0, b.c0, eps) &&
                   approxEqual(a.c1, b.c1, eps) &&
                   approxEqual(a.c2, b.c2, eps) &&
                   approxEqual(a.c3, b.c3, eps);
        }

        public static bool approxEqual(in quaternion a, in quaternion b, float eps=0.001f)
        {
            return approxEqual(a.value, b.value, eps);
        }

        public static quaternion mul(in quaternion a, in quaternion b)
        {
            quaternion r;
            r.value.x = a.value.w * b.value.x + (a.value.x * b.value.w + a.value.y * b.value.z) - a.value.z * b.value.y;
            r.value.y = a.value.w * b.value.y + (a.value.y * b.value.w + a.value.z * b.value.x) - a.value.x * b.value.z;
            r.value.z = a.value.w * b.value.z + (a.value.z * b.value.w + a.value.x * b.value.y) - a.value.y * b.value.x;
            r.value.w = a.value.w * b.value.w - (a.value.x * b.value.x + a.value.y * b.value.y) - a.value.z * b.value.z;
            return r;
        }

        public static float4 mulCol(in float4x4 a, float4 b)
        {
            float4 r;
            r.x = a.c0.x * b.x + a.c1.x * b.y + a.c2.x * b.z + a.c3.x * b.w;
            r.y = a.c0.y * b.x + a.c1.y * b.y + a.c2.y * b.z + a.c3.y * b.w;
            r.z = a.c0.z * b.x + a.c1.z * b.y + a.c2.z * b.z + a.c3.z * b.w;
            r.w = a.c0.w * b.x + a.c1.w * b.y + a.c2.w * b.z + a.c3.w * b.w;
            return r;
        }

        public static float2 transform(in float4x4 a, in float2 b)
        {
            float2 r;
            r.x = a.c0.x * b.x + a.c1.x * b.y + a.c3.x;
            r.y = a.c0.y * b.x + a.c1.y * b.y + a.c3.y;
            return r;
        }


        public static float4x4 mul(in float4x4 a, in float4x4 b)
        {
            float4x4 r;
            r.c0 = mulCol(a, b.c0);
            r.c1 = mulCol(a, b.c1);
            r.c2 = mulCol(a, b.c2);
            r.c3 = mulCol(a, b.c3);
            return r;
        }

        public static float4x4 Translation(in float3 t)
        {
            float4x4 r = default;

            r.c0.x = 1.0f;
            r.c1.y = 1.0f;
            r.c2.z = 1.0f;
            r.c3.x = t.x;
            r.c3.y = t.y;
            r.c3.z = t.z;
            r.c3.w = 1.0f;

            return r;
        }

        public static float4x4 Rotation(in quaternion q)
        {
            /*
            c0 = v2.y * asfloat(asuint(v.yxw) ^ npn) - v2.z * asfloat(asuint(v.zwx) ^ pnn) + float3(1, 0, 0);
            c1 = v2.z * asfloat(asuint(v.wzy) ^ nnp) - v2.x * asfloat(asuint(v.yxw) ^ npn) + float3(0, 1, 0);
            c2 = v2.x * asfloat(asuint(v.zwx) ^ pnn) - v2.y * asfloat(asuint(v.wzy) ^ nnp) + float3(0, 0, 1);
            */
            float4x4 r = default;
            float4 v = q.value;
            float3 v2;
            v2.x = v.x + v.x;
            v2.y = v.y + v.y;
            v2.z = v.z + v.z;

            r.c0.x = v2.y * -v.y - v2.z * v.z + 1.0f;
            r.c0.y = v2.y * v.x - v2.z * -v.w;
            r.c0.z = v2.y * -v.w - v2.z * -v.x;
            //r.c0.w = 0;

            r.c1.x = v2.z * -v.w - v2.x * -v.y;
            r.c1.y = v2.z * -v.z - v2.x * v.x + 1.0f;
            r.c1.z = v2.z * v.y - v2.x * -v.w;
            //r.c1.w = 0;

            r.c2.x = v2.x * v.z - v2.y * -v.w;
            r.c2.y = v2.x * -v.w - v2.y * -v.z;
            r.c2.z = v2.x * -v.x - v2.y * v.y + 1.0f;
            //r.c2.w = 0;

            //r.c3.x = 0;
            //r.c3.y = 0;
            //r.c3.z = 0;
            r.c3.w = 1.0f;

            return r;
        }

        public static float4x4 Scale(in float3 s)
        {
            float4x4 r = default;
            r.c0.x = s.x;
            r.c1.y = s.y;
            r.c2.z = s.z;
            r.c3.w = 1.0f;
            return r;
        }

        public static float4x4 TranslationRotation(in float3 t, in quaternion q)
        {
            float4x4 r = default;

            float4 v = q.value;
            float3 v2;
            v2.x = v.x + v.x;
            v2.y = v.y + v.y;
            v2.z = v.z + v.z;

            r.c0.x = v2.y * -v.y - v2.z * v.z + 1.0f;
            r.c0.y = v2.y * v.x - v2.z * -v.w;
            r.c0.z = v2.y * -v.w - v2.z * -v.x;
            //r.c0.w = 0;

            r.c1.x = v2.z * -v.w - v2.x * -v.y;
            r.c1.y = v2.z * -v.z - v2.x * v.x + 1.0f;
            r.c1.z = v2.z * v.y - v2.x * -v.w;
            //r.c1.w = 0;

            r.c2.x = v2.x * v.z - v2.y * -v.w;
            r.c2.y = v2.x * -v.w - v2.y * -v.z;
            r.c2.z = v2.x * -v.x - v2.y * v.y + 1.0f;
            //r.c2.w = 0;

            r.c3.x = t.x;
            r.c3.y = t.y;
            r.c3.z = t.z;
            r.c3.w = 1.0f;

            return r;
        }

        public static float4x4 TranslationScale(in float3 t, in float3 s)
        {
            float4x4 r = default;
            r.c0.x = s.x;
            r.c1.y = s.y;
            r.c2.z = s.z;
            r.c3.x = t.x;
            r.c3.y = t.y;
            r.c3.z = t.z;
            r.c3.w = 1.0f;
            return r;
        }

        public static float4x4 RotationScale(in quaternion q, in float3 s)
        {
            float4x4 r = default;

            float4 v = q.value;
            float3 v2;
            v2.x = v.x + v.x;
            v2.y = v.y + v.y;
            v2.z = v.z + v.z;

            r.c0.x = (v2.y * -v.y - v2.z * v.z + 1.0f) * s.x;
            r.c0.y = (v2.y * v.x - v2.z * -v.w) * s.x;
            r.c0.z = (v2.y * -v.w - v2.z * -v.x) * s.x;
            //r.c0.w = 0;

            r.c1.x = (v2.z * -v.w - v2.x * -v.y) * s.y;
            r.c1.y = (v2.z * -v.z - v2.x * v.x + 1.0f) * s.y;
            r.c1.z = (v2.z * v.y - v2.x * -v.w) * s.y;
            //r.c1.w = 0;

            r.c2.x = (v2.x * v.z - v2.y * -v.w) * s.z;
            r.c2.y = (v2.x * -v.w - v2.y * -v.z) * s.z;
            r.c2.z = (v2.x * -v.x - v2.y * v.y + 1.0f) * s.z;
            //r.c2.w = 0;

            //r.c3.x = 0;
            //r.c3.y = 0;
            //r.c3.z = 0;
            r.c3.w = 1.0f;

            return r;
        }

        public static float4x4 TranslationRotationScale(in float3 t, in quaternion q, in float3 s)
        {
            float4x4 r = default;
            float4 v = q.value;
            float3 v2;
            v2.x = v.x + v.x;
            v2.y = v.y + v.y;
            v2.z = v.z + v.z;

            r.c0.x = (v2.y * -v.y - v2.z * v.z + 1.0f) * s.x;
            r.c0.y = (v2.y * v.x - v2.z * -v.w) * s.x;
            r.c0.z = (v2.y * -v.w - v2.z * -v.x) * s.x;
            //r.c0.w = 0;

            r.c1.x = (v2.z * -v.w - v2.x * -v.y) * s.y;
            r.c1.y = (v2.z * -v.z - v2.x * v.x + 1.0f) * s.y;
            r.c1.z = (v2.z * v.y - v2.x * -v.w) * s.y;
            //r.c1.w = 0;

            r.c2.x = (v2.x * v.z - v2.y * -v.w) * s.z;
            r.c2.y = (v2.x * -v.w - v2.y * -v.z) * s.z;
            r.c2.z = (v2.x * -v.x - v2.y * v.y + 1.0f) * s.z;
            //r.c2.w = 0;

            r.c3.x = t.x;
            r.c3.y = t.y;
            r.c3.z = t.z;
            r.c3.w = 1.0f;

            return r;
        }
    }
}
#endif
