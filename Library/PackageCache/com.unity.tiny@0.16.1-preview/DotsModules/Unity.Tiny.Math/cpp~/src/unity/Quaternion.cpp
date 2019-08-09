#include "UnityPrefix.h"
#include "Quaternion.h"

Quaternionf Slerp(const Quaternionf& q1, const Quaternionf& q2, float t)
{
    float dot = Dot(q1, q2);

    // dot = cos(theta)
    // if (dot < 0), q1 and q2 are more than 90 degrees apart,
    // so we can invert one to reduce spinning
    Quaternionf tmpQuat;
    if (dot < 0.0f)
    {
        dot = -dot;
        tmpQuat.Set(-q2.x,
            -q2.y,
            -q2.z,
            -q2.w);
    }
    else
        tmpQuat = q2;

    if (dot < 0.95f)
    {
        float angle = acos(dot);
        float sinadiv, sinat, sinaomt;
        sinadiv = 1.0f / sin(angle);
        sinat   = sin(angle * t);
        sinaomt = sin(angle * (1.0f - t));
        tmpQuat.Set((q1.x * sinaomt + tmpQuat.x * sinat) * sinadiv,
            (q1.y * sinaomt + tmpQuat.y * sinat) * sinadiv,
            (q1.z * sinaomt + tmpQuat.z * sinat) * sinadiv,
            (q1.w * sinaomt + tmpQuat.w * sinat) * sinadiv);
        return tmpQuat;
    }
    // if the angle is small, use linear interpolation
    else
    {
        return Lerp(q1, tmpQuat, t);
    }
}

float AngularDistance(const Quaternionf& lhs, const Quaternionf& rhs)
{
    float dot = Dot(lhs, rhs);
    if (dot < 0.0f)
        dot = -dot;
    return acos(std::min(1.0F, dot)) * 2.0F;
}

Quaternionf CreateQuaternionFromAxisQuaternions(const Quaternionf& q1, const Quaternionf& q2, const Quaternionf& q3, Quaternionf& result)
{
    result = (q1 * q2) * q3;
    Assert(CompareApproximately(SqrMagnitude(result), 1.0F));
    return result;
}

Quaternionf EulerToQuaternion(const Vector3f& someEulerAngles, math::RotationOrder order /*= math::kOrderUnityDefault*/)
{
    float cX(cos(someEulerAngles.x / 2.0f));
    float sX(sin(someEulerAngles.x / 2.0f));

    float cY(cos(someEulerAngles.y / 2.0f));
    float sY(sin(someEulerAngles.y / 2.0f));

    float cZ(cos(someEulerAngles.z / 2.0f));
    float sZ(sin(someEulerAngles.z / 2.0f));

    Quaternionf qX(sX, 0.0F, 0.0F, cX);
    Quaternionf qY(0.0F, sY, 0.0F, cY);
    Quaternionf qZ(0.0F, 0.0F, sZ, cZ);
    Quaternionf ret;

    switch (order)
    {
        case math::kOrderZYX: CreateQuaternionFromAxisQuaternions(qX, qY, qZ, ret); break;
        case math::kOrderYZX: CreateQuaternionFromAxisQuaternions(qX, qZ, qY, ret); break;
        case math::kOrderXZY: CreateQuaternionFromAxisQuaternions(qY, qZ, qX, ret); break;
        case math::kOrderZXY: CreateQuaternionFromAxisQuaternions(qY, qX, qZ, ret); break;
        case math::kOrderYXZ: CreateQuaternionFromAxisQuaternions(qZ, qX, qY, ret); break;
        case math::kOrderXYZ: CreateQuaternionFromAxisQuaternions(qZ, qY, qX, ret); break;
    }

    return ret;
}

namespace
{
    //Indexes for values used to calculate euler angles
    enum Indexes
    {
        X1,
        X2,
        Y1,
        Y2,
        Z1,
        Z2,
        singularity_test,
        IndexesCount
    };

    //indexes for pre-multiplied quaternion values
    enum QuatIndexes
    {
        xx,
        xy,
        xz,
        xw,
        yy,
        yz,
        yw,
        zz,
        zw,
        ww,
        QuatIndexesCount
    };

    float qAsin(float a, float b)
    {
        return a * asin(::clamp(b, -1.0f, 1.0f));
    }

    float qAtan2(float a, float b)
    {
        return atan2(a, b);
    }

    float qNull(float a, float b)
    {
        return 0;
    }

    typedef float (*qFunc)(float, float);

