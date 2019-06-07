#ifndef MATRIX4X4_H
#define MATRIX4X4_H

#include "Vector2.h"
#include "Vector3.h"
#include "Vector4.h"

// Visual Studio generates bad code from x86 CopyMatrix4x4 SSE intrinsics. On x64 it uses SSE automatically.
// Only Neon benefits from custom copy constructors and assignment operators.
#define UNITY_USE_COPYMATRIX_4X4 (0)

#define MAT(m, r, c) (m)[(c)*4+(r)]

#define RETURN_ZERO PP_WRAP_CODE(\
    for (int i=0;i<16;i++) \
        out[i] = 0.0F; \
    return false;\
)

class Matrix3x3f;
class Matrix4x4f;
class Quaternionf;

struct FrustumPlanes
{
    float left;
    float right;
    float bottom;
    float top;
    float zNear;
    float zFar;
};

float ComputeUniformScale(const Matrix4x4f& matrix);

bool InvertMatrix4x4_Full(const float* m, float* out);
bool InvertMatrix4x4_General3D(const float* m, float* out);


/// Matrices in unity are column major.
class EXPORT_COREMODULE Matrix4x4f
{
public:
    float m_Data[16];

    enum InitIdentity { kIdentity };

    Matrix4x4f() {}  // Default ctor is intentionally empty for performance reasons
    explicit Matrix4x4f(InitIdentity) { SetIdentity(); }
#if UNITY_USE_COPYMATRIX_4X4
    Matrix4x4f(const Matrix4x4f &other);
#endif
    Matrix4x4f(const Matrix3x3f &other);
    explicit Matrix4x4f(const float data[16])
    {
        for (int i = 0; i < 16; i++)
            m_Data[i] = data[i];
    }

    float& Get(int row, int column)            { return m_Data[row + (column * 4)]; }
    const float& Get(int row, int column) const { return m_Data[row + (column * 4)]; }
    float* GetPtr()                            { return m_Data; }
    const float* GetPtr() const                 { return m_Data; }

    float operator[](int index) const         { return m_Data[index]; }
    float& operator[](int index)              { return m_Data[index]; }

    Matrix4x4f& operator*=(const Matrix4x4f& inM);

#if UNITY_USE_COPYMATRIX_4X4
    Matrix4x4f& operator=(const Matrix4x4f& m);
#endif
    Matrix4x4f& operator=(const Matrix3x3f& m);

    Vector3f MultiplyVector3(const Vector3f& inV) const;
    void MultiplyVector3(const Vector3f& inV, Vector3f& output) const;
    bool PerspectiveMultiplyVector3(const Vector3f& inV, Vector3f& output) const;
    Vector4f MultiplyVector4(const Vector4f& inV) const;
    void MultiplyVector4(const Vector4f& inV, Vector4f& output) const;
    Vector3f MultiplyPoint3(const Vector3f& inV) const;
    void MultiplyPoint3(const Vector3f& inV, Vector3f& output) const;
    Vector2f MultiplyPoint2(const Vector2f& inV) const;
    void MultiplyPoint2(const Vector2f& inV, Vector2f& output) const;
    bool PerspectiveMultiplyPoint3(const Vector3f& inV, Vector3f& output) const;
    Vector3f InverseMultiplyPoint3Affine(const Vector3f& inV) const;
    Vector3f InverseMultiplyVector3Affine(const Vector3f& inV) const;

    bool IsIdentity(float epsilon = Vector3f::epsilon) const;
    // Returns whether a matrix is a perspective projection transform (i.e. doesn't have 0,0,0,1 in the last column).
    bool IsPerspective() const { return (m_Data[3] != 0.0f || m_Data[7] != 0.0f || m_Data[11] != 0.0f || m_Data[15] != 1.0f); }
    // return maximum absolute scale, ~1 for no scale
    float MaxAbsScale() const;
    void RemoveScale();

    float GetDeterminant() const;
    float GetDeterminant2x2() const;

    Matrix4x4f& Invert_Full()
    {
        InvertMatrix4x4_Full(m_Data, m_Data);
        return *this;
    }

    static bool Invert_Full(const Matrix4x4f &inM, Matrix4x4f &outM)
    {
        return InvertMatrix4x4_Full(inM.m_Data, outM.m_Data);
    }

