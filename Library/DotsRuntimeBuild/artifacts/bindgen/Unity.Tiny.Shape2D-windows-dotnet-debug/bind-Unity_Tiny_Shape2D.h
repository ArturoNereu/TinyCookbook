#pragma once
/*
 * AUTO-GENERATED, DO NOT EDIT BY HAND
 */
#include <cstdint>
#include "EntityTypes.h"
#include "artifacts/bindgen/Unity.Entities.CPlusPlus-windows-dotnet-debug/bind-Unity_Entities_CPlusPlus.h"
#include "artifacts/bindgen/Unity.Tiny.Core2D-windows-dotnet-debug/bind-Unity_Tiny_Core2D.h"
#include "artifacts/bindgen/Unity.Tiny.Core2DTypes-windows-dotnet-debug/bind-Unity_Tiny_Core2DTypes.h"
namespace Unity { namespace Tiny { namespace Core2D { 
struct Shape2DRenderer {
  Unity::Entities::Entity shape;
  Unity::Tiny::Core2D::Color color;
  Unity::Tiny::Core2D::BlendOp blending;
};
}}}

#if !defined(BUILD_UNITY_TINY_SHAPE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Shape2D_1_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Shape2D_1_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Shape2DRenderer>() {
    return priv_bind_Unity_Tiny_Shape2D_1_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Shape2DRenderer>()
{
    if (priv_bind_Unity_Tiny_Shape2D_1_cid == -1) {
        priv_bind_Unity_Tiny_Shape2D_1_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(15927831839435932382ull);
    }
    return priv_bind_Unity_Tiny_Shape2D_1_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Shape2DIndex : Unity::Entities::IBufferElementData {
  uint16_t index;
};
}}}

#if !defined(BUILD_UNITY_TINY_SHAPE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Shape2D_2_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Shape2D_2_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Shape2DIndex>() {
    return priv_bind_Unity_Tiny_Shape2D_2_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Shape2DIndex>()
{
    if (priv_bind_Unity_Tiny_Shape2D_2_cid == -1) {
        priv_bind_Unity_Tiny_Shape2D_2_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(13354017945871078568ull);
    }
    return priv_bind_Unity_Tiny_Shape2D_2_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Shape2DVertex : Unity::Entities::IBufferElementData {
  Unity::Mathematics::float2 position;
};
}}}

#if !defined(BUILD_UNITY_TINY_SHAPE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Shape2D_3_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Shape2D_3_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Shape2DVertex>() {
    return priv_bind_Unity_Tiny_Shape2D_3_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Shape2DVertex>()
{
    if (priv_bind_Unity_Tiny_Shape2D_3_cid == -1) {
        priv_bind_Unity_Tiny_Shape2D_3_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(2512440768288023345ull);
    }
    return priv_bind_Unity_Tiny_Shape2D_3_cid;
}


