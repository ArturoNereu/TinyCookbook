#pragma once
/*
 * AUTO-GENERATED, DO NOT EDIT BY HAND
 */
#include <cstdint>
#include "EntityTypes.h"
#include "artifacts/bindgen/Unity.Entities.CPlusPlus-windows-dotnet-debug/bind-Unity_Entities_CPlusPlus.h"
#include "artifacts/bindgen/Unity.Tiny.Core2D-windows-dotnet-debug/bind-Unity_Tiny_Core2D.h"
#include "artifacts/bindgen/Unity.Tiny.Core2DTypes-windows-dotnet-debug/bind-Unity_Tiny_Core2DTypes.h"
#include "artifacts/bindgen/Unity.Tiny.Sprite2D-windows-dotnet-debug/bind-Unity_Tiny_Sprite2D.h"
#include "artifacts/bindgen/Unity.Tiny.Image2D-windows-dotnet-debug/bind-Unity_Tiny_Image2D.h"
#include "artifacts/bindgen/Unity.Tiny.Shape2D-windows-dotnet-debug/bind-Unity_Tiny_Shape2D.h"
namespace Unity { namespace Tiny { namespace HitBox2D { 
struct RayCastResult {
  Unity::Entities::Entity entityHit;
  float t;
};
}}}

#if !defined(BUILD_UNITY_TINY_HITBOX2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_HitBox2D_1_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_HitBox2D_1_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::HitBox2D::RayCastResult>() {
    return priv_bind_Unity_Tiny_HitBox2D_1_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::HitBox2D::RayCastResult>()
{
    if (priv_bind_Unity_Tiny_HitBox2D_1_cid == -1) {
        priv_bind_Unity_Tiny_HitBox2D_1_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(4504959379401366426ull);
    }
    return priv_bind_Unity_Tiny_HitBox2D_1_cid;
}

namespace Unity { namespace Tiny { namespace HitBox2D { 
struct HitTestResult {
  Unity::Entities::Entity entityHit;
  Unity::Mathematics::float2 uv;
};
}}}

#if !defined(BUILD_UNITY_TINY_HITBOX2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_HitBox2D_2_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_HitBox2D_2_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::HitBox2D::HitTestResult>() {
    return priv_bind_Unity_Tiny_HitBox2D_2_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::HitBox2D::HitTestResult>()
{
    if (priv_bind_Unity_Tiny_HitBox2D_2_cid == -1) {
        priv_bind_Unity_Tiny_HitBox2D_2_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(8088118200044856102ull);
    }
    return priv_bind_Unity_Tiny_HitBox2D_2_cid;
}

namespace Unity { namespace Tiny { namespace HitBox2D { 
struct HitBoxOverlap : Unity::Entities::IBufferElementData {
  Unity::Entities::Entity otherEntity;
  Unity::Entities::Entity camera;
};
}}}

#if !defined(BUILD_UNITY_TINY_HITBOX2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_HitBox2D_3_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_HitBox2D_3_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::HitBox2D::HitBoxOverlap>() {
    return priv_bind_Unity_Tiny_HitBox2D_3_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::HitBox2D::HitBoxOverlap>()
{
    if (priv_bind_Unity_Tiny_HitBox2D_3_cid == -1) {
        priv_bind_Unity_Tiny_HitBox2D_3_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(11313670787958164107ull);
    }
    return priv_bind_Unity_Tiny_HitBox2D_3_cid;
}

namespace Unity { namespace Tiny { namespace HitBox2D { 
struct Sprite2DRendererHitBox2D {
  bool pixelAccurate;
};
}}}

#if !defined(BUILD_UNITY_TINY_HITBOX2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_HitBox2D_4_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_HitBox2D_4_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::HitBox2D::Sprite2DRendererHitBox2D>() {
    return priv_bind_Unity_Tiny_HitBox2D_4_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::HitBox2D::Sprite2DRendererHitBox2D>()
{
    if (priv_bind_Unity_Tiny_HitBox2D_4_cid == -1) {
        priv_bind_Unity_Tiny_HitBox2D_4_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(8964035444447794530ull);
    }
    return priv_bind_Unity_Tiny_HitBox2D_4_cid;
}

namespace Unity { namespace Tiny { namespace HitBox2D { 
struct RectHitBox2D {
  Unity::Tiny::Core2D::Rect box;
};
}}}

#if !defined(BUILD_UNITY_TINY_HITBOX2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_HitBox2D_5_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_HitBox2D_5_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::HitBox2D::RectHitBox2D>() {
    return priv_bind_Unity_Tiny_HitBox2D_5_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::HitBox2D::RectHitBox2D>()
{
    if (priv_bind_Unity_Tiny_HitBox2D_5_cid == -1) {
        priv_bind_Unity_Tiny_HitBox2D_5_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(17694721706025218029ull);
    }
    return priv_bind_Unity_Tiny_HitBox2D_5_cid;
}


