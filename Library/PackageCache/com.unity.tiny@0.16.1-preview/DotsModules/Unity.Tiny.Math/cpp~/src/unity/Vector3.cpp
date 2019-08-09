#include "UnityPrefix.h"
#include "Vector3.h"
#include "Matrix3x3.h"

#define FPFIXES 1


const float     Vector3f::epsilon = 0.00001F;
const float     Vector3f::infinity = std::numeric_limits<float>::infinity();
const Vector3f  Vector3f::infinityVec = Vector3f(std::numeric_limits<float>::infinity(), std::numeric_limits<float>::infinity(), std::numeric_limits<float>::infinity());

const Vector3f  Vector3f::zero  = Vector3f(0, 0, 0);
const Vector3f  Vector3f::one  = Vector3f(1.0F, 1.0F, 1.0F);
const Vector3f  Vector3f::xAxis = Vector3f(1, 0, 0);
const Vector3f  Vector3f::yAxis = Vector3f(0, 1, 0);
const Vector3f  Vector3f::zAxis = Vector3f(0, 0, 1);

void OrthoNormalizeFast(Vector3f* inU, Vector3f* inV, Vector3f* inW)
{
    // compute u0
    *inU = Normalize(*inU);

    // compute u1
    float dot0 = Dot(*inU, *inV);
    *inV -= dot0 * *inU;
    *inV = Normalize(*inV);

    // compute u2
    float dot1 = Dot(*inV, *inW);
    dot0 = Dot(*inU, *inW);
    *inW -= dot0 * *inU + dot1 * *inV;
    *inW = Normalize(*inW);
}

void OrthoNormalize(Vector3f* inU, Vector3f* inV)
{
    // compute u0
    float mag = Magnitude(*inU);
    if (mag > Vector3f::epsilon)
        *inU /= mag;
    else
        *inU = Vector3f(1.0F, 0.0F, 0.0F);

    // compute u1
    float dot0 = Dot(*inU, *inV);
    *inV -= dot0 * *inU;
    mag = Magnitude(*inV);
    if (mag < Vector3f::epsilon)
        *inV = OrthoNormalVectorFast(*inU);
    else
        *inV /= mag;
}

void OrthoNormalize(Vector3f* inU, Vector3f* inV, Vector3f* inW)
{
    // compute u0
    float mag = Magnitude(*inU);
    if (mag > Vector3f::epsilon)
        *inU /= mag;
    else
        *inU = Vector3f(1.0F, 0.0F, 0.0F);

    // compute u1
    float dot0 = Dot(*inU, *inV);
    *inV -= dot0 * *inU;
    mag = Magnitude(*inV);
    if (mag > Vector3f::epsilon)
        *inV /= mag;
    else
        *inV = OrthoNormalVectorFast(*inU);

    // compute u2
    float dot1 = Dot(*inV, *inW);
    dot0 = Dot(*inU, *inW);
    *inW -= dot0 * *inU + dot1 * *inV;
    mag = Magnitude(*inW);
    if (mag > Vector3f::epsilon)
        *inW /= mag;
    else
        *inW = Cross(*inU, *inV);
}

#define k1OverSqrt2 float(0.7071067811865475244008443621048490)

Vector3f OrthoNormalVectorFast(const Vector3f& n)
{
    Vector3f res;
    if (Abs(n.z) > k1OverSqrt2)
    {
        // choose p in y-z plane
        float a = n.y * n.y + n.z * n.z;
        float k = 1.0F / Sqrt(a);
        res.x = 0;
        res.y = -n.z * k;
        res.z = n.y * k;
    }
    else
    {
        // choose p in x-y plane
        float a = n.x * n.x + n.y * n.y;
        float k = 1.0F / Sqrt(a);
        res.x = -n.y * k;
        res.y = n.x * k;
        res.z = 0;
    }
    return res;
}

