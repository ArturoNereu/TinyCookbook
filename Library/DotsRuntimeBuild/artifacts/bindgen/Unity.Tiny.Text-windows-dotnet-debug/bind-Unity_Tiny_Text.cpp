/*
 * AUTO-GENERATED, DO NOT EDIT BY HAND
 */
#if !defined(BUILD_UNITY_TINY_TEXT_DLL)
#define BUILD_UNITY_TINY_TEXT_DLL 1
#endif
#include "bind-Unity_Tiny_Text.h"

#if defined(__clang__)
#pragma clang diagnostic ignored "-Wreturn-type-c-linkage"
#elif defined(_MSC_VER)
#pragma warning(disable : 4190)
#endif

#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(CharacterInfo) == 40, "CharacterInfo size mismatch");
static_assert(offsetof(CharacterInfo, value) == 0, "CharacterInfo.value offset mismatch");
static_assert(offsetof(CharacterInfo, advance) == 4, "CharacterInfo.advance offset mismatch");
static_assert(offsetof(CharacterInfo, bearingX) == 8, "CharacterInfo.bearingX offset mismatch");
static_assert(offsetof(CharacterInfo, bearingY) == 12, "CharacterInfo.bearingY offset mismatch");
static_assert(offsetof(CharacterInfo, width) == 16, "CharacterInfo.width offset mismatch");
static_assert(offsetof(CharacterInfo, height) == 20, "CharacterInfo.height offset mismatch");
static_assert(offsetof(CharacterInfo, characterRegion) == 24, "CharacterInfo.characterRegion offset mismatch");
}}}
#endif
#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(CharacterInfo) == 40, "CharacterInfo size mismatch");
static_assert(offsetof(CharacterInfo, value) == 0, "CharacterInfo.value offset mismatch");
static_assert(offsetof(CharacterInfo, advance) == 4, "CharacterInfo.advance offset mismatch");
static_assert(offsetof(CharacterInfo, bearingX) == 8, "CharacterInfo.bearingX offset mismatch");
static_assert(offsetof(CharacterInfo, bearingY) == 12, "CharacterInfo.bearingY offset mismatch");
static_assert(offsetof(CharacterInfo, width) == 16, "CharacterInfo.width offset mismatch");
static_assert(offsetof(CharacterInfo, height) == 20, "CharacterInfo.height offset mismatch");
static_assert(offsetof(CharacterInfo, characterRegion) == 24, "CharacterInfo.characterRegion offset mismatch");
}}}
#endif

#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(GlyphPrivate) == 48, "GlyphPrivate size mismatch");
static_assert(offsetof(GlyphPrivate, ci) == 0, "GlyphPrivate.ci offset mismatch");
static_assert(offsetof(GlyphPrivate, position) == 40, "GlyphPrivate.position offset mismatch");
}}}
#endif
#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(GlyphPrivate) == 48, "GlyphPrivate size mismatch");
static_assert(offsetof(GlyphPrivate, ci) == 0, "GlyphPrivate.ci offset mismatch");
static_assert(offsetof(GlyphPrivate, position) == 40, "GlyphPrivate.position offset mismatch");
}}}
#endif

