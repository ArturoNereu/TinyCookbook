#pragma once
#include <limits.h>
#include <limits>
#include <cstdint>
#include <cassert>
#include <functional>

#include "ArrayUtility.h"
#include "GeminiAssert.h"

#define DebugAssert(expr)
#define AssertMsg(expr, msg)

#define UT_WARN_UNUSED

using SInt8 = std::int8_t;
using SInt16 = std::int16_t;
using SInt32 = std::int32_t;
using SInt64 = std::int64_t;
using UInt8 = std::uint8_t;
using UInt16 = std::uint16_t;
using UInt32 = std::uint32_t;
using UInt64 = std::uint64_t;

#define EXPORT_COREMODULE
#define BIND_MANAGED_TYPE_NAME(x, y) class y {}
#define PP_WRAP_CODE(code) do { code; } while(0)

template<class T, class Compare>
constexpr const T& clamp( const T& v, const T& lo, const T& hi, Compare comp )
{
    return comp(v, lo) ? lo : comp(hi, v) ? hi : v;
}

template<class T>
constexpr const T& clamp( const T& v, const T& lo, const T& hi )
{
    return ::clamp( v, lo, hi, std::less<T>() );
}

// Utility.h
template<class T>
inline T* Stride(T* p, size_t offset)
{
    return reinterpret_cast<T*>((char*)p + offset);
}

#if defined(_MSC_VER)
#define ALIGN_TYPE(ALIGN_)  __declspec(align(ALIGN_))
#elif defined(__clang__) || defined(__GNUC__)
#define ALIGN_TYPE(ALIGN_)  __attribute__((aligned(ALIGN_)))
#else
#error need ALIGN_TYPE() macro
#endif
