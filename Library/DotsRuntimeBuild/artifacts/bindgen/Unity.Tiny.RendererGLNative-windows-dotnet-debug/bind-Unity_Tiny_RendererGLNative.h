#pragma once
/*
 * AUTO-GENERATED, DO NOT EDIT BY HAND
 */
#include <cstdint>
#include "EntityTypes.h"
#include "artifacts/bindgen/Unity.Entities.CPlusPlus-windows-dotnet-debug/bind-Unity_Entities_CPlusPlus.h"
#include "artifacts/bindgen/Unity.Tiny.Core2D-windows-dotnet-debug/bind-Unity_Tiny_Core2D.h"
#include "artifacts/bindgen/Unity.Tiny.Core2DTypes-windows-dotnet-debug/bind-Unity_Tiny_Core2DTypes.h"
#include "artifacts/bindgen/Unity.Tiny.Image2D-windows-dotnet-debug/bind-Unity_Tiny_Image2D.h"
#include "artifacts/bindgen/Unity.Tiny.Sprite2D-windows-dotnet-debug/bind-Unity_Tiny_Sprite2D.h"
#include "artifacts/bindgen/Unity.Tiny.Shape2D-windows-dotnet-debug/bind-Unity_Tiny_Shape2D.h"
#include "artifacts/bindgen/Unity.Tiny.Image2DIOSTB-windows-dotnet-debug/bind-Unity_Tiny_Image2DIOSTB.h"
#include "artifacts/bindgen/Unity.Tiny.GLFW-windows-dotnet-debug/bind-Unity_Tiny_GLFW.h"
#include "artifacts/bindgen/Unity.Tiny.Text-windows-dotnet-debug/bind-Unity_Tiny_Text.h"
namespace Unity { namespace Tiny { namespace Rendering { 
struct TextureGL : Unity::Entities::ISystemStateComponentData {
  uint32_t glTexId;
  bool externalOwner;
};
}}}

#if !defined(BUILD_UNITY_TINY_RENDERERGLNATIVE_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_RendererGLNative_1_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_RendererGLNative_1_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Rendering::TextureGL>() {
    return priv_bind_Unity_Tiny_RendererGLNative_1_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Rendering::TextureGL>()
{
    if (priv_bind_Unity_Tiny_RendererGLNative_1_cid == -1) {
        priv_bind_Unity_Tiny_RendererGLNative_1_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(11372737788017458978ull);
    }
    return priv_bind_Unity_Tiny_RendererGLNative_1_cid;
}


