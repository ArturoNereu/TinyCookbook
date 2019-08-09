#pragma once

#include "UnityMathTypes.h"

namespace ut {
namespace Math {

struct TRS {
    Vector3f t;
    Quaternionf q;
    Vector3f s;

    TRS() {}
    TRS(int) : t(Vector3f::zero), q(Quaternionf::identity()), s(Vector3f::one) {}
    TRS(const Vector3f& tv, const Quaternionf& tq, const Vector3f& ts) : t(tv), q(tq), s(ts) {}

    TRS(const TRS& other) = default;
    ~TRS() = default;

    void ToMatrix(Matrix4x4f& m) const {
        m.SetTRS(t, q, s);
    }

    Vector3f TransformPoint(const Vector3f& v) const UT_WARN_UNUSED {
        return t + RotateVectorByQuat(q, v * s);
    }

    Vector3f InverseTransformPoint(const Vector3f& v) const UT_WARN_UNUSED {
        return RotateVectorByQuat(Conjugate(q), v - t) * Inverse(s);
    }

    Vector3f TransformVector(const Vector3f& v) const UT_WARN_UNUSED {
        return RotateVectorByQuat(q, v * s);
    }

    Vector3f InverseTransformVector(const Vector3f& v) const UT_WARN_UNUSED {
        return RotateVectorByQuat(Conjugate(q), v) * Inverse(s);
    }

    Vector3f TransformDirection(const Vector3f& v) const UT_WARN_UNUSED {
        return RotateVectorByQuat(q, v);
    }

    Vector3f InverseTransformDirection(const Vector3f& v) const UT_WARN_UNUSED {
        return RotateVectorByQuat(Conjugate(q), v);
    }
};


} // namespace Math
} // namespace ut
