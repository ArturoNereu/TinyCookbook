#pragma once

#include "../Annotations.h"

#ifndef PP_CAT
#define _PP_CAT(a, b)    a##b
#define PP_CAT(a, b)     _PP_CAT(a, b)
#endif

// #define META_TRACE // this marco allow us to debug meta template code, unity editor is compiled with inlining enabled for debug build
#if defined(META_TRACE) || (PLATFORM_N3DS && defined(_DEBUG))
#define MATH_FORCEINLINE		UNITY_INLINE
#else
#define MATH_FORCEINLINE		UNITY_FORCEINLINE
#endif

#define MATH_NOINLINE			UNITY_NOINLINE
#define MATH_INLINE				UNITY_INLINE
#define MATH_EMPTYINLINE		UNITY_EMPTYINLINE


#if defined(_MSC_VER)

	#if !defined(_DEBUG)
	#define META_PEEPHOLE
	#endif

	#define explicit_typename           typename
	#define template_decl(name, type)   template<type> name
	#define template_spec(name, val)    template<> name<val>
	#define template_inst(name, val)    name<val>
	#define explicit_operator			operator

	#define vec_attr            const
	#define rhs_attr
	#define lhs_attr
	#define scalar_attr     mutable

#else

	#if defined(__OPTIMIZE__)
	#define META_PEEPHOLE
	#endif

	#define explicit_typename           typename
	#define template_decl(name, type)   template<type, int foo> name
	#define template_spec(name, val)    template<int foo> name<val, foo>
	#define template_inst(name, val)    name<val, 0>

    #if 1 //HAS_CLANG_FEATURE(cxx_explicit_conversions)
		#define explicit_operator			explicit operator
	#else
		#define explicit_operator			operator
	#endif

	#if (defined  __has_extension)
		#define SUPPORT_VECTOR_EXTENSION __has_extension(attribute_ext_vector_type)
	#else
		#define SUPPORT_VECTOR_EXTENSION 0
	#endif

	#if defined(MATH_HAS_NATIVE_SIMD) && MATH_HAS_NATIVE_SIMD==0
		#undef MATH_HAS_NATIVE_SIMD
	#elif defined(__clang__) && SUPPORT_VECTOR_EXTENSION && (defined(__SSE__) || defined(__ARM_NEON__)) && !defined(__EMSCRIPTEN__)
		#define MATH_HAS_NATIVE_SIMD
	#endif

	#define vec_attr            const
	#define rhs_attr
	#define lhs_attr
	#define scalar_attr     mutable

#endif