/* from chris hecker (Generates Orthonormal basis)
void
DextralBases(real32 const *XAxis, real32 *YAxis, real32 *ZAxis)
{
    real32 CrossVector[3] = {1.0f, 1.0f, 1.0f};

    real32 MaximumElement = 0.0f;

    int MaximumElementIndex = 0;
    {for(int ElementIndex = 0;
         ElementIndex < 3;
         ++ElementIndex)
    {
        real32 ElementValue = AbsoluteValue(XAxis[ElementIndex]);
        if(ElementValue > MaximumElement)
        {
            MaximumElement = ElementValue;
            MaximumElementIndex = ElementIndex;
        }
    }}

    CrossVector[MaximumElementIndex] = 0.0f;

    VectorCrossProduct3(YAxis, CrossVector, XAxis);
    Normalize3(YAxis);

    VectorCrossProduct3(ZAxis, XAxis, YAxis);
    Normalize3(ZAxis);
}

*/

/// Returns a Vector3 that moves lhs towards rhs by a maximum of clampedDistance
Vector3f MoveTowards(const Vector3f& lhs, const Vector3f& rhs, float clampedDistance)
{
    Vector3f delta = rhs - lhs;
    float sqrDelta = SqrMagnitude(delta);
    float sqrClampedDistance = clampedDistance * clampedDistance;
    if (sqrDelta > sqrClampedDistance)
    {
        float deltaMag = Sqrt(sqrDelta);
        if (deltaMag > Vector3f::epsilon)
            return lhs + delta / deltaMag * clampedDistance;
        else
            return lhs;
    }
    else
        return rhs;
}

static inline float ClampedMove(float lhs, float rhs, float clampedDelta)
{
    float delta = rhs - lhs;
    if (delta > 0.0F)
        return lhs + std::min(delta, clampedDelta);
    else
        return lhs - std::min(-delta, clampedDelta);
}

Vector3f RotateTowards(const Vector3f& lhs, const Vector3f& rhs, float angleMove, float magnitudeMove)
{
    float lhsMag = Magnitude(lhs);
    float rhsMag = Magnitude(rhs);

    // both vectors are non-zero
    if (lhsMag > Vector3f::epsilon && rhsMag > Vector3f::epsilon)
    {
        Vector3f lhsNorm = lhs / lhsMag;
        Vector3f rhsNorm = rhs / rhsMag;

        float dot = Dot(lhsNorm, rhsNorm);
        // direction is almost the same
        if (dot > 1.0F - Vector3f::epsilon)
        {
            return MoveTowards(lhs, rhs, magnitudeMove);
        }
        // directions are almost opposite
        else if (dot < -1.0F + Vector3f::epsilon)
        {
            Vector3f axis = OrthoNormalVectorFast(lhsNorm);
            Matrix3x3f m;
            m.SetAxisAngle(axis, angleMove);
            Vector3f rotated = m.MultiplyPoint3(lhsNorm);
            rotated *= ClampedMove(lhsMag, rhsMag, magnitudeMove);
            return rotated;
        }
        // normal case
        else
        {
            float angle = std::acos(dot);
            Vector3f axis = Normalize(Cross(lhsNorm, rhsNorm));
            Matrix3x3f m;
            m.SetAxisAngle(axis, std::min(angleMove, angle));
            Vector3f rotated = m.MultiplyPoint3(lhsNorm);
            rotated *= ClampedMove(lhsMag, rhsMag, magnitudeMove);
            return rotated;
        }
    }
    // at least one of the vectors is almost zero
    else
    {
        return MoveTowards(lhs, rhs, magnitudeMove);
    }
}