    static bool Invert_General3D(const Matrix4x4f &in, Matrix4x4f &out)
    {
        float pos, neg, t;
        float det;

        // Calculate the determinant of upper left 3x3 sub-matrix and
        // determine if the matrix is singular.
        pos = neg = 0.0;
        t = MAT(in, 0, 0) * MAT(in, 1, 1) * MAT(in, 2, 2);
        if (t >= 0.0)
            pos += t;
        else
            neg += t;

        t = MAT(in, 1, 0) * MAT(in, 2, 1) * MAT(in, 0, 2);
        if (t >= 0.0)
            pos += t;
        else
            neg += t;

        t = MAT(in, 2, 0) * MAT(in, 0, 1) * MAT(in, 1, 2);
        if (t >= 0.0)
            pos += t;
        else
            neg += t;

        t = -MAT(in, 2, 0) * MAT(in, 1, 1) * MAT(in, 0, 2);
        if (t >= 0.0)
            pos += t;
        else
            neg += t;

        t = -MAT(in, 1, 0) * MAT(in, 0, 1) * MAT(in, 2, 2);
        if (t >= 0.0)
            pos += t;
        else
            neg += t;

        t = -MAT(in, 0, 0) * MAT(in, 2, 1) * MAT(in, 1, 2);
        if (t >= 0.0)
            pos += t;
        else
            neg += t;

        det = pos + neg;

        if (det * det < 1e-25)
            RETURN_ZERO;

        det = 1.0F / det;
        MAT(out, 0, 0) = ((MAT(in, 1, 1) * MAT(in, 2, 2) - MAT(in, 2, 1) * MAT(in, 1, 2)) * det);
        MAT(out, 0, 1) = (-(MAT(in, 0, 1) * MAT(in, 2, 2) - MAT(in, 2, 1) * MAT(in, 0, 2)) * det);
        MAT(out, 0, 2) = ((MAT(in, 0, 1) * MAT(in, 1, 2) - MAT(in, 1, 1) * MAT(in, 0, 2)) * det);
        MAT(out, 1, 0) = (-(MAT(in, 1, 0) * MAT(in, 2, 2) - MAT(in, 2, 0) * MAT(in, 1, 2)) * det);
        MAT(out, 1, 1) = ((MAT(in, 0, 0) * MAT(in, 2, 2) - MAT(in, 2, 0) * MAT(in, 0, 2)) * det);
        MAT(out, 1, 2) = (-(MAT(in, 0, 0) * MAT(in, 1, 2) - MAT(in, 1, 0) * MAT(in, 0, 2)) * det);
        MAT(out, 2, 0) = ((MAT(in, 1, 0) * MAT(in, 2, 1) - MAT(in, 2, 0) * MAT(in, 1, 1)) * det);
        MAT(out, 2, 1) = (-(MAT(in, 0, 0) * MAT(in, 2, 1) - MAT(in, 2, 0) * MAT(in, 0, 1)) * det);
        MAT(out, 2, 2) = ((MAT(in, 0, 0) * MAT(in, 1, 1) - MAT(in, 1, 0) * MAT(in, 0, 1)) * det);

        // Do the translation part
        MAT(out, 0, 3) = -(MAT(in, 0, 3) * MAT(out, 0, 0) +
            MAT(in, 1, 3) * MAT(out, 0, 1) +
            MAT(in, 2, 3) * MAT(out, 0, 2));
        MAT(out, 1, 3) = -(MAT(in, 0, 3) * MAT(out, 1, 0) +
            MAT(in, 1, 3) * MAT(out, 1, 1) +
            MAT(in, 2, 3) * MAT(out, 1, 2));
        MAT(out, 2, 3) = -(MAT(in, 0, 3) * MAT(out, 2, 0) +
            MAT(in, 1, 3) * MAT(out, 2, 1) +
            MAT(in, 2, 3) * MAT(out, 2, 2));

        MAT(out, 3, 0) = 0.0f;
        MAT(out, 3, 1) = 0.0f;
        MAT(out, 3, 2) = 0.0f;
        MAT(out, 3, 3) = 1.0f;

        return true;
    }

    Matrix4x4f& Transpose();

