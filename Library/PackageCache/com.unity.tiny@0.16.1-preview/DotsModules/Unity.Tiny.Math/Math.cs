using Unity.Entities;
using Unity.Tiny;
using Unity.Mathematics;

namespace Unity.Tiny.Math {

    // dummy c# file, used for including c++ math lib

    public struct Range
    {
        public float start;
        public float end;
    }

    public struct RangeInt
    {
        public int start;
        public int end;
    }

    public struct Vector2
    {
        public float3 value;
    }

    public struct Vector3
    {
        public float3 value;
    }

    public struct Vector4
    {
        public float4 value;
    }

    public struct Quaternion
    {
        public quaternion value;
    }

    public struct Matrix4x4
    {
        public float4x4 value;
    }

    public struct Matrix3x3
    {
        public float3x3 value;
    }
}
