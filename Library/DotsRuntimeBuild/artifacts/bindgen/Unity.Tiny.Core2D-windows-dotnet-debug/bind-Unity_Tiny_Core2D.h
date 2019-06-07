#pragma once
/*
 * AUTO-GENERATED, DO NOT EDIT BY HAND
 */
#include <cstdint>
#include "EntityTypes.h"
#include "artifacts/bindgen/Unity.Entities.CPlusPlus-windows-dotnet-debug/bind-Unity_Entities_CPlusPlus.h"
#include "artifacts/bindgen/Unity.Tiny.Core2DTypes-windows-dotnet-debug/bind-Unity_Tiny_Core2DTypes.h"
namespace Unity { namespace Tiny { namespace Core2D { 
enum class CameraClearFlags : int32_t {
  Nothing = 0,
  SolidColor = 1
};
}}}
namespace Unity { namespace Tiny { namespace Core2D { 
enum class CameraCullingMode : int32_t {
  NoCulling = 0,
  All = 1,
  Any = 2,
  None = 3
};
}}}
namespace Unity { namespace Tiny { namespace Core2D { 
enum class DisplayOrientation : int32_t {
  Horizontal = 0,
  Vertical = 1
};
}}}
namespace Unity { namespace Tiny { namespace Core2D { 
enum class RenderMode : int32_t {
  Auto = 0,
  Canvas = 1,
  WebGL = 2
};
}}}
namespace Unity { namespace Tiny { namespace Core2D { 
enum class DisplayListEntryType : int32_t {
  Unknown = 0,
  HitBoxOnly = 1,
  Sprite = 2,
  TiledSprite = 3,
  SlicedSprite = 4,
  Shape = 5,
  GroupOnly = 6,
  Text = 7
};
}}}
namespace Unity { namespace Tiny { namespace Core2D { 
enum class LoadResult : int32_t {
  stillWorking = 0,
  success = 1,
  failed = 2
};
}}}
namespace Unity { namespace Tiny { namespace Core2D { 
struct Rect {
  float x;
  float y;
  float width;
  float height;
};
}}}
namespace Unity { namespace Tiny { namespace Core2D { 
struct LayerSorting {
  int16_t layer;
  int16_t order;
  int32_t id;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_1_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_1_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::LayerSorting>() {
    return priv_bind_Unity_Tiny_Core2D_1_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::LayerSorting>()
{
    if (priv_bind_Unity_Tiny_Core2D_1_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_1_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(16791256614846730296ull);
    }
    return priv_bind_Unity_Tiny_Core2D_1_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct PrivateTransformData : Unity::Entities::ISystemStateComponentData {
  Unity::Entities::Entity inSortingGroup;
  uint32_t flags;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_2_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_2_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::PrivateTransformData>() {
    return priv_bind_Unity_Tiny_Core2D_2_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::PrivateTransformData>()
{
    if (priv_bind_Unity_Tiny_Core2D_2_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_2_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(2533882968160891188ull);
    }
    return priv_bind_Unity_Tiny_Core2D_2_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct PrivateTransformStatic : Unity::Entities::ISystemStateComponentData {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_3_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_3_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::PrivateTransformStatic>() {
    return priv_bind_Unity_Tiny_Core2D_3_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::PrivateTransformStatic>()
{
    if (priv_bind_Unity_Tiny_Core2D_3_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_3_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(8344042385225833178ull);
    }
    return priv_bind_Unity_Tiny_Core2D_3_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct TransformStatic {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_4_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_4_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::TransformStatic>() {
    return priv_bind_Unity_Tiny_Core2D_4_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::TransformStatic>()
{
    if (priv_bind_Unity_Tiny_Core2D_4_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_4_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(1392278223528803667ull);
    }
    return priv_bind_Unity_Tiny_Core2D_4_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct SortingGroup {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_5_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_5_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::SortingGroup>() {
    return priv_bind_Unity_Tiny_Core2D_5_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::SortingGroup>()
{
    if (priv_bind_Unity_Tiny_Core2D_5_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_5_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(16871309093634343422ull);
    }
    return priv_bind_Unity_Tiny_Core2D_5_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct LocalToWorld {
  Unity::Mathematics::float4x4 Value;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_6_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_6_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::LocalToWorld>() {
    return priv_bind_Unity_Tiny_Core2D_6_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::LocalToWorld>()
{
    if (priv_bind_Unity_Tiny_Core2D_6_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_6_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(17692315395642305262ull);
    }
    return priv_bind_Unity_Tiny_Core2D_6_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct LocalToParent {
  Unity::Mathematics::float4x4 Value;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_7_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_7_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::LocalToParent>() {
    return priv_bind_Unity_Tiny_Core2D_7_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::LocalToParent>()
{
    if (priv_bind_Unity_Tiny_Core2D_7_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_7_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(13650718982011419260ull);
    }
    return priv_bind_Unity_Tiny_Core2D_7_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Scale {
  float Value;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_8_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_8_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Scale>() {
    return priv_bind_Unity_Tiny_Core2D_8_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Scale>()
{
    if (priv_bind_Unity_Tiny_Core2D_8_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_8_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(17842374557893705896ull);
    }
    return priv_bind_Unity_Tiny_Core2D_8_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct NonUniformScale {
  Unity::Mathematics::float3 Value;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_9_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_9_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::NonUniformScale>() {
    return priv_bind_Unity_Tiny_Core2D_9_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::NonUniformScale>()
{
    if (priv_bind_Unity_Tiny_Core2D_9_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_9_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(17378348762221180850ull);
    }
    return priv_bind_Unity_Tiny_Core2D_9_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Rotation {
  Unity::Mathematics::quaternion Value;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_10_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_10_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Rotation>() {
    return priv_bind_Unity_Tiny_Core2D_10_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Rotation>()
{
    if (priv_bind_Unity_Tiny_Core2D_10_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_10_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(9104342357061045742ull);
    }
    return priv_bind_Unity_Tiny_Core2D_10_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Translation {
  Unity::Mathematics::float3 Value;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_11_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_11_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Translation>() {
    return priv_bind_Unity_Tiny_Core2D_11_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Translation>()
{
    if (priv_bind_Unity_Tiny_Core2D_11_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_11_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(266869500686333426ull);
    }
    return priv_bind_Unity_Tiny_Core2D_11_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Parent {
  Unity::Entities::Entity Value;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_12_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_12_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Parent>() {
    return priv_bind_Unity_Tiny_Core2D_12_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Parent>()
{
    if (priv_bind_Unity_Tiny_Core2D_12_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_12_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(793987765392955020ull);
    }
    return priv_bind_Unity_Tiny_Core2D_12_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct SortedEntity : Unity::Entities::IBufferElementData {
  uint64_t combinedKey;
  int32_t idx;
  Unity::Entities::Entity e;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_13_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_13_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::SortedEntity>() {
    return priv_bind_Unity_Tiny_Core2D_13_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::SortedEntity>()
{
    if (priv_bind_Unity_Tiny_Core2D_13_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_13_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(2786044047806322699ull);
    }
    return priv_bind_Unity_Tiny_Core2D_13_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct DisplayListCamera : Unity::Entities::ISystemStateComponentData {
  Unity::Mathematics::float4x4 world;
  Unity::Mathematics::float4x4 inverseWorld;
  Unity::Mathematics::float4 sortingDot;
  Unity::Mathematics::float2 clip2D;
  float clipZNear;
  float clipZFar;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_14_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_14_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::DisplayListCamera>() {
    return priv_bind_Unity_Tiny_Core2D_14_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::DisplayListCamera>()
{
    if (priv_bind_Unity_Tiny_Core2D_14_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_14_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(17982816350707444731ull);
    }
    return priv_bind_Unity_Tiny_Core2D_14_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct DisplayListEntry : Unity::Entities::IBufferElementData {
  Unity::Entities::Entity e;
  Unity::Mathematics::float4x4 finalMatrix;
  Unity::Tiny::Core2D::Rect inBounds;
  Unity::Tiny::Core2D::DisplayListEntryType type;
  Unity::Entities::Entity inSortingGroup;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_15_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_15_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::DisplayListEntry>() {
    return priv_bind_Unity_Tiny_Core2D_15_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::DisplayListEntry>()
{
    if (priv_bind_Unity_Tiny_Core2D_15_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_15_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(17681552670498504278ull);
    }
    return priv_bind_Unity_Tiny_Core2D_15_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct DisplayInfo {
  int32_t width;
  int32_t height;
  int32_t framebufferHeight;
  int32_t framebufferWidth;
  bool autoSizeToFrame;
  int32_t frameWidth;
  int32_t frameHeight;
  int32_t screenWidth;
  int32_t screenHeight;
  float screenDpiScale;
  Unity::Tiny::Core2D::DisplayOrientation orientation;
  Unity::Tiny::Core2D::RenderMode renderMode;
  bool focused;
  bool visible;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_16_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_16_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::DisplayInfo>() {
    return priv_bind_Unity_Tiny_Core2D_16_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::DisplayInfo>()
{
    if (priv_bind_Unity_Tiny_Core2D_16_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_16_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(13503913488219737475ull);
    }
    return priv_bind_Unity_Tiny_Core2D_16_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Camera2DClippingPlanes {
  float near;
  float far;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_17_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_17_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Camera2DClippingPlanes>() {
    return priv_bind_Unity_Tiny_Core2D_17_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Camera2DClippingPlanes>()
{
    if (priv_bind_Unity_Tiny_Core2D_17_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_17_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(10394232920009172372ull);
    }
    return priv_bind_Unity_Tiny_Core2D_17_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Camera2DAxisSort {
  Unity::Mathematics::float3 axis;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_18_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_18_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Camera2DAxisSort>() {
    return priv_bind_Unity_Tiny_Core2D_18_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Camera2DAxisSort>()
{
    if (priv_bind_Unity_Tiny_Core2D_18_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_18_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(8969857802765580131ull);
    }
    return priv_bind_Unity_Tiny_Core2D_18_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Camera2DRenderToTexture {
  int32_t width;
  int32_t height;
  bool freeze;
  Unity::Entities::Entity target;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_19_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_19_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Camera2DRenderToTexture>() {
    return priv_bind_Unity_Tiny_Core2D_19_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Camera2DRenderToTexture>()
{
    if (priv_bind_Unity_Tiny_Core2D_19_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_19_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(8102982298029797428ull);
    }
    return priv_bind_Unity_Tiny_Core2D_19_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer15 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_20_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_20_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer15>() {
    return priv_bind_Unity_Tiny_Core2D_20_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer15>()
{
    if (priv_bind_Unity_Tiny_Core2D_20_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_20_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(16559962388874391133ull);
    }
    return priv_bind_Unity_Tiny_Core2D_20_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer14 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_21_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_21_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer14>() {
    return priv_bind_Unity_Tiny_Core2D_21_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer14>()
{
    if (priv_bind_Unity_Tiny_Core2D_21_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_21_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(6155434906829704128ull);
    }
    return priv_bind_Unity_Tiny_Core2D_21_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer13 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_22_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_22_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer13>() {
    return priv_bind_Unity_Tiny_Core2D_22_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer13>()
{
    if (priv_bind_Unity_Tiny_Core2D_22_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_22_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(9456056978009148983ull);
    }
    return priv_bind_Unity_Tiny_Core2D_22_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer12 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_23_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_23_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer12>() {
    return priv_bind_Unity_Tiny_Core2D_23_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer12>()
{
    if (priv_bind_Unity_Tiny_Core2D_23_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_23_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(10094497374378025010ull);
    }
    return priv_bind_Unity_Tiny_Core2D_23_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer11 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_24_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_24_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer11>() {
    return priv_bind_Unity_Tiny_Core2D_24_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer11>()
{
    if (priv_bind_Unity_Tiny_Core2D_24_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_24_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(16702321084414558177ull);
    }
    return priv_bind_Unity_Tiny_Core2D_24_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer10 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_25_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_25_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer10>() {
    return priv_bind_Unity_Tiny_Core2D_25_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer10>()
{
    if (priv_bind_Unity_Tiny_Core2D_25_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_25_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(15552930797465551092ull);
    }
    return priv_bind_Unity_Tiny_Core2D_25_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer9 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_26_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_26_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer9>() {
    return priv_bind_Unity_Tiny_Core2D_26_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer9>()
{
    if (priv_bind_Unity_Tiny_Core2D_26_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_26_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(4274791357559240284ull);
    }
    return priv_bind_Unity_Tiny_Core2D_26_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer8 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_27_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_27_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer8>() {
    return priv_bind_Unity_Tiny_Core2D_27_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer8>()
{
    if (priv_bind_Unity_Tiny_Core2D_27_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_27_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(11203521914768601097ull);
    }
    return priv_bind_Unity_Tiny_Core2D_27_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer7 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_28_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_28_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer7>() {
    return priv_bind_Unity_Tiny_Core2D_28_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer7>()
{
    if (priv_bind_Unity_Tiny_Core2D_28_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_28_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(5616607532113878638ull);
    }
    return priv_bind_Unity_Tiny_Core2D_28_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer6 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_29_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_29_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer6>() {
    return priv_bind_Unity_Tiny_Core2D_29_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer6>()
{
    if (priv_bind_Unity_Tiny_Core2D_29_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_29_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(18410598181329887123ull);
    }
    return priv_bind_Unity_Tiny_Core2D_29_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer5 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_30_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_30_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer5>() {
    return priv_bind_Unity_Tiny_Core2D_30_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer5>()
{
    if (priv_bind_Unity_Tiny_Core2D_30_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_30_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(18439655668318098448ull);
    }
    return priv_bind_Unity_Tiny_Core2D_30_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer4 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_31_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_31_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer4>() {
    return priv_bind_Unity_Tiny_Core2D_31_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer4>()
{
    if (priv_bind_Unity_Tiny_Core2D_31_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_31_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(5287593981885136173ull);
    }
    return priv_bind_Unity_Tiny_Core2D_31_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer3 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_32_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_32_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer3>() {
    return priv_bind_Unity_Tiny_Core2D_32_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer3>()
{
    if (priv_bind_Unity_Tiny_Core2D_32_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_32_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(1157235472713801538ull);
    }
    return priv_bind_Unity_Tiny_Core2D_32_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer2 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_33_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_33_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer2>() {
    return priv_bind_Unity_Tiny_Core2D_33_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer2>()
{
    if (priv_bind_Unity_Tiny_Core2D_33_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_33_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(16270279090277337415ull);
    }
    return priv_bind_Unity_Tiny_Core2D_33_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer1 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_34_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_34_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer1>() {
    return priv_bind_Unity_Tiny_Core2D_34_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer1>()
{
    if (priv_bind_Unity_Tiny_Core2D_34_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_34_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(16308320121113893444ull);
    }
    return priv_bind_Unity_Tiny_Core2D_34_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct CameraLayer0 {
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_35_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_35_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::CameraLayer0>() {
    return priv_bind_Unity_Tiny_Core2D_35_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::CameraLayer0>()
{
    if (priv_bind_Unity_Tiny_Core2D_35_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_35_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(12359321090931291569ull);
    }
    return priv_bind_Unity_Tiny_Core2D_35_cid;
}

namespace Unity { namespace Tiny { namespace Core2D { 
struct Camera2D {
  float halfVerticalSize;
  Unity::Tiny::Core2D::Rect rect;
  Unity::Tiny::Core2D::Color backgroundColor;
  Unity::Tiny::Core2D::CameraClearFlags clearFlags;
  float depth;
  Unity::Tiny::Core2D::CameraCullingMode cullingMode;
  uint32_t cullingMask;
};
}}}

#if !defined(BUILD_UNITY_TINY_CORE2D_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_36_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Core2D_36_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Core2D::Camera2D>() {
    return priv_bind_Unity_Tiny_Core2D_36_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Core2D::Camera2D>()
{
    if (priv_bind_Unity_Tiny_Core2D_36_cid == -1) {
        priv_bind_Unity_Tiny_Core2D_36_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(742575471234645341ull);
    }
    return priv_bind_Unity_Tiny_Core2D_36_cid;
}