    Matrix4x4f& SetIdentity();
    Matrix4x4f& SetZero();
    Matrix4x4f& SetPerspective(float fovy, float aspect, float zNear, float zFar);
    // rad = Deg2Rad(fovy/2), contanHalfFOV = cos(rad)/sin(rad)
    Matrix4x4f& SetPerspectiveCotan(float cotanHalfFOV, float zNear, float zFar);
    Matrix4x4f& SetOrtho(float left, float right, float bottom, float top, float zNear, float zFar);
    Matrix4x4f& SetFrustum(float left, float right, float bottom, float top, float nearval, float farval);
    Matrix4x4f& AdjustDepthRange(float origNear, float newNear, float newFar);

    Vector3f GetAxisX() const;
    Vector3f GetAxisY() const;
    Vector3f GetAxisZ() const;
    Vector3f GetAxis(int axis) const;
    Vector3f GetPosition() const;
    Quaternionf GetRotation() const;
    Vector3f GetLossyScale() const;
    Vector4f GetRow(int row) const;
    Vector4f GetColumn(int col) const;
    // these set only these components of the matrix, everything else is untouched!
    void SetAxisX(const Vector3f& v);
    void SetAxisY(const Vector3f& v);
    void SetAxisZ(const Vector3f& v);
    void SetAxis(int axis, const Vector3f& v);
    void SetPosition(const Vector3f& v);
    void SetRow(int row, const Vector4f& v);
    void SetColumn(int col, const Vector4f& v);

    //TODO: bring back the definition to cpp once we figured out why we can't link tiny.math from non-webgl builds
    Matrix4x4f& SetTranslate(const Vector3f& inTrans)
    {
        Get(0, 0) = 1.0;   Get(0, 1) = 0.0;   Get(0, 2) = 0.0;   Get(0, 3) = inTrans[0];
        Get(1, 0) = 0.0;   Get(1, 1) = 1.0;   Get(1, 2) = 0.0;   Get(1, 3) = inTrans[1];
        Get(2, 0) = 0.0;   Get(2, 1) = 0.0;   Get(2, 2) = 1.0;   Get(2, 3) = inTrans[2];
        Get(3, 0) = 0.0;   Get(3, 1) = 0.0;   Get(3, 2) = 0.0;   Get(3, 3) = 1.0;
        return *this;
    }
    Matrix4x4f& SetBasis(const Vector3f& inX, const Vector3f& inY, const Vector3f& inZ);
    Matrix4x4f& SetBasisTransposed(const Vector3f& inX, const Vector3f& inY, const Vector3f& inZ);

    //TODO: bring back the definition to cpp once we figured out why we can't link tiny.math from non-webgl builds
    Matrix4x4f& SetScale(const Vector3f& inScale)
    {
        Get(0, 0) = inScale[0];    Get(0, 1) = 0.0;           Get(0, 2) = 0.0;           Get(0, 3) = 0.0;
        Get(1, 0) = 0.0;           Get(1, 1) = inScale[1];    Get(1, 2) = 0.0;           Get(1, 3) = 0.0;
        Get(2, 0) = 0.0;           Get(2, 1) = 0.0;           Get(2, 2) = inScale[2];    Get(2, 3) = 0.0;
        Get(3, 0) = 0.0;           Get(3, 1) = 0.0;           Get(3, 2) = 0.0;           Get(3, 3) = 1.0;
        return *this;
    }
    Matrix4x4f& SetScaleAndPosition(const Vector3f& inScale, const Vector3f& inPosition);
    Matrix4x4f& SetPositionAndOrthoNormalBasis(const Vector3f& inPosition, const Vector3f& inX, const Vector3f& inY, const Vector3f& inZ);

    Matrix4x4f& Translate(const Vector3f& inTrans);
    Matrix4x4f& Scale(const Vector3f& inScale);

    Matrix4x4f& SetFromToRotation(const Vector3f& from, const Vector3f& to);

    void SetTR(const Vector3f& pos, const Quaternionf& q);
    void SetTRS(const Vector3f& pos, const Quaternionf& q, const Vector3f& s);
    void SetTRInverse(const Vector3f& pos, const Quaternionf& q);
    FrustumPlanes DecomposeProjection() const;
    static const Matrix4x4f identity;
    bool ValidTRS() const;
};

