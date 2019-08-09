#pragma once
/*
 * AUTO-GENERATED, DO NOT EDIT BY HAND
 */
#include <cstdint>
#include "EntityTypes.h"
#include "artifacts/bindgen/Unity.Entities.CPlusPlus-windows-dotnet-develop/bind-Unity_Entities_CPlusPlus.h"
#include "artifacts/bindgen/Unity.Tiny.Core2D-windows-dotnet-develop/bind-Unity_Tiny_Core2D.h"
#include "artifacts/bindgen/Unity.Tiny.Core2DTypes-windows-dotnet-develop/bind-Unity_Tiny_Core2DTypes.h"
#include "artifacts/bindgen/Unity.Tiny.Image2D-windows-dotnet-develop/bind-Unity_Tiny_Image2D.h"
#include "artifacts/bindgen/Unity.Tiny.Image2DIOSTB-windows-dotnet-develop/bind-Unity_Tiny_Image2DIOSTB.h"
#include "artifacts/bindgen/Unity.Tiny.Text-windows-dotnet-develop/bind-Unity_Tiny_Text.h"
namespace Unity { namespace Tiny { namespace TextNative { 
struct Text2DPrivateCacheNative : Unity::Entities::ISystemStateComponentData {
  float size;
  Unity::Tiny::Core2D::Color color;
  float minSizeAutoFit;
  float maxSizeAutoFit;
  Unity::Mathematics::float2 rect;
  int32_t glTexId;
  bool dirty;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXTNATIVE_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_TextNative_1_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_TextNative_1_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::TextNative::Text2DPrivateCacheNative>() {
    return priv_bind_Unity_Tiny_TextNative_1_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::TextNative::Text2DPrivateCacheNative>()
{
    if (priv_bind_Unity_Tiny_TextNative_1_cid == -1) {
        priv_bind_Unity_Tiny_TextNative_1_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(10042253112628239794ull);
    }
    return priv_bind_Unity_Tiny_TextNative_1_cid;
}


