#pragma once
/*
 * AUTO-GENERATED, DO NOT EDIT BY HAND
 */
#include <cstdint>
#include "EntityTypes.h"
#include "artifacts/bindgen/Unity.Entities.CPlusPlus-windows-dotnet-debug/bind-Unity_Entities_CPlusPlus.h"
#include "artifacts/bindgen/Unity.Tiny.Core2D-windows-dotnet-debug/bind-Unity_Tiny_Core2D.h"
#include "artifacts/bindgen/Unity.Tiny.Image2D-windows-dotnet-debug/bind-Unity_Tiny_Image2D.h"
namespace Unity { namespace Tiny { namespace STB { 
struct Image2DSTBLoading : Unity::Entities::ISystemStateComponentData {
  int64_t internalId;
};
}}}

#if !defined(BUILD_UNITY_TINY_IMAGE2DIOSTB_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Image2DIOSTB_1_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Image2DIOSTB_1_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::STB::Image2DSTBLoading>() {
    return priv_bind_Unity_Tiny_Image2DIOSTB_1_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::STB::Image2DSTBLoading>()
{
    if (priv_bind_Unity_Tiny_Image2DIOSTB_1_cid == -1) {
        priv_bind_Unity_Tiny_Image2DIOSTB_1_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(18415357923689791578ull);
    }
    return priv_bind_Unity_Tiny_Image2DIOSTB_1_cid;
}

namespace Unity { namespace Tiny { namespace STB { 
struct Image2DSTB : Unity::Entities::ISystemStateComponentData {
  int32_t imageHandle;
};
}}}

#if !defined(BUILD_UNITY_TINY_IMAGE2DIOSTB_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Image2DIOSTB_2_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Image2DIOSTB_2_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::STB::Image2DSTB>() {
    return priv_bind_Unity_Tiny_Image2DIOSTB_2_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::STB::Image2DSTB>()
{
    if (priv_bind_Unity_Tiny_Image2DIOSTB_2_cid == -1) {
        priv_bind_Unity_Tiny_Image2DIOSTB_2_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(5123166490985975547ull);
    }
    return priv_bind_Unity_Tiny_Image2DIOSTB_2_cid;
}