bool CompareApproximately(const Matrix4x4f& lhs, const Matrix4x4f& rhs, float dist = Vector3f::epsilon);

/// Transforms an array of vertices. input may be the same as output.
void EXPORT_COREMODULE TransformPoints3x3(const Matrix4x4f &matrix, const Vector3f* input, Vector3f* ouput, int count);
void EXPORT_COREMODULE TransformPoints3x4(const Matrix4x4f &matrix, const Vector3f* input, Vector3f* ouput, int count);
void EXPORT_COREMODULE TransformPoints3x3(const Matrix4x4f &matrix, const Vector3f* input, size_t inStride, Vector3f* ouput, size_t outStride, int count);
void EXPORT_COREMODULE TransformPoints3x4(const Matrix4x4f &matrix, const Vector3f* input, size_t inStride, Vector3f* ouput, size_t outStride, int count);

void MultiplyMatrices3x4(const Matrix4x4f& lhs, const Matrix4x4f& rhs, Matrix4x4f& res);
void MultiplyMatrices2D(const Matrix4x4f& lhs, const Matrix4x4f& rhs, Matrix4x4f& res);

void CopyMatrix4x4REF(const float* __restrict lhs, float* __restrict res);
void TransposeMatrix4x4REF(const Matrix4x4f* __restrict lhs, Matrix4x4f* __restrict res);

// foreach R[i] = A[i] * B[i]
void MultiplyMatrixArray4x4REF(const Matrix4x4f* __restrict arrayA, const Matrix4x4f* __restrict arrayB,
    Matrix4x4f* __restrict arrayRes, size_t count);
// foreach R[i] = BASE * A[i] * B[i]
void MultiplyMatrixArrayWithBase4x4REF(const Matrix4x4f* __restrict base,
    const Matrix4x4f* __restrict arrayA, const Matrix4x4f* __restrict arrayB,
    Matrix4x4f* __restrict arrayRes, size_t count);

#if UNITY_SUPPORTS_SSE
    #include "Simd/Matrix4x4Simd.h"
#elif UNITY_SUPPORTS_NEON

    #if UNITY_USE_PREFIX_EXTERN_SYMBOLS
        #define MultiplyMatrices4x4_NEON                _MultiplyMatrices4x4_NEON
        #define CopyMatrix4x4_NEON                      _CopyMatrix4x4_NEON
        #define TransposeMatrix4x4_NEON                 _TransposeMatrix4x4_NEON
        #define MultiplyMatrixArray4x4_NEON             _MultiplyMatrixArray4x4_NEON
        #define MultiplyMatrixArrayWithBase4x4_NEON     _MultiplyMatrixArrayWithBase4x4_NEON
    #endif

extern "C"
{
void CopyMatrix4x4_NEON(const float* __restrict lhs, float* __restrict res);
void TransposeMatrix4x4_NEON(const Matrix4x4f* __restrict lhs, Matrix4x4f* __restrict res);

void MultiplyMatrices4x4_NEON(const Matrix4x4f* __restrict lhs, const Matrix4x4f* __restrict rhs, Matrix4x4f* __restrict res);
void MultiplyMatrixArray4x4_NEON(const Matrix4x4f* __restrict arrayA, const Matrix4x4f* __restrict arrayB,
    Matrix4x4f* __restrict arrayRes, size_t count);
void MultiplyMatrixArrayWithBase4x4_NEON(const Matrix4x4f* __restrict base,
    const Matrix4x4f* __restrict arrayA, const Matrix4x4f* __restrict arrayB,
    Matrix4x4f* __restrict arrayRes, size_t count);
}

    #define CopyMatrix4x4       CopyMatrix4x4_NEON
    #define TransposeMatrix4x4  TransposeMatrix4x4_NEON
    #define MultiplyMatrices4x4 MultiplyMatrices4x4_NEON
    #define MultiplyMatrixArray4x4          MultiplyMatrixArray4x4_NEON
    #define MultiplyMatrixArrayWithBase4x4  MultiplyMatrixArrayWithBase4x4_NEON

#elif PLATFORM_WIIU