    qFunc qFuncs[math::kRotationOrderCount][3] =
    {
        {&qAtan2, &qAsin, &qAtan2},     //OrderXYZ
        {&qAtan2, &qAtan2, &qAsin},     //OrderXZY
        {&qAtan2, &qAtan2, &qAsin},     //OrderYZX,
        {&qAsin, &qAtan2, &qAtan2},     //OrderYXZ,
        {&qAsin, &qAtan2, &qAtan2},     //OrderZXY,
        {&qAtan2, &qAsin, &qAtan2},     //OrderZYX,
    };
}

Vector3f QuaternionToEuler(const Quaternionf& q, math::RotationOrder order /*= math::kOrderUnityDefault*/)
{
    AssertMsg(fabsf(SqrMagnitude(q) - 1.0f) < Vector3f::epsilon, "QuaternionToEuler: Input quaternion was not normalized");
    //setup all needed values
    float d[QuatIndexesCount] = {q.x * q.x, q.x * q.y, q.x * q.z, q.x * q.w, q.y * q.y, q.y * q.z, q.y * q.w, q.z * q.z, q.z * q.w, q.w * q.w};

    //Float array for values needed to calculate the angles
    float v[IndexesCount];
    qFunc f[3] = {qFuncs[order][0], qFuncs[order][1], qFuncs[order][2]}; //functions to be used to calculate angles

    const float SINGULARITY_CUTOFF = 0.499999f;
    Vector3f rot;
    switch (order)
    {
        case math::kOrderZYX:
            v[singularity_test] = d[xz] + d[yw];
            v[Z1] = 2.0f * (-d[xy] + d[zw]);
            v[Z2] = d[xx] - d[zz] - d[yy] + d[ww];
            v[Y1] = 1.0f;
            v[Y2] = 2.0f * v[singularity_test];
            if (Abs(v[singularity_test]) < SINGULARITY_CUTOFF)
            {
                v[X1] = 2.0f * (-d[yz] + d[xw]);
                v[X2] = d[zz] - d[yy] - d[xx] + d[ww];
            }
            else //x == xzx z == 0
            {
                float a, b, c, e;
                a = d[xz] + d[yw];
                b = -d[xy] + d[zw];
                c = d[xz] - d[yw];
                e = d[xy] + d[zw];

                v[X1] = a * e + b * c;
                v[X2] = b * e - a * c;
                f[2] = &qNull;
            }
            break;
        case math::kOrderXZY:
            v[singularity_test] = d[xy] + d[zw];
            v[X1] = 2.0f * (-d[yz] + d[xw]);
            v[X2] = d[yy] - d[zz] - d[xx] + d[ww];
            v[Z1] = 1.0f;
            v[Z2] = 2.0f * v[singularity_test];

            if (Abs(v[singularity_test]) < SINGULARITY_CUTOFF)
            {
                v[Y1] = 2.0f * (-d[xz] + d[yw]);
                v[Y2] = d[xx] - d[zz] - d[yy] + d[ww];
            }
            else //y == yxy x == 0
            {
                float a, b, c, e;
                a = d[xy] + d[zw];
                b = -d[yz] + d[xw];
                c = d[xy] - d[zw];
                e = d[yz] + d[xw];

                v[Y1] = a * e + b * c;
                v[Y2] = b * e - a * c;
                f[0] = &qNull;
            }
            break;

        case math::kOrderYZX:
            v[singularity_test] = d[xy] - d[zw];
            v[Y1] = 2.0f * (d[xz] + d[yw]);
            v[Y2] = d[xx] - d[zz] - d[yy] + d[ww];
            v[Z1] = -1.0f;
            v[Z2] = 2.0f * v[singularity_test];

            if (Abs(v[singularity_test]) < SINGULARITY_CUTOFF)
            {
                v[X1] = 2.0f * (d[yz] + d[xw]);
                v[X2] = d[yy] - d[xx] - d[zz] + d[ww];
            }
            else //x == xyx y == 0
            {
                float a, b, c, e;
                a = d[xy] - d[zw];
                b = d[xz] + d[yw];
                c = d[xy] + d[zw];
                e = -d[xz] + d[yw];

                v[X1] = a * e + b * c;
                v[X2] = b * e - a * c;
                f[1] = &qNull;
            }
            break;
        case math::kOrderZXY:
        {
            v[singularity_test] = d[yz] - d[xw];
            v[Z1] = 2.0f * (d[xy] + d[zw]);
            v[Z2] = d[yy] - d[zz] - d[xx] + d[ww];
            v[X1] = -1.0f;
            v[X2] = 2.0f * v[singularity_test];

            if (Abs(v[singularity_test]) < SINGULARITY_CUTOFF)
            {
                v[Y1] = 2.0f * (d[xz] + d[yw]);
                v[Y2] = d[zz] - d[xx] - d[yy] + d[ww];
            }
            else //x == yzy z == 0
            {
                float a, b, c, e;
                a = d[xy] + d[zw];
                b = -d[yz] + d[xw];
                c = d[xy] - d[zw];
                e = d[yz] + d[xw];

                v[Y1] = a * e + b * c;
                v[Y2] = b * e - a * c;
                f[2] = &qNull;
            }
        }
        break;
        case math::kOrderYXZ:
            v[singularity_test] = d[yz] + d[xw];
            v[Y1] = 2.0f * (-d[xz] + d[yw]);
            v[Y2] = d[zz] - d[yy] - d[xx] + d[ww];
            v[X1] = 1.0f;
            v[X2] = 2.0f * v[singularity_test];

            if (Abs(v[singularity_test]) < SINGULARITY_CUTOFF)
            {
                v[Z1] = 2.0f * (-d[xy] + d[zw]);
                v[Z2] = d[yy] - d[zz] - d[xx] + d[ww];
            }
            else //x == zyz y == 0
            {
                float a, b, c, e;
                a = d[yz] + d[xw];
                b = -d[xz] + d[yw];
                c = d[yz] - d[xw];
                e = d[xz] + d[yw];

                v[Z1] = a * e + b * c;
                v[Z2] = b * e - a * c;
                f[1] = &qNull;
            }
            break;
        case math::kOrderXYZ:
            v[singularity_test] = d[xz] - d[yw];
            v[X1] = 2.0f * (d[yz] + d[xw]);
            v[X2] = d[zz] - d[yy] - d[xx] + d[ww];
            v[Y1] = -1.0f;
            v[Y2] = 2.0f * v[singularity_test];

            if (Abs(v[singularity_test]) < SINGULARITY_CUTOFF)
            {
                v[Z1] = 2.0f * (d[xy] + d[zw]);
                v[Z2] = d[xx] - d[zz] - d[yy] + d[ww];
            }
            else //x == zxz x == 0
            {
                float a, b, c, e;
                a = d[xz] - d[yw];
                b = d[yz] + d[xw];
                c = d[xz] + d[yw];
                e = -d[yz] + d[xw];

                v[Z1] = a * e + b * c;
                v[Z2] = b * e - a * c;
                f[0] = &qNull;
            }
            break;
    }

    rot = Vector3f(f[0](v[X1], v[X2]),
            f[1](v[Y1], v[Y2]),
            f[2](v[Z1], v[Z2]));

    Assert(IsFinite(rot));

    return rot;
}