#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DPrivateCacheBitmap) == 36, "Text2DPrivateCacheBitmap size mismatch");
static_assert(offsetof(Text2DPrivateCacheBitmap, size) == 0, "Text2DPrivateCacheBitmap.size offset mismatch");
static_assert(offsetof(Text2DPrivateCacheBitmap, color) == 4, "Text2DPrivateCacheBitmap.color offset mismatch");
static_assert(offsetof(Text2DPrivateCacheBitmap, minSizeAutoFit) == 20, "Text2DPrivateCacheBitmap.minSizeAutoFit offset mismatch");
static_assert(offsetof(Text2DPrivateCacheBitmap, maxSizeAutoFit) == 24, "Text2DPrivateCacheBitmap.maxSizeAutoFit offset mismatch");
static_assert(offsetof(Text2DPrivateCacheBitmap, rect) == 28, "Text2DPrivateCacheBitmap.rect offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DPrivateCacheBitmap) == 36, "Text2DPrivateCacheBitmap size mismatch");
static_assert(offsetof(Text2DPrivateCacheBitmap, size) == 0, "Text2DPrivateCacheBitmap.size offset mismatch");
static_assert(offsetof(Text2DPrivateCacheBitmap, color) == 4, "Text2DPrivateCacheBitmap.color offset mismatch");
static_assert(offsetof(Text2DPrivateCacheBitmap, minSizeAutoFit) == 20, "Text2DPrivateCacheBitmap.minSizeAutoFit offset mismatch");
static_assert(offsetof(Text2DPrivateCacheBitmap, maxSizeAutoFit) == 24, "Text2DPrivateCacheBitmap.maxSizeAutoFit offset mismatch");
static_assert(offsetof(Text2DPrivateCacheBitmap, rect) == 28, "Text2DPrivateCacheBitmap.rect offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_1_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DPrivateBitmap) == 28, "Text2DPrivateBitmap size mismatch");
static_assert(offsetof(Text2DPrivateBitmap, bounds) == 0, "Text2DPrivateBitmap.bounds offset mismatch");
static_assert(offsetof(Text2DPrivateBitmap, fontScale) == 16, "Text2DPrivateBitmap.fontScale offset mismatch");
static_assert(offsetof(Text2DPrivateBitmap, size) == 24, "Text2DPrivateBitmap.size offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DPrivateBitmap) == 28, "Text2DPrivateBitmap size mismatch");
static_assert(offsetof(Text2DPrivateBitmap, bounds) == 0, "Text2DPrivateBitmap.bounds offset mismatch");
static_assert(offsetof(Text2DPrivateBitmap, fontScale) == 16, "Text2DPrivateBitmap.fontScale offset mismatch");
static_assert(offsetof(Text2DPrivateBitmap, size) == 24, "Text2DPrivateBitmap.size offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_2_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DPrivateNative) == 20, "Text2DPrivateNative size mismatch");
static_assert(offsetof(Text2DPrivateNative, bounds) == 0, "Text2DPrivateNative.bounds offset mismatch");
static_assert(offsetof(Text2DPrivateNative, size) == 16, "Text2DPrivateNative.size offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DPrivateNative) == 20, "Text2DPrivateNative size mismatch");
static_assert(offsetof(Text2DPrivateNative, bounds) == 0, "Text2DPrivateNative.bounds offset mismatch");
static_assert(offsetof(Text2DPrivateNative, size) == 16, "Text2DPrivateNative.size offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_3_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(TextPrivateFontName) == 2, "TextPrivateFontName size mismatch");
static_assert(offsetof(TextPrivateFontName, c) == 0, "TextPrivateFontName.c offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(TextPrivateFontName) == 2, "TextPrivateFontName size mismatch");
static_assert(offsetof(TextPrivateFontName, c) == 0, "TextPrivateFontName.c offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_4_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(TextPrivateString) == 2, "TextPrivateString size mismatch");
static_assert(offsetof(TextPrivateString, c) == 0, "TextPrivateString.c offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(TextPrivateString) == 2, "TextPrivateString size mismatch");
static_assert(offsetof(TextPrivateString, c) == 0, "TextPrivateString.c offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_5_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DAutoFit) == 8, "Text2DAutoFit size mismatch");
static_assert(offsetof(Text2DAutoFit, minSize) == 0, "Text2DAutoFit.minSize offset mismatch");
static_assert(offsetof(Text2DAutoFit, maxSize) == 4, "Text2DAutoFit.maxSize offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DAutoFit) == 8, "Text2DAutoFit size mismatch");
static_assert(offsetof(Text2DAutoFit, minSize) == 0, "Text2DAutoFit.minSize offset mismatch");
static_assert(offsetof(Text2DAutoFit, maxSize) == 4, "Text2DAutoFit.maxSize offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_6_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DRenderer) == 20, "Text2DRenderer size mismatch");
static_assert(offsetof(Text2DRenderer, style) == 0, "Text2DRenderer.style offset mismatch");
static_assert(offsetof(Text2DRenderer, pivot) == 8, "Text2DRenderer.pivot offset mismatch");
static_assert(offsetof(Text2DRenderer, blending) == 16, "Text2DRenderer.blending offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DRenderer) == 20, "Text2DRenderer size mismatch");
static_assert(offsetof(Text2DRenderer, style) == 0, "Text2DRenderer.style offset mismatch");
static_assert(offsetof(Text2DRenderer, pivot) == 8, "Text2DRenderer.pivot offset mismatch");
static_assert(offsetof(Text2DRenderer, blending) == 16, "Text2DRenderer.blending offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_7_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(TextString) == 2, "TextString size mismatch");
static_assert(offsetof(TextString, c) == 0, "TextString.c offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(TextString) == 2, "TextString size mismatch");
static_assert(offsetof(TextString, c) == 0, "TextString.c offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_8_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DStyle) == 20, "Text2DStyle size mismatch");
static_assert(offsetof(Text2DStyle, color) == 0, "Text2DStyle.color offset mismatch");
static_assert(offsetof(Text2DStyle, size) == 16, "Text2DStyle.size offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DStyle) == 20, "Text2DStyle size mismatch");
static_assert(offsetof(Text2DStyle, color) == 0, "Text2DStyle.color offset mismatch");
static_assert(offsetof(Text2DStyle, size) == 16, "Text2DStyle.size offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_9_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DStyleBitmapFont) == 8, "Text2DStyleBitmapFont size mismatch");
static_assert(offsetof(Text2DStyleBitmapFont, font) == 0, "Text2DStyleBitmapFont.font offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DStyleBitmapFont) == 8, "Text2DStyleBitmapFont size mismatch");
static_assert(offsetof(Text2DStyleBitmapFont, font) == 0, "Text2DStyleBitmapFont.font offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_10_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DStyleNativeFont) == 16, "Text2DStyleNativeFont size mismatch");
static_assert(offsetof(Text2DStyleNativeFont, font) == 0, "Text2DStyleNativeFont.font offset mismatch");
static_assert(offsetof(Text2DStyleNativeFont, italic) == 8, "Text2DStyleNativeFont.italic offset mismatch");
static_assert(offsetof(Text2DStyleNativeFont, weight) == 12, "Text2DStyleNativeFont.weight offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(Text2DStyleNativeFont) == 16, "Text2DStyleNativeFont size mismatch");
static_assert(offsetof(Text2DStyleNativeFont, font) == 0, "Text2DStyleNativeFont.font offset mismatch");
static_assert(offsetof(Text2DStyleNativeFont, italic) == 8, "Text2DStyleNativeFont.italic offset mismatch");
static_assert(offsetof(Text2DStyleNativeFont, weight) == 12, "Text2DStyleNativeFont.weight offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_11_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(NativeFont) == 8, "NativeFont size mismatch");
static_assert(offsetof(NativeFont, name) == 0, "NativeFont.name offset mismatch");
static_assert(offsetof(NativeFont, worldUnitsToPt) == 4, "NativeFont.worldUnitsToPt offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(NativeFont) == 8, "NativeFont size mismatch");
static_assert(offsetof(NativeFont, name) == 0, "NativeFont.name offset mismatch");
static_assert(offsetof(NativeFont, worldUnitsToPt) == 4, "NativeFont.worldUnitsToPt offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_12_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(BitmapFont) == 20, "BitmapFont size mismatch");
static_assert(offsetof(BitmapFont, textureAtlas) == 0, "BitmapFont.textureAtlas offset mismatch");
static_assert(offsetof(BitmapFont, size) == 8, "BitmapFont.size offset mismatch");
static_assert(offsetof(BitmapFont, ascent) == 12, "BitmapFont.ascent offset mismatch");
static_assert(offsetof(BitmapFont, descent) == 16, "BitmapFont.descent offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(BitmapFont) == 20, "BitmapFont size mismatch");
static_assert(offsetof(BitmapFont, textureAtlas) == 0, "BitmapFont.textureAtlas offset mismatch");
static_assert(offsetof(BitmapFont, size) == 8, "BitmapFont.size offset mismatch");
static_assert(offsetof(BitmapFont, ascent) == 12, "BitmapFont.ascent offset mismatch");
static_assert(offsetof(BitmapFont, descent) == 16, "BitmapFont.descent offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_13_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(CharacterInfoBuffer) == 40, "CharacterInfoBuffer size mismatch");
static_assert(offsetof(CharacterInfoBuffer, data) == 0, "CharacterInfoBuffer.data offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(CharacterInfoBuffer) == 40, "CharacterInfoBuffer size mismatch");
static_assert(offsetof(CharacterInfoBuffer, data) == 0, "CharacterInfoBuffer.data offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_14_cid = -1;


#ifdef UT_32BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(GlyphPrivateBuffer) == 48, "GlyphPrivateBuffer size mismatch");
static_assert(offsetof(GlyphPrivateBuffer, c) == 0, "GlyphPrivateBuffer.c offset mismatch");
}}}
#endif

#ifdef UT_64BIT
namespace Unity { namespace Tiny { namespace Text { 
static_assert(sizeof(GlyphPrivateBuffer) == 48, "GlyphPrivateBuffer size mismatch");
static_assert(offsetof(GlyphPrivateBuffer, c) == 0, "GlyphPrivateBuffer.c offset mismatch");
}}}
#endif

DLLEXPORT ComponentTypeId priv_bind_Unity_Tiny_Text_15_cid = -1;