void TransposeMatrix4x4(const Matrix4x4f* __restrict lhs, Matrix4x4f* __restrict res);
void MultiplyMatrices4x4(const Matrix4x4f* __restrict lhs, const Matrix4x4f* __restrict rhs, Matrix4x4f* __restrict res);

    #define MultiplyMatrixArray4x4          MultiplyMatrixArray4x4REF
    #define MultiplyMatrixArrayWithBase4x4  MultiplyMatrixArrayWithBase4x4REF

#else

    #define TransposeMatrix4x4              TransposeMatrix4x4REF
    #define MultiplyMatrices4x4             MultiplyMatrices4x4REF
    #define MultiplyMatrixArray4x4          MultiplyMatrixArray4x4REF
    #define MultiplyMatrixArrayWithBase4x4  MultiplyMatrixArrayWithBase4x4REF

#endif

#if UNITY_USE_COPYMATRIX_4X4
inline Matrix4x4f::Matrix4x4f(const Matrix4x4f &other)
{
    CopyMatrix4x4(other.GetPtr(), GetPtr());
}

inline Matrix4x4f& Matrix4x4f::operator=(const Matrix4x4f& m)
{
    CopyMatrix4x4(m.GetPtr(), GetPtr());
    return *this;
}

#endif

//TODO: bring back the definition to cpp once we figured out why we can't link tiny.math from non-webgl builds
inline void MultiplyMatrices4x4REF(const Matrix4x4f* __restrict lhs, const Matrix4x4f* __restrict rhs, Matrix4x4f* __restrict res)
{
    Assert(lhs != rhs && lhs != res && rhs != res);
    for (int i = 0; i < 4; i++)
    {
        res->m_Data[i] = lhs->m_Data[i] * rhs->m_Data[0] + lhs->m_Data[i + 4] * rhs->m_Data[1] + lhs->m_Data[i + 8] * rhs->m_Data[2] + lhs->m_Data[i + 12] * rhs->m_Data[3];
        res->m_Data[i + 4] = lhs->m_Data[i] * rhs->m_Data[4] + lhs->m_Data[i + 4] * rhs->m_Data[5] + lhs->m_Data[i + 8] * rhs->m_Data[6] + lhs->m_Data[i + 12] * rhs->m_Data[7];
        res->m_Data[i + 8] = lhs->m_Data[i] * rhs->m_Data[8] + lhs->m_Data[i + 4] * rhs->m_Data[9] + lhs->m_Data[i + 8] * rhs->m_Data[10] + lhs->m_Data[i + 12] * rhs->m_Data[11];
        res->m_Data[i + 12] = lhs->m_Data[i] * rhs->m_Data[12] + lhs->m_Data[i + 4] * rhs->m_Data[13] + lhs->m_Data[i + 8] * rhs->m_Data[14] + lhs->m_Data[i + 12] * rhs->m_Data[15];
    }
}

inline Vector3f Matrix4x4f::GetAxisX() const
{
    return Vector3f(Get(0, 0), Get(1, 0), Get(2, 0));
}

inline Vector3f Matrix4x4f::GetAxisY() const
{
    return Vector3f(Get(0, 1), Get(1, 1), Get(2, 1));
}

inline Vector3f Matrix4x4f::GetAxisZ() const
{
    return Vector3f(Get(0, 2), Get(1, 2), Get(2, 2));
}

inline Vector3f Matrix4x4f::GetAxis(int axis) const
{
    return Vector3f(Get(0, axis), Get(1, axis), Get(2, axis));
}

inline Vector3f Matrix4x4f::GetPosition() const
{
    return Vector3f(Get(0, 3), Get(1, 3), Get(2, 3));
}

inline Vector4f Matrix4x4f::GetRow(int row) const
{
    return Vector4f(Get(row, 0), Get(row, 1), Get(row, 2), Get(row, 3));
}

inline Vector4f Matrix4x4f::GetColumn(int col) const
{
    return Vector4f(Get(0, col), Get(1, col), Get(2, col), Get(3, col));
}

inline void Matrix4x4f::SetAxisX(const Vector3f& v)
{
    Get(0, 0) = v.x; Get(1, 0) = v.y; Get(2, 0) = v.z;
}

inline void Matrix4x4f::SetAxisY(const Vector3f& v)
{
    Get(0, 1) = v.x; Get(1, 1) = v.y; Get(2, 1) = v.z;
}