void QuaternionToMatrix(const Quaternionf& q, Matrix3x3f& m)
{
    // If q is guaranteed to be a unit quaternion, s will always
    // be 1.  In that case, this calculation can be optimized out.
    #if DEBUGMODE
    if (!CompareApproximately(SqrMagnitude(q), 1.0F, Vector3f::epsilon))
    {
        AssertString(Format("Quaternion To Matrix conversion failed because input Quaternion is invalid {%f, %f, %f, %f} l=%f", q.x, q.y, q.z, q.w, SqrMagnitude(q)));
    }
    #endif

    // Precalculate coordinate products
    float x = q.x * 2.0F;
    float y = q.y * 2.0F;
    float z = q.z * 2.0F;
    float xx = q.x * x;
    float yy = q.y * y;
    float zz = q.z * z;
    float xy = q.x * y;
    float xz = q.x * z;
    float yz = q.y * z;
    float wx = q.w * x;
    float wy = q.w * y;
    float wz = q.w * z;

    // Calculate 3x3 matrix from orthonormal basis
    m.m_Data[0] = 1.0f - (yy + zz);
    m.m_Data[1] = xy + wz;
    m.m_Data[2] = xz - wy;

    m.m_Data[3] = xy - wz;
    m.m_Data[4] = 1.0f - (xx + zz);
    m.m_Data[5] = yz + wx;

    m.m_Data[6]  = xz + wy;
    m.m_Data[7]  = yz - wx;
    m.m_Data[8] = 1.0f - (xx + yy);
}

