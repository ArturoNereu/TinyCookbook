#pragma once
/*
 * AUTO-GENERATED, DO NOT EDIT BY HAND
 */
#include <cstdint>
#include "EntityTypes.h"
#include "artifacts/bindgen/Unity.Entities.CPlusPlus-windows-dotnet-debug/bind-Unity_Entities_CPlusPlus.h"
namespace Unity { namespace Tiny { namespace Core2D { 
enum class BlendOp : int32_t {
  Alpha = 0,
  Add = 1,
  Multiply = 2,
  MultiplyAlpha = 3,
  Disabled = 4
};
}}}
namespace Unity { namespace Tiny { namespace Core2D { 
enum class LoopMode : int32_t {
  Loop = 0,
  Once = 1,
  PingPong = 2,
  PingPongOnce = 3,
  ClampForever = 4
};
}}}
namespace Unity { namespace Tiny { namespace Core2D { 
struct RectTransformFinalSize : Unity::Entities::ISystemStateComponentData {
  Unity::Mathematics::float2 size;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2DTYPES_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2DTypes_1_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2DTypes_1_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::RectTransformFinalSize>() {
    return priv_bind_Unity_Tiny_Core2DTypes_1_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::RectTransformFinalSize>()
{
    if (priv_bind_Unity_Tiny_Core2DTypes_1_cid == -1) {
        priv_bind_Unity_Tiny_Core2DTypes_1_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(9511079293109873250ull);
    }
    return priv_bind_Unity_Tiny_Core2DTypes_1_cid;
}


