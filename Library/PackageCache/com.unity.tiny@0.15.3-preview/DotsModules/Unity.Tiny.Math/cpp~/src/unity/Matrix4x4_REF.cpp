#include "UnityPrefix.h"
#include "Matrix4x4.h"

void TransposeMatrix4x4REF(const Matrix4x4f* __restrict lhs, Matrix4x4f* __restrict res)
{
    *res = *lhs;
    std::swap(res->Get(0, 1), res->Get(1, 0));
    std::swap(res->Get(0, 2), res->Get(2, 0));
    std::swap(res->Get(0, 3), res->Get(3, 0));
    std::swap(res->Get(1, 2), res->Get(2, 1));
    std::swap(res->Get(1, 3), res->Get(3, 1));
    std::swap(res->Get(2, 3), res->Get(3, 2));
}

/*void MultiplyMatrices4x4REF(const Matrix4x4f* __restrict lhs, const Matrix4x4f* __restrict rhs, Matrix4x4f* __restrict res)
{
    Assert(lhs != rhs && lhs != res && rhs != res);
    for (int i = 0; i < 4; i++)
    {
        res->m_Data[i]    = lhs->m_Data[i] * rhs->m_Data[0]  + lhs->m_Data[i + 4] * rhs->m_Data[1]  + lhs->m_Data[i + 8] * rhs->m_Data[2]  + lhs->m_Data[i + 12] * rhs->m_Data[3];
        res->m_Data[i + 4]  = lhs->m_Data[i] * rhs->m_Data[4]  + lhs->m_Data[i + 4] * rhs->m_Data[5]  + lhs->m_Data[i + 8] * rhs->m_Data[6]  + lhs->m_Data[i + 12] * rhs->m_Data[7];
        res->m_Data[i + 8]  = lhs->m_Data[i] * rhs->m_Data[8]  + lhs->m_Data[i + 4] * rhs->m_Data[9]  + lhs->m_Data[i + 8] * rhs->m_Data[10] + lhs->m_Data[i + 12] * rhs->m_Data[11];
        res->m_Data[i + 12] = lhs->m_Data[i] * rhs->m_Data[12] + lhs->m_Data[i + 4] * rhs->m_Data[13] + lhs->m_Data[i + 8] * rhs->m_Data[14] + lhs->m_Data[i + 12] * rhs->m_Data[15];
    }
}*/

void MultiplyMatrixArray4x4REF(const Matrix4x4f* __restrict a, const Matrix4x4f* __restrict b, Matrix4x4f* __restrict res, size_t count)
{
    Assert(a);
    Assert(b);
    Assert(res);

    for (size_t i = 0; i < count; ++i)
    {
        MultiplyMatrices4x4(a + i, b + i, res + i);
    }
}

void MultiplyMatrixArrayWithBase4x4REF(const Matrix4x4f* __restrict base,
    const Matrix4x4f* __restrict a, const Matrix4x4f* __restrict b, Matrix4x4f* __restrict res, size_t count)
{
    Assert(base);
    Assert(a);
    Assert(b);
    Assert(res);

    Matrix4x4f tmp;
    for (size_t i = 0; i < count; ++i)
    {
        MultiplyMatrices4x4(base, a + i, &tmp);
        MultiplyMatrices4x4(&tmp, b + i, res + i);
    }
}
