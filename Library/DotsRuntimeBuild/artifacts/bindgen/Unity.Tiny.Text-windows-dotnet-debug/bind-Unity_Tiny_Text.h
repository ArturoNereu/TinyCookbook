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
namespace Unity { namespace Tiny { namespace Text { 
enum class FontName : int32_t {
  SansSerif = 0,
  Serif = 1,
  Monospace = 2
};
}}}
namespace Unity { namespace Tiny { namespace Text { 
struct CharacterInfo {
  uint32_t value;
  float advance;
  float bearingX;
  float bearingY;
  float width;
  float height;
  Unity::Tiny::Core2D::Rect characterRegion;
};
}}}
namespace Unity { namespace Tiny { namespace Text { 
struct GlyphPrivate {
  Unity::Tiny::Text::CharacterInfo ci;
  Unity::Mathematics::float2 position;
};
}}}
namespace Unity { namespace Tiny { namespace Text { 
struct Text2DPrivateCacheBitmap : Unity::Entities::ISystemStateComponentData {
  float size;
  Unity::Tiny::Core2D::Color color;
  float minSizeAutoFit;
  float maxSizeAutoFit;
  Unity::Mathematics::float2 rect;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_1_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_1_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::Text2DPrivateCacheBitmap>() {
    return priv_bind_Unity_Tiny_Text_1_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::Text2DPrivateCacheBitmap>()
{
    if (priv_bind_Unity_Tiny_Text_1_cid == -1) {
        priv_bind_Unity_Tiny_Text_1_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(16225031478773264919ull);
    }
    return priv_bind_Unity_Tiny_Text_1_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct Text2DPrivateBitmap : Unity::Entities::ISystemStateComponentData {
  Unity::Tiny::Core2D::Rect bounds;
  Unity::Mathematics::float2 fontScale;
  float size;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_2_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_2_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::Text2DPrivateBitmap>() {
    return priv_bind_Unity_Tiny_Text_2_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::Text2DPrivateBitmap>()
{
    if (priv_bind_Unity_Tiny_Text_2_cid == -1) {
        priv_bind_Unity_Tiny_Text_2_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(7316654996857395579ull);
    }
    return priv_bind_Unity_Tiny_Text_2_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct Text2DPrivateNative : Unity::Entities::ISystemStateComponentData {
  Unity::Tiny::Core2D::Rect bounds;
  float size;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_3_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_3_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::Text2DPrivateNative>() {
    return priv_bind_Unity_Tiny_Text_3_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::Text2DPrivateNative>()
{
    if (priv_bind_Unity_Tiny_Text_3_cid == -1) {
        priv_bind_Unity_Tiny_Text_3_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(11076862130264412536ull);
    }
    return priv_bind_Unity_Tiny_Text_3_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct TextPrivateFontName : Unity::Entities::IBufferElementData {
  uint16_t c;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_4_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_4_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::TextPrivateFontName>() {
    return priv_bind_Unity_Tiny_Text_4_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::TextPrivateFontName>()
{
    if (priv_bind_Unity_Tiny_Text_4_cid == -1) {
        priv_bind_Unity_Tiny_Text_4_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(16911631966872320750ull);
    }
    return priv_bind_Unity_Tiny_Text_4_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct TextPrivateString : Unity::Entities::IBufferElementData {
  uint16_t c;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_5_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_5_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::TextPrivateString>() {
    return priv_bind_Unity_Tiny_Text_5_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::TextPrivateString>()
{
    if (priv_bind_Unity_Tiny_Text_5_cid == -1) {
        priv_bind_Unity_Tiny_Text_5_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(6996355434796986259ull);
    }
    return priv_bind_Unity_Tiny_Text_5_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct Text2DAutoFit {
  float minSize;
  float maxSize;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_6_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_6_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::Text2DAutoFit>() {
    return priv_bind_Unity_Tiny_Text_6_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::Text2DAutoFit>()
{
    if (priv_bind_Unity_Tiny_Text_6_cid == -1) {
        priv_bind_Unity_Tiny_Text_6_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(10569324262974196863ull);
    }
    return priv_bind_Unity_Tiny_Text_6_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct Text2DRenderer {
  Unity::Entities::Entity style;
  Unity::Mathematics::float2 pivot;
  Unity::Tiny::Core2D::BlendOp blending;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_7_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_7_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::Text2DRenderer>() {
    return priv_bind_Unity_Tiny_Text_7_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::Text2DRenderer>()
{
    if (priv_bind_Unity_Tiny_Text_7_cid == -1) {
        priv_bind_Unity_Tiny_Text_7_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(5953061511517805254ull);
    }
    return priv_bind_Unity_Tiny_Text_7_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct TextString : Unity::Entities::IBufferElementData {
  uint16_t c;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_8_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_8_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::TextString>() {
    return priv_bind_Unity_Tiny_Text_8_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::TextString>()
{
    if (priv_bind_Unity_Tiny_Text_8_cid == -1) {
        priv_bind_Unity_Tiny_Text_8_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(1511947279668656474ull);
    }
    return priv_bind_Unity_Tiny_Text_8_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct Text2DStyle {
  Unity::Tiny::Core2D::Color color;
  float size;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_9_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_9_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::Text2DStyle>() {
    return priv_bind_Unity_Tiny_Text_9_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::Text2DStyle>()
{
    if (priv_bind_Unity_Tiny_Text_9_cid == -1) {
        priv_bind_Unity_Tiny_Text_9_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(17649336122499667438ull);
    }
    return priv_bind_Unity_Tiny_Text_9_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct Text2DStyleBitmapFont {
  Unity::Entities::Entity font;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_10_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_10_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::Text2DStyleBitmapFont>() {
    return priv_bind_Unity_Tiny_Text_10_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::Text2DStyleBitmapFont>()
{
    if (priv_bind_Unity_Tiny_Text_10_cid == -1) {
        priv_bind_Unity_Tiny_Text_10_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(15392216118221209652ull);
    }
    return priv_bind_Unity_Tiny_Text_10_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct Text2DStyleNativeFont {
  Unity::Entities::Entity font;
  bool italic;
  int32_t weight;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_11_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_11_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::Text2DStyleNativeFont>() {
    return priv_bind_Unity_Tiny_Text_11_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::Text2DStyleNativeFont>()
{
    if (priv_bind_Unity_Tiny_Text_11_cid == -1) {
        priv_bind_Unity_Tiny_Text_11_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(14053006102498698157ull);
    }
    return priv_bind_Unity_Tiny_Text_11_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct NativeFont {
  Unity::Tiny::Text::FontName name;
  float worldUnitsToPt;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_12_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_12_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::NativeFont>() {
    return priv_bind_Unity_Tiny_Text_12_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::NativeFont>()
{
    if (priv_bind_Unity_Tiny_Text_12_cid == -1) {
        priv_bind_Unity_Tiny_Text_12_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(7594031312023238757ull);
    }
    return priv_bind_Unity_Tiny_Text_12_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct BitmapFont {
  Unity::Entities::Entity textureAtlas;
  float size;
  float ascent;
  float descent;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_13_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_13_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::BitmapFont>() {
    return priv_bind_Unity_Tiny_Text_13_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::BitmapFont>()
{
    if (priv_bind_Unity_Tiny_Text_13_cid == -1) {
        priv_bind_Unity_Tiny_Text_13_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(10866330436665620479ull);
    }
    return priv_bind_Unity_Tiny_Text_13_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct CharacterInfoBuffer : Unity::Entities::IBufferElementData {
  Unity::Tiny::Text::CharacterInfo data;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_14_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_14_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::CharacterInfoBuffer>() {
    return priv_bind_Unity_Tiny_Text_14_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::CharacterInfoBuffer>()
{
    if (priv_bind_Unity_Tiny_Text_14_cid == -1) {
        priv_bind_Unity_Tiny_Text_14_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(15402236120158511343ull);
    }
    return priv_bind_Unity_Tiny_Text_14_cid;
}

namespace Unity { namespace Tiny { namespace Text { 
struct GlyphPrivateBuffer : Unity::Entities::IBufferElementData {
  Unity::Tiny::Text::GlyphPrivate c;
};
}}}

#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
    extern DLLIMPORT ComponentTypeId priv_bind_Unity_Tiny_Text_15_cid;
#else
    extern DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_15_cid;
#endif

template<> inline ComponentTypeId ComponentId<::Unity::Tiny::Text::GlyphPrivateBuffer>() {
    return priv_bind_Unity_Tiny_Text_15_cid;
}

template<> inline ComponentTypeId InitComponentId<::Unity::Tiny::Text::GlyphPrivateBuffer>()
{
    if (priv_bind_Unity_Tiny_Text_15_cid == -1) {
        priv_bind_Unity_Tiny_Text_15_cid = Unity::Entities::TypeManager::TypeIndexForStableTypeHash(9172232493986285135ull);
    }
    return priv_bind_Unity_Tiny_Text_15_cid;
}