Vector3f Slerp(const Vector3f& lhs, const Vector3f& rhs, float t)
{
    float lhsMag = Magnitude(lhs);
    float rhsMag = Magnitude(rhs);

    if (lhsMag < Vector3f::epsilon || rhsMag < Vector3f::epsilon)
        return Lerp(lhs, rhs, t);

    float lerpedMagnitude = Lerp(lhsMag, rhsMag, t);

    float dot = Dot(lhs, rhs) / (lhsMag * rhsMag);
    // direction is almost the same
    if (dot > 1.0F - Vector3f::epsilon)
    {
        return Lerp(lhs, rhs, t);
    }
    // directions are almost opposite
    else if (dot < -1.0F + Vector3f::epsilon)
    {
        Vector3f lhsNorm = lhs / lhsMag;
        Vector3f axis = OrthoNormalVectorFast(lhsNorm);
        Matrix3x3f m;
        m.SetAxisAngle(axis, kPI * t);
        Vector3f slerped = m.MultiplyPoint3(lhsNorm);
        slerped *= lerpedMagnitude;
        return slerped;
    }
    // normal case
    else
    {
        Vector3f axis = Cross(lhs, rhs);
        Vector3f lhsNorm = lhs / lhsMag;
        axis = Normalize(axis);
        float angle = std::acos(dot) * t;

        Matrix3x3f m;
        m.SetAxisAngle(axis, angle);
        Vector3f slerped = m.MultiplyPoint3(lhsNorm);
        slerped *= lerpedMagnitude;
        return slerped;
    }
}

inline static Vector3f NormalizeRobust(const Vector3f& a, float &l, float &div)
{
    float a0, a1, a2, aa0, aa1, aa2;
    a0 = a[0];
    a1 = a[1];
    a2 = a[2];

#if FPFIXES
    if (CompareApproximately(a0, 0.0F, 0.00001F))
        a0 = aa0 = 0;
    else
#endif
    {
        aa0 = Abs(a0);
    }

#if FPFIXES
    if (CompareApproximately(a1, 0.0F, 0.00001F))
        a1 = aa1 = 0;
    else
#endif
    {
        aa1 = Abs(a1);
    }

#if FPFIXES
    if (CompareApproximately(a2, 0.0F, 0.00001F))
        a2 = aa2 = 0;
    else
#endif
    {
        aa2 = Abs(a2);
    }

    if (aa1 > aa0)
    {
        if (aa2 > aa1)
        {
            a0 /= aa2;
            a1 /= aa2;
            l = InvSqrt(a0 * a0 + a1 * a1 + 1.0F);
            div = aa2;
            return Vector3f(a0 * l, a1 * l, CopySignf(l, a2));
        }
        else
        {
            // aa1 is largest
            a0 /= aa1;
            a2 /= aa1;
            l = InvSqrt(a0 * a0 + a2 * a2 + 1.0F);
            div = aa1;
            return Vector3f(a0 * l, CopySignf(l, a1), a2 * l);
        }
    }
    else
    {
        if (aa2 > aa0)
        {
            // aa2 is largest
            a0 /= aa2;
            a1 /= aa2;
            l = InvSqrt(a0 * a0 + a1 * a1 + 1.0F);
            div = aa2;
            return Vector3f(a0 * l, a1 * l, CopySignf(l, a2));
        }
        else
        {
            // aa0 is largest
            if (aa0 <= 0)
            {
                l = 0;
                div = 1;
                return Vector3f(0.0F, 1.0F, 0.0F);
            }

            a1 /= aa0;
            a2 /= aa0;
            l = InvSqrt(a1 * a1 + a2 * a2 + 1.0F);
            div = aa0;
            return Vector3f(CopySignf(l, a0), a1 * l, a2 * l);
        }
    }
}

Vector3f NormalizeRobust(const Vector3f& a)
{
    float l, div;
    return NormalizeRobust(a, l, div);
}

Vector3f NormalizeRobust(const Vector3f& a, float &invOriginalLength)
{
    float l, div;
    const Vector3f &n = NormalizeRobust(a, l, div);
    invOriginalLength = l / div;
    // guard for NaNs
    Assert(n == n);
    Assert(invOriginalLength == invOriginalLength);
    Assert(IsNormalized(n));
    return n;
}

#if ENABLE_UNIT_TESTS
#include <ostream>
std::ostream& operator<<(std::ostream& stream, const Vector3f& vec)
{
    stream << "{x: " << vec.x << ", y: " << vec.y << ", z: " << vec.z << "}";
    return stream;
}

#endif
