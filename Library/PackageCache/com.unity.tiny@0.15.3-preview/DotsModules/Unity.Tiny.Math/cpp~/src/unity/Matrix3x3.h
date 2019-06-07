#pragma once

#include "Vector3.h"

class EXPORT_COREMODULE Matrix3x3f
{
public:

    float m_Data[9];

    Matrix3x3f() {}  // Default ctor is intentionally empty for performance reasons
    Matrix3x3f(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22) { Get(0, 0) = m00; Get(1, 0) = m10; Get(2, 0) = m20; Get(0, 1) = m01; Get(1, 1) = m11; Get(2, 1) = m21; Get(0, 2) = m02; Get(1, 2) = m12; Get(2, 2) = m22; }
    explicit Matrix3x3f(const class Matrix4x4f& m);
    // The Get function accesses the matrix in std math convention
    // m0,0 m0,1 m0,2
    // m1,0 m1,1 m1,2
    // m2,0 m2,1 m2,2

    // The floats are laid out:
    // m0   m3   m6
    // m1   m4   m7
    // m2   m5   m8


    float& Get(int row, int column)                { return m_Data[row + (column * 3)]; }
    const float& Get(int row, int column) const     { return m_Data[row + (column * 3)]; }

    float& operator[](int row)                { return m_Data[row]; }
    float operator[](int row) const               { return m_Data[row]; }

    float* GetPtr()                                { return m_Data; }
    const float* GetPtr() const                { return m_Data; }

    Vector3f GetColumn(int col) const { return Vector3f(Get(0, col), Get(1, col), Get(2, col)); }
    Matrix3x3f& operator=(const class Matrix4x4f& m);

    Matrix3x3f& operator*=(const Matrix3x3f& inM);
    Matrix3x3f& operator*=(const class Matrix4x4f& inM);
    friend Matrix3x3f operator*(const Matrix3x3f& lhs, const Matrix3x3f& rhs);
    Vector3f MultiplyVector3(const Vector3f& inV) const;
    void MultiplyVector3(const Vector3f& inV, Vector3f& output) const;

    Vector3f MultiplyPoint3(const Vector3f& inV) const                 { return MultiplyVector3(inV); }
    Vector3f MultiplyVector3Transpose(const Vector3f& inV) const;
    Vector3f MultiplyPoint3Transpose(const Vector3f& inV) const        { return MultiplyVector3Transpose(inV); }

    Matrix3x3f& operator*=(float f);
    Matrix3x3f& operator/=(float f) { return *this *= (1.0F / f); }

    float GetDeterminant() const;

    //  Matrix3x3f& Transpose (const Matrix3x3f& inM);
    Matrix3x3f& Transpose();
    //  Matrix3x3f& Invert (const Matrix3x3f& inM)                                              { return Transpose (inM); }
    bool Invert();
    void InvertTranspose();

    Matrix3x3f& SetIdentity();
    Matrix3x3f& SetZero();
    Matrix3x3f& SetFromToRotation(const Vector3f& from, const Vector3f& to);
    Matrix3x3f& SetAxisAngle(const Vector3f& rotationAxis, float radians);
    Matrix3x3f& SetBasis(const Vector3f& inX, const Vector3f& inY, const Vector3f& inZ);
    Matrix3x3f& SetBasisTransposed(const Vector3f& inX, const Vector3f& inY, const Vector3f& inZ);
    Matrix3x3f& SetScale(const Vector3f& inScale);
    Matrix3x3f& Scale(const Vector3f& inScale);

    bool IsIdentity(float threshold = Vector3f::epsilon);

    static const Matrix3x3f zero;
    static const Matrix3x3f identity;
};

// Generates an orthornormal basis from a look at rotation, returns if it was successful
// (Righthanded)
bool LookRotationToMatrix(const Vector3f& viewVec, const Vector3f& upVec, Matrix3x3f* m);

void EulerToMatrix(const Vector3f& v, Matrix3x3f& matrix);

inline Vector3f Matrix3x3f::MultiplyVector3(const Vector3f& v) const
{
    Vector3f res;
    res.x = m_Data[0] * v.x + m_Data[3] * v.y + m_Data[6] * v.z;
    res.y = m_Data[1] * v.x + m_Data[4] * v.y + m_Data[7] * v.z;
    res.z = m_Data[2] * v.x + m_Data[5] * v.y + m_Data[8] * v.z;
    return res;
}

inline void Matrix3x3f::MultiplyVector3(const Vector3f& v, Vector3f& output) const
{
    output.x = m_Data[0] * v.x + m_Data[3] * v.y + m_Data[6] * v.z;
    output.y = m_Data[1] * v.x + m_Data[4] * v.y + m_Data[7] * v.z;
    output.z = m_Data[2] * v.x + m_Data[5] * v.y + m_Data[8] * v.z;
}

inline void MultiplyMatrices3x3(const Matrix3x3f* __restrict lhs, const Matrix3x3f* __restrict rhs, Matrix3x3f* __restrict res)
{
    Assert(lhs != rhs && lhs != res && rhs != res);
    for (int i = 0; i < 3; ++i)
    {
        res->m_Data[i]    = lhs->m_Data[i] * rhs->m_Data[0]  + lhs->m_Data[i + 3] * rhs->m_Data[1]  + lhs->m_Data[i + 6] * rhs->m_Data[2];
        res->m_Data[i + 3]  = lhs->m_Data[i] * rhs->m_Data[3]  + lhs->m_Data[i + 3] * rhs->m_Data[4]  + lhs->m_Data[i + 6] * rhs->m_Data[5];
        res->m_Data[i + 6]  = lhs->m_Data[i] * rhs->m_Data[6]  + lhs->m_Data[i + 3] * rhs->m_Data[7]  + lhs->m_Data[i + 6] * rhs->m_Data[8];
    }
}

inline Matrix3x3f operator*(const Matrix3x3f& lhs, const Matrix3x3f& rhs)
{
    Matrix3x3f temp;
    MultiplyMatrices3x3(&lhs, &rhs, &temp);
    return temp;
}

inline Vector3f Matrix3x3f::MultiplyVector3Transpose(const Vector3f& v) const
{
    Vector3f res;
    res.x = Get(0, 0) * v.x + Get(1, 0) * v.y + Get(2, 0) * v.z;
    res.y = Get(0, 1) * v.x + Get(1, 1) * v.y + Get(2, 1) * v.z;
    res.z = Get(0, 2) * v.x + Get(1, 2) * v.y + Get(2, 2) * v.z;
    return res;
}

void EXPORT_COREMODULE OrthoNormalize(Matrix3x3f& matrix);
