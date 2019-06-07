#pragma once
/*
 * AUTO-GENERATED, DO NOT EDIT BY HAND
 */
#include <cstdint>
#include "EntityTypes.h"
#include "artifacts/bindgen/Unity.Entities.CPlusPlus-windows-dotnet-debug/bind-Unity_Entities_CPlusPlus.h"
namespace Unity { namespace Tiny { namespace Core2D { 
enum class ImageStatus : int32_t {
  Invalid = 0,
  Loaded = 1,
  Loading = 2,
  LoadError = 3
};
}}}
namespace Unity { namespace Tiny { namespace Core2D { 
enum class Image2DSaveStatus : int32_t {
  Invalid = 0,
  Written = 1,
  Writing = 2,
  WriteErrorBadInput = 3,
  WriteErrorUnsuportedFormat = 4,
  WriteError = 5
};
}}}
namespace Unity { namespace Tiny { namespace Core2D { 
enum class Image2DMemoryFormat : int32_t {
  RGBA8Premultiplied = 0,
  RGBA8 = 1,
  A8 = 2
};
}}}
namespace Unity { namespace Tiny { namespace Core2D { 
struct Image2DAlphaMaskData : Unity::Entities::IBufferElementData {
  uint8_t c;
};
}}}

#if !defined(BUILD_UNITY_TINY_IMAGE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_1_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_1_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Image2DAlphaMaskData>() {
    return priv_bind_Unity_Tiny_Image2D_1_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Image2DAlphaMaskData>()
{
    if (priv_bind_Unity_Tiny_Image2D_1_cid == -1) {
        priv_bind_Unity_Tiny_Image2D_1_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(2249307904479293209ull);
    }
    return priv_bind_Unity_Tiny_Image2D_1_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Image2DAlphaMask {
  float threshold;
};
}}}

#if !defined(BUILD_UNITY_TINY_IMAGE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_2_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_2_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Image2DAlphaMask>() {
    return priv_bind_Unity_Tiny_Image2D_2_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Image2DAlphaMask>()
{
    if (priv_bind_Unity_Tiny_Image2D_2_cid == -1) {
        priv_bind_Unity_Tiny_Image2D_2_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(7352807831305408065ull);
    }
    return priv_bind_Unity_Tiny_Image2D_2_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Image2D {
  bool disableSmoothing;
  Unity::Mathematics::float2 imagePixelSize;
  bool hasAlpha;
  Unity::Tiny::Core2D::ImageStatus status;
};
}}}

#if !defined(BUILD_UNITY_TINY_IMAGE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_3_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_3_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Image2D>() {
    return priv_bind_Unity_Tiny_Image2D_3_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Image2D>()
{
    if (priv_bind_Unity_Tiny_Image2D_3_cid == -1) {
        priv_bind_Unity_Tiny_Image2D_3_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(14707027362875060486ull);
    }
    return priv_bind_Unity_Tiny_Image2D_3_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Image2DRenderToTexture {
};
}}}

#if !defined(BUILD_UNITY_TINY_IMAGE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_4_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_4_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Image2DRenderToTexture>() {
    return priv_bind_Unity_Tiny_Image2D_4_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Image2DRenderToTexture>()
{
    if (priv_bind_Unity_Tiny_Image2D_4_cid == -1) {
        priv_bind_Unity_Tiny_Image2D_4_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(14789900920384307588ull);
    }
    return priv_bind_Unity_Tiny_Image2D_4_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Image2DLoadFromFileMaskFile : Unity::Entities::IBufferElementData {
  uint16_t s;
};
}}}

#if !defined(BUILD_UNITY_TINY_IMAGE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_5_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_5_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Image2DLoadFromFileMaskFile>() {
    return priv_bind_Unity_Tiny_Image2D_5_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Image2DLoadFromFileMaskFile>()
{
    if (priv_bind_Unity_Tiny_Image2D_5_cid == -1) {
        priv_bind_Unity_Tiny_Image2D_5_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(12327861584018337679ull);
    }
    return priv_bind_Unity_Tiny_Image2D_5_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Image2DLoadFromFileImageFile : Unity::Entities::IBufferElementData {
  uint16_t s;
};
}}}

#if !defined(BUILD_UNITY_TINY_IMAGE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_6_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_6_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Image2DLoadFromFileImageFile>() {
    return priv_bind_Unity_Tiny_Image2D_6_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Image2DLoadFromFileImageFile>()
{
    if (priv_bind_Unity_Tiny_Image2D_6_cid == -1) {
        priv_bind_Unity_Tiny_Image2D_6_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(18416684045041807316ull);
    }
    return priv_bind_Unity_Tiny_Image2D_6_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Image2DLoadFromFile {
  int32_t dummy;
};
}}}

#if !defined(BUILD_UNITY_TINY_IMAGE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_7_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Image2D_7_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Image2DLoadFromFile>() {
    return priv_bind_Unity_Tiny_Image2D_7_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Image2DLoadFromFile>()
{
    if (priv_bind_Unity_Tiny_Image2D_7_cid == -1) {
        priv_bind_Unity_Tiny_Image2D_7_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(9220594566207582347ull);
    }
    return priv_bind_Unity_Tiny_Image2D_7_cid;
}