inline void Matrix4x4f::SetAxisZ(const Vector3f& v)
{
    Get(0, 2) = v.x; Get(1, 2) = v.y; Get(2, 2) = v.z;
}

inline void Matrix4x4f::SetAxis(int axis, const Vector3f& v)
{
    Get(0, axis) = v.x; Get(1, axis) = v.y; Get(2, axis) = v.z;
}

inline void Matrix4x4f::SetPosition(const Vector3f& v)
{
    Get(0, 3) = v.x; Get(1, 3) = v.y; Get(2, 3) = v.z;
}

inline void Matrix4x4f::SetRow(int row, const Vector4f& v)
{
    Get(row, 0) = v.x; Get(row, 1) = v.y; Get(row, 2) = v.z; Get(row, 3) = v.w;
}

inline void Matrix4x4f::SetColumn(int col, const Vector4f& v)
{
    Get(0, col) = v.x; Get(1, col) = v.y; Get(2, col) = v.z; Get(3, col) = v.w;
}

inline Vector3f Matrix4x4f::MultiplyPoint3(const Vector3f& v) const
{
    Vector3f res;
    res.x = m_Data[0] * v.x + m_Data[4] * v.y + m_Data[8] * v.z + m_Data[12];
    res.y = m_Data[1] * v.x + m_Data[5] * v.y + m_Data[9] * v.z + m_Data[13];
    res.z = m_Data[2] * v.x + m_Data[6] * v.y + m_Data[10] * v.z + m_Data[14];
    return res;
}

inline void Matrix4x4f::MultiplyPoint3(const Vector3f& v, Vector3f& output) const
{
    output.x = m_Data[0] * v.x + m_Data[4] * v.y + m_Data[8] * v.z + m_Data[12];
    output.y = m_Data[1] * v.x + m_Data[5] * v.y + m_Data[9] * v.z + m_Data[13];
    output.z = m_Data[2] * v.x + m_Data[6] * v.y + m_Data[10] * v.z + m_Data[14];
}

inline Vector2f Matrix4x4f::MultiplyPoint2(const Vector2f& v) const
{
    Vector2f res;
    res.x = m_Data[0] * v.x + m_Data[4] * v.y + m_Data[12];
    res.y = m_Data[1] * v.x + m_Data[5] * v.y + m_Data[13];
    return res;
}

inline void Matrix4x4f::MultiplyPoint2(const Vector2f& v, Vector2f& output) const
{
    output.x = m_Data[0] * v.x + m_Data[4] * v.y + m_Data[12];
    output.y = m_Data[1] * v.x + m_Data[5] * v.y + m_Data[13];
}

inline Vector3f Matrix4x4f::MultiplyVector3(const Vector3f& v) const
{
    Vector3f res;
    res.x = m_Data[0] * v.x + m_Data[4] * v.y + m_Data[8] * v.z;
    res.y = m_Data[1] * v.x + m_Data[5] * v.y + m_Data[9] * v.z;
    res.z = m_Data[2] * v.x + m_Data[6] * v.y + m_Data[10] * v.z;
    return res;
}

inline void Matrix4x4f::MultiplyVector3(const Vector3f& v, Vector3f& output) const
{
    output.x = m_Data[0] * v.x + m_Data[4] * v.y + m_Data[8] * v.z;
    output.y = m_Data[1] * v.x + m_Data[5] * v.y + m_Data[9] * v.z;
    output.z = m_Data[2] * v.x + m_Data[6] * v.y + m_Data[10] * v.z;
}

inline bool Matrix4x4f::PerspectiveMultiplyPoint3(const Vector3f& v, Vector3f& output) const
{
    Vector3f res;
    float w;
    res.x = Get(0, 0) * v.x + Get(0, 1) * v.y + Get(0, 2) * v.z + Get(0, 3);
    res.y = Get(1, 0) * v.x + Get(1, 1) * v.y + Get(1, 2) * v.z + Get(1, 3);
    res.z = Get(2, 0) * v.x + Get(2, 1) * v.y + Get(2, 2) * v.z + Get(2, 3);
    w     = Get(3, 0) * v.x + Get(3, 1) * v.y + Get(3, 2) * v.z + Get(3, 3);
    if (Abs(w) > 1.0e-7f)
    {
        float invW = 1.0f / w;
        output.x = res.x * invW;
        output.y = res.y * invW;
        output.z = res.z * invW;
        return true;
    }
    else
    {
        output.x = 0.0f;
        output.y = 0.0f;
        output.z = 0.0f;
        return false;
    }
}

