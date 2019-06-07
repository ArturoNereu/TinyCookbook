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
namespace Unity { namespace Tiny { namespace Core2D { 
enum class DrawMode : int32_t {
  ContinuousTiling = 0,
  AdaptiveTiling = 1,
  Stretch = 2
};
}}}
namespace Unity { namespace Tiny { namespace Core2D { 
struct Sprite2DSequencePlayer {
  Unity::Entities::Entity sequence;
  float speed;
  float time;
  bool paused;
  Unity::Tiny::Core2D::LoopMode loop;
};
}}}

#if !defined(BUILD_UNITY_TINY_SPRITE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_1_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_1_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Sprite2DSequencePlayer>() {
    return priv_bind_Unity_Tiny_Sprite2D_1_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Sprite2DSequencePlayer>()
{
    if (priv_bind_Unity_Tiny_Sprite2D_1_cid == -1) {
        priv_bind_Unity_Tiny_Sprite2D_1_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(13688764092462950983ull);
    }
    return priv_bind_Unity_Tiny_Sprite2D_1_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Sprite2DSequence : Unity::Entities::IBufferElementData {
  Unity::Entities::Entity e;
};
}}}

#if !defined(BUILD_UNITY_TINY_SPRITE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_2_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_2_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Sprite2DSequence>() {
    return priv_bind_Unity_Tiny_Sprite2D_2_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Sprite2DSequence>()
{
    if (priv_bind_Unity_Tiny_Sprite2D_2_cid == -1) {
        priv_bind_Unity_Tiny_Sprite2D_2_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(16033114912968864438ull);
    }
    return priv_bind_Unity_Tiny_Sprite2D_2_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Sprite2DSequenceOptions {
  float frameRate;
};
}}}

#if !defined(BUILD_UNITY_TINY_SPRITE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_3_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_3_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Sprite2DSequenceOptions>() {
    return priv_bind_Unity_Tiny_Sprite2D_3_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Sprite2DSequenceOptions>()
{
    if (priv_bind_Unity_Tiny_Sprite2D_3_cid == -1) {
        priv_bind_Unity_Tiny_Sprite2D_3_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(3043739201827621478ull);
    }
    return priv_bind_Unity_Tiny_Sprite2D_3_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct SpriteAtlas : Unity::Entities::IBufferElementData {
  Unity::Entities::Entity sprite;
};
}}}

#if !defined(BUILD_UNITY_TINY_SPRITE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_4_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_4_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::SpriteAtlas>() {
    return priv_bind_Unity_Tiny_Sprite2D_4_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::SpriteAtlas>()
{
    if (priv_bind_Unity_Tiny_Sprite2D_4_cid == -1) {
        priv_bind_Unity_Tiny_Sprite2D_4_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(2808581199008031712ull);
    }
    return priv_bind_Unity_Tiny_Sprite2D_4_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Sprite2DBorder {
  Unity::Mathematics::float2 bottomLeft;
  Unity::Mathematics::float2 topRight;
};
}}}

#if !defined(BUILD_UNITY_TINY_SPRITE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_5_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_5_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Sprite2DBorder>() {
    return priv_bind_Unity_Tiny_Sprite2D_5_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Sprite2DBorder>()
{
    if (priv_bind_Unity_Tiny_Sprite2D_5_cid == -1) {
        priv_bind_Unity_Tiny_Sprite2D_5_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(10296512627781736149ull);
    }
    return priv_bind_Unity_Tiny_Sprite2D_5_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Sprite2DRendererOptions {
  Unity::Mathematics::float2 size;
  Unity::Tiny::Core2D::DrawMode drawMode;
};
}}}

#if !defined(BUILD_UNITY_TINY_SPRITE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_6_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_6_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Sprite2DRendererOptions>() {
    return priv_bind_Unity_Tiny_Sprite2D_6_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Sprite2DRendererOptions>()
{
    if (priv_bind_Unity_Tiny_Sprite2D_6_cid == -1) {
        priv_bind_Unity_Tiny_Sprite2D_6_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(147127668789581756ull);
    }
    return priv_bind_Unity_Tiny_Sprite2D_6_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Sprite2DRenderer {
  Unity::Entities::Entity sprite;
  Unity::Tiny::Core2D::Color color;
  Unity::Tiny::Core2D::BlendOp blending;
};
}}}

#if !defined(BUILD_UNITY_TINY_SPRITE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_7_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_7_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Sprite2DRenderer>() {
    return priv_bind_Unity_Tiny_Sprite2D_7_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Sprite2DRenderer>()
{
    if (priv_bind_Unity_Tiny_Sprite2D_7_cid == -1) {
        priv_bind_Unity_Tiny_Sprite2D_7_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(12932679046359085526ull);
    }
    return priv_bind_Unity_Tiny_Sprite2D_7_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Sprite2DPrivate : Unity::Entities::ISystemStateComponentData {
  bool valid;
  Unity::Tiny::Core2D::Rect rect;
};
}}}

#if !defined(BUILD_UNITY_TINY_SPRITE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_8_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_8_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Sprite2DPrivate>() {
    return priv_bind_Unity_Tiny_Sprite2D_8_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Sprite2DPrivate>()
{
    if (priv_bind_Unity_Tiny_Sprite2D_8_cid == -1) {
        priv_bind_Unity_Tiny_Sprite2D_8_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(9161414504581158721ull);
    }
    return priv_bind_Unity_Tiny_Sprite2D_8_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Sprite2D {
  Unity::Entities::Entity image;
  Unity::Tiny::Core2D::Rect imageRegion;
  Unity::Mathematics::float2 pivot;
  float pixelsToWorldUnits;
};
}}}

#if !defined(BUILD_UNITY_TINY_SPRITE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_9_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Sprite2D_9_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Sprite2D>() {
    return priv_bind_Unity_Tiny_Sprite2D_9_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Sprite2D>()
{
    if (priv_bind_Unity_Tiny_Sprite2D_9_cid == -1) {
        priv_bind_Unity_Tiny_Sprite2D_9_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(14532146675218703721ull);
    }
    return priv_bind_Unity_Tiny_Sprite2D_9_cid;
}


