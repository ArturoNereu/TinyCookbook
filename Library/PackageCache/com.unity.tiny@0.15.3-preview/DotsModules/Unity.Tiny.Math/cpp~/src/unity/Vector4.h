#pragma once

#include "Vector3.h"

class Vector4f
{
public:

    Vector4f() {} // Default ctor is intentionally empty for performance reasons
    Vector4f(const Vector4f& v) : x(v.x), y(v.y), z(v.z), w(v.w) {}   // Necessary for correct optimized GCC codegen
    Vector4f(float inX, float inY, float inZ, float inW) : x(inX), y(inY), z(inZ), w(inW) {}
    explicit Vector4f(const Vector3f& v, float inW) : x(v.x), y(v.y), z(v.z), w(inW) {}
    explicit Vector4f(const float* v) : x(v[0]), y(v[1]), z(v[2]), w(v[3]) {}

    void Set(float inX, float inY, float inZ, float inW) { x = inX; y = inY; z = inZ; w = inW; }
    void Set(const float* array) { x = array[0]; y = array[1]; z = array[2]; w = array[3]; }
    void SetZero() { x = 0.0f; y = 0.0f; z = 0.0f; w = 0.0f; }

    float* GetPtr()             { return &x; }
    const float* GetPtr() const { return &x; }

    float& operator[](int i)                       { DebugAssert(i >= 0 && i <= 3); return (&x)[i]; }
    const float& operator[](int i) const            { DebugAssert(i >= 0 && i <= 3); return (&x)[i]; }

    bool operator==(const Vector4f& v) const      { return x == v.x && y == v.y && z == v.z && w == v.w; }
    bool operator!=(const Vector4f& v) const      { return x != v.x || y != v.y || z != v.z || w != v.w; }
    bool operator==(const float v[4]) const       { return x == v[0] && y == v[1] && z == v[2] && w == v[3]; }
    bool operator!=(const float v[4]) const       { return x != v[0] || y != v[1] || z != v[2] || w != v[3]; }

    Vector4f operator-() const                    { return Vector4f(-x, -y, -z, -w); }

    float x;
    float y;
    float z;
    float w;

    EXPORT_COREMODULE static const float    infinity;
    EXPORT_COREMODULE static const Vector4f infinityVec;
    EXPORT_COREMODULE static const Vector4f zero;
    EXPORT_COREMODULE static const Vector4f one;
};


inline Vector4f operator*(const Vector4f& lhs, const Vector4f& rhs)   { return Vector4f(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z, lhs.w * rhs.w); }
inline Vector4f operator*(const Vector4f& inV, const float s)         { return Vector4f(inV.x * s, inV.y * s, inV.z * s, inV.w * s); }
inline Vector4f operator+(const Vector4f& lhs, const Vector4f& rhs)   { return Vector4f(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z, lhs.w + rhs.w); }
inline Vector4f operator-(const Vector4f& lhs, const Vector4f& rhs)   { return Vector4f(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z, lhs.w - rhs.w); }
inline float Dot(const Vector4f& lhs, const Vector4f& rhs)             { return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z + lhs.w * rhs.w; }

inline float SqrMagnitude(const Vector4f& inV)                         { return Dot(inV, inV); }
inline float Magnitude(const Vector4f& inV)                            { return SqrtImpl(Dot(inV, inV)); }

inline bool IsFinite(const Vector4f& f)
{
    return IsFinite(f.x) & IsFinite(f.y) & IsFinite(f.z) && IsFinite(f.w);
}

inline bool CompareApproximately(const Vector4f& inV0, const Vector4f& inV1, const float inMaxDist = Vector3f::epsilon)
{
    return SqrMagnitude(inV1 - inV0) <= inMaxDist * inMaxDist;
}

inline Vector4f Lerp(const Vector4f& from, const Vector4f& to, float t) { return to * t + from * (1.0F - t); }