inline Vector4f Matrix4x4f::MultiplyVector4(const Vector4f& v) const
{
    Vector4f res;
    MultiplyVector4(v, res);
    return res;
}

inline void Matrix4x4f::MultiplyVector4(const Vector4f& v, Vector4f& output) const
{
    output.x = m_Data[0] * v.x + m_Data[4] * v.y + m_Data[8] * v.z + m_Data[12] * v.w;
    output.y = m_Data[1] * v.x + m_Data[5] * v.y + m_Data[9] * v.z + m_Data[13] * v.w;
    output.z = m_Data[2] * v.x + m_Data[6] * v.y + m_Data[10] * v.z + m_Data[14] * v.w;
    output.w = m_Data[3] * v.x + m_Data[7] * v.y + m_Data[11] * v.z + m_Data[15] * v.w;
}

inline bool Matrix4x4f::PerspectiveMultiplyVector3(const Vector3f& v, Vector3f& output) const
{
    Vector3f res;
    float w;
    res.x = Get(0, 0) * v.x + Get(0, 1) * v.y + Get(0, 2) * v.z;
    res.y = Get(1, 0) * v.x + Get(1, 1) * v.y + Get(1, 2) * v.z;
    res.z = Get(2, 0) * v.x + Get(2, 1) * v.y + Get(2, 2) * v.z;
    w     = Get(3, 0) * v.x + Get(3, 1) * v.y + Get(3, 2) * v.z;
    if (Abs(w) > 1.0e-7f)
    {
        float invW = 1.0f / w;
        output.x = res.x * invW;
        output.y = res.y * invW;
        output.z = res.z * invW;
        return true;
    }
    else
    {
        output.x = 0.0f;
        output.y = 0.0f;
        output.z = 0.0f;
        return false;
    }
}

inline Vector3f Matrix4x4f::InverseMultiplyPoint3Affine(const Vector3f& inV) const
{
    Vector3f v(inV.x - Get(0, 3), inV.y - Get(1, 3), inV.z - Get(2, 3));
    Vector3f res;
    res.x = Get(0, 0) * v.x + Get(1, 0) * v.y + Get(2, 0) * v.z;
    res.y = Get(0, 1) * v.x + Get(1, 1) * v.y + Get(2, 1) * v.z;
    res.z = Get(0, 2) * v.x + Get(1, 2) * v.y + Get(2, 2) * v.z;
    return res;
}

inline Vector3f Matrix4x4f::InverseMultiplyVector3Affine(const Vector3f& v) const
{
    Vector3f res;
    res.x = Get(0, 0) * v.x + Get(1, 0) * v.y + Get(2, 0) * v.z;
    res.y = Get(0, 1) * v.x + Get(1, 1) * v.y + Get(2, 1) * v.z;
    res.z = Get(0, 2) * v.x + Get(1, 2) * v.y + Get(2, 2) * v.z;
    return res;
}

inline bool IsFinite(const Matrix4x4f& f)
{
    return
        IsFinite(f.m_Data[0]) & IsFinite(f.m_Data[1]) & IsFinite(f.m_Data[2]) &
        IsFinite(f.m_Data[4]) & IsFinite(f.m_Data[5]) & IsFinite(f.m_Data[6]) &
        IsFinite(f.m_Data[8]) & IsFinite(f.m_Data[9]) & IsFinite(f.m_Data[10]) &
        IsFinite(f.m_Data[12]) & IsFinite(f.m_Data[13]) & IsFinite(f.m_Data[14]) & IsFinite(f.m_Data[15]);
}

#if ENABLE_UNIT_TESTS
#include "Runtime/Testing/TestingForwardDecls.h"

std::ostream& operator<<(std::ostream& stream, const Matrix4x4f& m);

namespace UnitTest
{
    template<> inline bool AreClose(Matrix4x4f const& expected, Matrix4x4f const& actual, float const& tolerance)
    {
        return CompareApproximately(expected, actual, tolerance);
    }
}
#endif

#endif