void QuaternionToMatrix(const Quaternionf& q, Matrix4x4f& m)
{
    // If q is guaranteed to be a unit quaternion, s will always
    // be 1.  In that case, this calculation can be optimized out.
    #if DEBUGMODE
    if (!CompareApproximately(SqrMagnitude(q), 1.0F, Vector3f::epsilon))
    {
        AssertString(Format("Quaternion To Matrix conversion failed because input Quaternion is invalid {%f, %f, %f, %f} l=%f", q.x, q.y, q.z, q.w, SqrMagnitude(q)));
    }
    #endif

    // Precalculate coordinate products
    float x = q.x * 2.0F;
    float y = q.y * 2.0F;
    float z = q.z * 2.0F;
    float xx = q.x * x;
    float yy = q.y * y;
    float zz = q.z * z;
    float xy = q.x * y;
    float xz = q.x * z;
    float yz = q.y * z;
    float wx = q.w * x;
    float wy = q.w * y;
    float wz = q.w * z;

    // Calculate 3x3 matrix from orthonormal basis
    m.m_Data[0] = 1.0f - (yy + zz);
    m.m_Data[1] = xy + wz;
    m.m_Data[2] = xz - wy;
    m.m_Data[3] = 0.0F;

    m.m_Data[4] = xy - wz;
    m.m_Data[5] = 1.0f - (xx + zz);
    m.m_Data[6] = yz + wx;
    m.m_Data[7] = 0.0F;

    m.m_Data[8]  = xz + wy;
    m.m_Data[9]  = yz - wx;
    m.m_Data[10] = 1.0f - (xx + yy);
    m.m_Data[11] = 0.0F;

    m.m_Data[12] = 0.0F;
    m.m_Data[13] = 0.0F;
    m.m_Data[14] = 0.0F;
    m.m_Data[15] = 1.0F;
}

void MatrixToQuaternion(const Matrix4x4f& m, Quaternionf& q)
{
    Matrix3x3f mat(
        m.Get(0, 0), m.Get(0, 1), m.Get(0, 2),
        m.Get(1, 0), m.Get(1, 1), m.Get(1, 2),
        m.Get(2, 0), m.Get(2, 1), m.Get(2, 2));

    MatrixToQuaternion(mat, q);
}

void MatrixToQuaternion(const Matrix3x3f& kRot, Quaternionf& q)
{
    // Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
    // article "Quaternionf Calculus and Fast Animation".
    #if DEBUGMODE
    float det = kRot.GetDeterminant();
    Assert(CompareApproximately(det, 1.0F, .005f));
    #endif
    float fTrace = kRot.Get(0, 0) + kRot.Get(1, 1) + kRot.Get(2, 2);
    float fRoot;

    if (fTrace > 0.0f)
    {
        // |w| > 1/2, may as well choose w > 1/2
        fRoot = std::sqrt(fTrace + 1.0f);   // 2w
        q.w = 0.5f * fRoot;
        fRoot = 0.5f / fRoot;  // 1/(4w)
        q.x = (kRot.Get(2, 1) - kRot.Get(1, 2)) * fRoot;
        q.y = (kRot.Get(0, 2) - kRot.Get(2, 0)) * fRoot;
        q.z = (kRot.Get(1, 0) - kRot.Get(0, 1)) * fRoot;
    }
    else
    {
        // |w| <= 1/2
        int s_iNext[3] = { 1, 2, 0 };
        int i = 0;
        if (kRot.Get(1, 1) > kRot.Get(0, 0))
            i = 1;
        if (kRot.Get(2, 2) > kRot.Get(i, i))
            i = 2;
        int j = s_iNext[i];
        int k = s_iNext[j];

        fRoot = std::sqrt(kRot.Get(i, i) - kRot.Get(j, j) - kRot.Get(k, k) + 1.0f);
        float* apkQuat[3] = { &q.x, &q.y, &q.z };
        Assert(fRoot >= Vector3f::epsilon);
        *apkQuat[i] = 0.5f * fRoot;
        fRoot = 0.5f / fRoot;
        q.w = (kRot.Get(k, j) - kRot.Get(j, k)) * fRoot;
        *apkQuat[j] = (kRot.Get(j, i) + kRot.Get(i, j)) * fRoot;
        *apkQuat[k] = (kRot.Get(k, i) + kRot.Get(i, k)) * fRoot;
    }
    q = Normalize(q);
}

bool LookRotationToQuaternion(const Vector3f& viewVec, const Vector3f& upVec, Quaternionf* res)
{
    Matrix3x3f m;
    if (!LookRotationToMatrix(viewVec, upVec, &m))
        return false;
    MatrixToQuaternion(m, *res);
    return true;
}

Quaternionf FromToQuaternionSafe(const Vector3f& lhs, const Vector3f& rhs)
{
    float lhsMag = Magnitude(lhs);
    float rhsMag = Magnitude(rhs);
    if (lhsMag < Vector3f::epsilon || rhsMag < Vector3f::epsilon)
        return Quaternionf::identity();
    else
        return FromToQuaternion(lhs / lhsMag, rhs / rhsMag);
}

Quaternionf FromToQuaternion(const Vector3f& from, const Vector3f& to)
{
    Matrix3x3f m;
    m.SetFromToRotation(from, to);
    Quaternionf q;
    MatrixToQuaternion(m, q);
    return q;
}
