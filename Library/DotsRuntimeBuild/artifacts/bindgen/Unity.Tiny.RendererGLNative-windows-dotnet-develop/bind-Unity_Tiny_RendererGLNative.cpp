/*
 * AUTO-GENERATED, DO NOT EDIT BY HAND
 */
#if !defined(BUILD_UNITY_TINY_RENDERERGLNATIVE_DLL)
#define BUILD_UNITY_TINY_RENDERERGLNATIVE_DLL 1
#endif
#include "bind-Unity_Tiny_RendererGLNative.h"

#if defined(__clang__)
#pragma clang diagnostic ignored "-Wreturn-type-c-linkage"
#elif defined(_MSC_VER)
#pragma warning(disable : 4190)
#endif

#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Rendering { 
static_assert(sizeof(TextureGL) == 8, "TextureGL size mismatch");
static_assert(offsetof(TextureGL, glTexId) == 0, "TextureGL.glTexId offset mismatch");
static_assert(offsetof(TextureGL, externalOwner) == 4, "TextureGL.externalOwner offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Rendering { 
static_assert(sizeof(TextureGL) == 8, "TextureGL size mismatch");
static_assert(offsetof(TextureGL, glTexId) == 0, "TextureGL.glTexId offset mismatch");
static_assert(offsetof(TextureGL, externalOwner) == 4, "TextureGL.externalOwner offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_RendererGLNative_1_cid = -1;


