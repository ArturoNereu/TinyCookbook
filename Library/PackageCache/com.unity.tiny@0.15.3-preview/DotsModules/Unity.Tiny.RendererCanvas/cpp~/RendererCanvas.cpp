#include <stdint.h>
#include <emscripten.h>
#include <cstring>
#include <string>
#include <memory>
#include <vector>

#include "GeminiMath.h"
#include "RendererCanvasLib.h"
#include "bind-Unity_Tiny_HTML.h"
#include "bind-Unity_Tiny_Core2D.h"
#include "bind-Unity_Tiny_Image2D.h"
#include "bind-Unity_Tiny_Image2DIOHTML.h"
#include "bind-Unity_Tiny_RendererCanvas.h"
#include "bind-Unity_Tiny_Text.h"
#include "bind-Unity_Tiny_TextHTML.h"
//#include "bind-Unity_Tiny_Shape2D.h"

#include "zeroplayer.h"
#include "EntityWrappers.h"
#include "CoreHelpers.h"

using namespace Unity::Tiny::Core2D;
using namespace Unity::Tiny::HTML;
using namespace Unity::Tiny::Math;
using namespace Unity::Mathematics;
using namespace Unity::Tiny::Rendering;
using namespace Unity::Entities;
using namespace Unity::Tiny::Text;
using namespace Unity::Tiny::TextHTML;
using namespace Unity::Tiny::Core;

#ifndef __EMSCRIPTEN__
#define EM_ASM(...) ((void)0)
#define EM_ASM_INT(...) ((void)0)
#define MAIN_THREAD_ASYNC_EM_ASM(...) ((void)0)
#endif

#undef Assert
#define Assert(x) if(!(x)) { printf ("Assert \"%s\" failed at %s:%i\n", #x, __FILE__, __LINE__ ); }

// OLD HEADER -----------------------------------------------------------------------------------------------------------------------

class HTMLRendererPrivate
{
public:
    bool init(EntityManager& w) ;
    void deInit(EntityManager& w) ;

    bool initialized;

    void beginScene(EntityManager& man, const Vector2f &targetSize) ;
    void endScene(EntityManager& man) ;
    void beginCamera(EntityManager& man, Entity e, const Vector2f& targetSize) ;
    void endCamera(EntityManager& man, Entity e) ;
    void renderSpriteBatch(int n, DisplayListEntry* list, EntityManager& world) ;
    bool beginRenderToTexture(EntityManager& man, Entity e, const Camera2DRenderToTexture *rtt) ;
    void endRenderToTexture(EntityManager& man, Entity e, const Camera2DRenderToTexture *rtt) ;

    HTMLRendererPrivate() : initialized(false) {}

    void renderShape(EntityManager &man, DisplayListEntry& de);
    void renderText(EntityManager &man, DisplayListEntry& de);
    void renderTextWithNativeFont(EntityManager &man, DisplayListEntry& de);
    void renderTextWithBitmapFont(EntityManager &man, DisplayListEntry& de);
    void renderNormalSpriteBatch(EntityManager& man, const DisplayListEntry*  list, int n);
    void renderTiledSprite(EntityManager& man, DisplayListEntry& de);
    void renderSlicedSprite(EntityManager& man, DisplayListEntry& de);
    void updateRTT(EntityManager& w);

    static inline bool isWhite(const Color& c);
    static int updateOrCreatePattern(Sprite2DRendererHTML *spriteHtml, int imageIndex,
                            const Rect &sb,const Color &color);
    static int updateOrCreateTintedSprite(Sprite2DRendererHTML *spriteHtml, int imageIndex,
                            const Rect &sb,const Color &color);
    static int updateOrCreateTintedTile(TileHTML* tileHtml, int imageIndex, const Rect& sb, const Color& color);
    static Rect fixupPixelRectangle(const Rect& src, const float2& imagePixelSize);
    static bool clipSliceRectsTopRight(Rect& sRect, Rect& dRect, const Vector2f& clipAt);
    static bool clipSliceRectsBottomLeft(Rect& sRect, Rect& dRect, const Vector2f& clipAt);
    static void makeSlice9Parts(const Rect& sb, const Rect& sbInner, const Rect& b,
                            float pToW, DrawMode tileMode, std::vector<Rect>& outParts);
    static void makeCanvasTransform(const Matrix4x4f& lhs, const Matrix4x4f& rhs, float& txa, float& txb, float& txc,
                                float& txd, float& txe, float& txf);
    static inline void setBlendingAndSmoothing(BlendOp blendop, bool smooth);
    static int updateOrCreateTintedGlyph(TintedGlyphHTML* glyphBuffer, int bufferLength, TintedGlyphIndex* currentIndex, uint32_t value, int imageIndex, const Rect& region,
    const Color& color);
    static void releaseTintedGlyphs(EntityManager &man, Entity entity);

    Matrix4x4f currentViewMatrix;
    Vector2f currentCamClip;
};

// IMPL -----------------------------------------------------------------------------------------------------------------------
static inline float nextFullScale(float x, float bias= .5f)
{
    // bias is "Stretch Value" in bigU
    // helper used for computing scale factor for adaptive tiling
    if (x < 0.0f)
        return -nextFullScale(-x);
    x += bias;
    if (x <= 1.0f)
        return 1.0f;
    return (float)((int)x);
}

static bool operator==(const HTMLTintedSpriteDesc& a, const HTMLTintedSpriteDesc& b) {
    return memcmp(&a, &b, sizeof(HTMLTintedSpriteDesc))==0;
}

static inline Matrix4x4f UMtoM (const float4x4 &src) {
    Matrix4x4f r;
    memcpy ( &r, &src, 4*4*4 );
    return r;
}

static inline Vector2f UMtoM (const float2 &src) {
    Vector2f r;
    memcpy ( &r, &src, 4*2 );
    return r;
}

bool
HTMLRendererPrivate::init(EntityManager& w)
{
    if (initialized)
        return true;
    if (!js_canvasInit())
        return false;

    InitComponentId<Image2D>();
    InitComponentId<Image2DHTML>();
    InitComponentId<Image2DRenderToTexture>();
    //InitComponentId<Image2DRenderToTextureHTML>();
    InitComponentId<Camera2D>();
    //InitComponentId<Shape2DRenderer>();
    //InitComponentId<Shape2DVertex>();
    //InitComponentId<Shape2DIndex>();
    InitComponentId<Sprite2DRendererOptions>();
    InitComponentId<Sprite2DRenderer>();
    InitComponentId<Sprite2DBorder>();
    InitComponentId<Sprite2D>();
    InitComponentId<Text2DRenderer>();
    InitComponentId<Text2DStyleNativeFont>();
    InitComponentId<Text2DStyleBitmapFont>();
    InitComponentId<Text2DStyle>();
    InitComponentId<TextString>();
    InitComponentId<Text2DPrivateNative>();
    InitComponentId<Text2DPrivateCacheHTML>();
    InitComponentId<Text2DPrivateCacheBitmap>();
    InitComponentId<TextPrivateFontName>();
    InitComponentId<TextPrivateString>();
    InitComponentId<Text2DPrivateBitmap>();
    InitComponentId<GlyphPrivateBuffer>();
    InitComponentId<BitmapFont>();
    InitComponentId<TintedGlyphHTML>();
    InitComponentId<TextBitmapHTML>();
    InitComponentId<TintedGlyphIndex>();

#ifdef DEVELOPMENT
    if ( !js_canvasSupportsMultiply() ) {
        ut::logWarning("Canvas does not support multiply blend op.");
    }
#endif
    initialized = true;
    return true;
}

void
HTMLRendererPrivate::deInit(EntityManager& w)
{
    if (!initialized)
        return;
    /*
    // remove private components
    EntityCommandBuffer ecb;
    w.forEach<Entity,Image2DRenderToTextureHTML>([&ecb](Entity &e, Image2DRenderToTextureHTML &rtt){
        if (rtt.displaying.imageIndex)
            js_canvasFreeImage(rtt.displaying.imageIndex);
        if (rtt.rendering.imageIndex)
            js_canvasFreeImage(rtt.rendering.imageIndex);
        ecb.removeComponent<Image2DRenderToTextureHTML>(e);
    }, BufferingMode::Unbuffered);
    ecb.commit(w);
    */
    initialized = false;
}

static void releaseTintedGlyphs(EntityManager& man, Entity entity);

void
HTMLRendererPrivate::beginScene(EntityManager& man, const Vector2f& targetSize)
{
    // release private component
    /*
    man.forEach<Entity, Sprite2DRendererHTML>({ComponentSpec::subtractive<Sprite2DRenderer>()},
                                              [&man](Entity& e, Sprite2DRendererHTML& sr) {
                                                  if (sr.tintedIndex > 0)
                                                      js_canvasReleaseTintedSprite(sr.tintedIndex);
                                                  man.removeComponent<Sprite2DRendererHTML>(e);
                                              });
*/


    // make sure we have a private component
    /*
    man.forEach<Entity, Sprite2DRenderer>(
        {ComponentSpec::subtractive<Sprite2DRendererHTML>()},
        [&man](Entity& e, Sprite2DRenderer&) { man.addComponent<Sprite2DRendererHTML>(e); });*/

    // update rtt components as well
    updateRTT(man);
}

void
HTMLRendererPrivate::endScene(EntityManager &man)
{
}

bool
HTMLRendererPrivate::beginRenderToTexture(EntityManager& man, Entity e, const Camera2DRenderToTexture *rtt) {
    /*
    if (!man.hasComponent<Image2DRenderToTextureHTML>(rtt->target) ||
        !man.hasComponent<Image2DHTML>(rtt->target)) {
        Assert(0);
        return false;
    }
    Image2DHTML *destHTML = man.getComponentPtrUnsafe<Image2DHTML>(rtt->target);
    Image2DRenderToTextureHTML *dest = man.getComponentPtrUnsafe<Image2DRenderToTextureHTML>(rtt->target);
    Image2D *destImg = man.getComponentPtrUnsafe<Image2D>(rtt->target);
    // (re)allocate dest target if needed - this smoothly handles resolution changes and init
    if (dest->rendering.w != rtt->width || dest->rendering.h != rtt->height || !dest->rendering.imageIndex) {
        if (!dest->rendering.imageIndex)
            dest->rendering.imageIndex = js_canvasMakeRenderTarget(rtt->width, rtt->height);
        else
            js_canvasResizeRenderTarget(dest->rendering.imageIndex, rtt->width, rtt->height);
        dest->rendering.w = rtt->width;
        dest->rendering.h = rtt->height;
    }
    js_canvasPushImageAsContext(dest->rendering.imageIndex);
    return true;
    */
    return false;
}

void
HTMLRendererPrivate::endRenderToTexture(EntityManager& man, Entity e, const Camera2DRenderToTexture *rtt) {
    /*
    Image2DHTML *destHTML = man.getComponentPtrUnsafe<Image2DHTML>(rtt->target);
    Image2DRenderToTextureHTML *dest = man.getComponentPtrUnsafe<Image2DRenderToTextureHTML>(rtt->target);
    Image2D *destImg = man.getComponentPtrUnsafe<Image2D>(rtt->target);
    std::swap(dest->displaying, dest->rendering);
    // update image
    destImg->status = ImageStatus::Loaded; // now ready to use
    destImg->imagePixelSize.Set((float)dest->displaying.w, (float)dest->displaying.h);
    destImg->hasAlpha = true;
    // update native image to rendered target
    if (!destHTML->externalOwner && destHTML->imageIndex)
        js_canvasFreeImage(destHTML->imageIndex);
    destHTML->externalOwner = true;
    destHTML->imageIndex = dest->displaying.imageIndex;
    // bind back original target
    js_canvasPopContext();
    */
}

static inline void ClampRect(Rect &r, const Rect &bounds)
{
    float x2 = r.x + r.width;
    float y2 = r.y + r.height;
    float rx2 = bounds.x + bounds.width;
    float ry2 = bounds.y + bounds.height;

    if (r.x < bounds.x) r.x = bounds.x;
    if (x2 > rx2) x2 = rx2;
    if (r.y < bounds.y) r.y = bounds.y;
    if (y2 > ry2) y2 = ry2;

    r.width = x2 - r.x;
    if (r.width < 0) r.width = 0;

    r.height = y2 - r.y;
    if (r.height < 0) r.height = 0;
}

void
HTMLRendererPrivate::beginCamera(EntityManager& man, Entity e, const Vector2f& targetSize)
{
    Assert(man.hasComponent<Camera2D>(e));
    const Camera2D* cam = man.getComponentPtrConstUnsafe<Camera2D>(e);
    // always save context
    MAIN_THREAD_ASYNC_EM_ASM({ ut._HTML.canvasContext.save(); });
    Vector2f actualTargetSize;
    Rect viewPort;
    if (cam->rect.width <= 0.0f || cam->rect.height <= 0.0f || cam->rect.width>=1.0f || cam->rect.height>=1.0f) {
        actualTargetSize = targetSize;
        viewPort = Rect{0, 0, targetSize.x, targetSize.y};
    } else {
        Rect targetRect{0, 0, targetSize.x, targetSize.y};
        Rect clipRect{cam->rect.x * targetSize.x, cam->rect.y * targetSize.y, cam->rect.width * targetSize.x,
            cam->rect.height * targetSize.y};
        // set a clip rect
        ClampRect(clipRect,targetRect);
        actualTargetSize = Vector2f(clipRect.width, clipRect.height);
        MAIN_THREAD_ASYNC_EM_ASM(
            {
                ut._HTML.canvasContext.beginPath();
                ut._HTML.canvasContext.rect($0, $1, $2, $3);
                ut._HTML.canvasContext.clip();
            },
            clipRect.x, targetSize.y - (clipRect.y + clipRect.height), clipRect.width, clipRect.height);
        viewPort = clipRect;
    }
    if (cam->clearFlags == CameraClearFlags::SolidColor) {
        // draw fullscreen rect, will be clipped by above clip path anyhow
        js_canvasClear((int)(cam->backgroundColor.r * 255.f), (int)(cam->backgroundColor.g * 255.f), (int)(cam->backgroundColor.b * 255.f),
                       cam->backgroundColor.a, (int)targetSize.x, (int)targetSize.y);
    }
    // create a gl-like view transform
    Matrix4x4f deviceMatrix; // from normalized device coordinates to canvas pixels coordinates
    float dy = targetSize.y - (viewPort.height + viewPort.y);
    deviceMatrix.SetScaleAndPosition(Vector3f(viewPort.width * .5f, -viewPort.height * .5f, 1.0f),
                                     Vector3f(viewPort.width * .5f + viewPort.x, viewPort.height * .5f + dy, 0.0f));

    float tsx = actualTargetSize.y / (cam->halfVerticalSize * actualTargetSize.x);
    float tsy = 1.0f / cam->halfVerticalSize;
    Matrix4x4f viewMatrix; // camera aspect
    viewMatrix.SetScaleAndPosition(Vector3f(tsx, tsy, 1.0f), Vector3f(0.0f, 0.0f, 0.0f));

    MultiplyMatrices4x4(&deviceMatrix, &viewMatrix, &currentViewMatrix);
    currentCamClip.Set(1.0f / tsx, 1.0f / tsy);
}

void
HTMLRendererPrivate::endCamera(EntityManager& man, Entity e)
{
    MAIN_THREAD_ASYNC_EM_ASM({ ut._HTML.canvasContext.restore(); });
}

bool
HTMLRendererPrivate::isWhite(const Color& c)
{
    return c.r >= 1.0f && c.g >= 1.0f && c.b >= 1.0f;
}

int
HTMLRendererPrivate::updateOrCreateTintedSprite(Sprite2DRendererHTML* spriteHtml, int imageIndex, const Rect& sb,
                                                const Color& color)
{
    HTMLTintedSpriteDesc d;
    d.imageIndex = imageIndex;
    d.texRect = sb;
    d.tintColor = color;
    if (spriteHtml->tintedIndex > 0 && d == spriteHtml->desc)
        return spriteHtml->tintedIndex;

    // need to get a new one, release old one first
    if (spriteHtml->tintedIndex > 0)
        js_canvasReleaseTintedSprite(spriteHtml->tintedIndex);

    int newTintIndex = js_canvasMakeTintedSprite(imageIndex, sb.x, sb.y, sb.width, sb.height, (int)(color.r * 255.0f),
                                                 (int)(color.g * 255.0f), (int)(color.b * 255.0f));

    spriteHtml->desc = d;
    spriteHtml->tintedIndex = newTintIndex;
    spriteHtml->haspattern = false;
    return newTintIndex;
}

static int GetGlyphDescIndex(TintedGlyphHTML* glyphHtml, int bufferLength, uint32_t value);

int
HTMLRendererPrivate::updateOrCreateTintedGlyph(TintedGlyphHTML* glyphBuffer, int bufferLength, TintedGlyphIndex* currentIndex, uint32_t value, int imageIndex, const Rect& region,
    const Color& color)
{
    HTMLTintedSpriteDesc d{ imageIndex, region, color };
    int newTintIndex = -1;

    int glyphIndex = GetGlyphDescIndex(glyphBuffer, bufferLength, value);
    if (glyphIndex >= 0)
    {
        TintedGlyphHTML glyph = *(glyphBuffer + glyphIndex);
        int currentTintIndex = glyph.tintedIndex;
        //Found the glyph desc
        if (currentTintIndex > 0 && d == glyph.desc)
            return currentTintIndex;
        // need to get a new one, release old one first
        if (currentTintIndex > 0)
        {
            js_canvasReleaseTintedSprite(currentTintIndex);
            newTintIndex = js_canvasMakeTintedSprite(imageIndex, region.x, region.y, region.width, region.height, (int)(color.r * 255.0f),
                (int)(color.g * 255.0f), (int)(color.b * 255.0f));
            glyph.tintedIndex = newTintIndex;
            glyph.desc = d;
            glyph.value = value;
        }
    }
    else {

        newTintIndex = js_canvasMakeTintedSprite(imageIndex, region.x, region.y, region.width, region.height, (int)(color.r * 255.0f),
            (int)(color.g * 255.0f), (int)(color.b * 255.0f));
        TintedGlyphHTML glyph = {};
        glyph.desc = d;
        glyph.value = value;
        glyph.tintedIndex = newTintIndex;
        *(glyphBuffer + currentIndex->index) = glyph;
        currentIndex->index++;
    }
    return newTintIndex;
}

int
HTMLRendererPrivate::updateOrCreateTintedTile(TileHTML* tileHtml, int imageIndex, const Rect& sb,
                                                const Color& color)
{
    HTMLTintedSpriteDesc d{imageIndex, sb, color};
    if (tileHtml->tintedIndex > 0 && d == tileHtml->desc)
        return tileHtml->tintedIndex;

    // need to get a new one, release old one first
    if (tileHtml->tintedIndex > 0)
        js_canvasReleaseTintedSprite(tileHtml->tintedIndex);

    int newTintIndex = js_canvasMakeTintedSprite(imageIndex, sb.x, sb.y, sb.width, sb.height, (int)(color.r * 255.0f),
                                                 (int)(color.g * 255.0f), (int)(color.b * 255.0f));

    tileHtml->desc = d;
    tileHtml->tintedIndex = newTintIndex;
    return newTintIndex;
}


int
HTMLRendererPrivate::updateOrCreatePattern(Sprite2DRendererHTML* spriteHtml, int imageIndex, const Rect& sb,
                                           const Color& color)
{
    int idx = updateOrCreateTintedSprite(spriteHtml, imageIndex, sb, color);
    if ( spriteHtml->haspattern )
        return idx;
    js_canvasMakePattern(idx);
    spriteHtml->haspattern = true;
    return idx;
}

Rect
HTMLRendererPrivate::fixupPixelRectangle(const Rect& src, const float2& imagePixelSize)
{
    Rect sb = src;
    sb.x *= imagePixelSize.x;
    sb.width *= imagePixelSize.x;
    sb.y *= imagePixelSize.y;
    sb.height *= imagePixelSize.y;
    sb.y = imagePixelSize.y - sb.y -
           sb.height; // change y origin, canvas images have top left origin, our rects have bottom left origin
    return sb;
}

void
HTMLRendererPrivate::renderShape(EntityManager& man, DisplayListEntry& de)
{
    /*
    const Shape2DRenderer* sr = man.getComponentPtrConstUnsafe<Shape2DRenderer>(de.e);
    const ComponentNativeBuffer<Shape2DVertex> shape = man.getBuffer<Shape2DVertex>(sr->shape);
    const Shape2DVertex *verts = shape.data();
    int nv = (int)shape.size();
    const Shape2DIndex *inds = 0;
    int ni = 0;
    if (man.hasBuffer<Shape2DIndex>(sr->shape)) {
        const ComponentNativeBuffer<Shape2DIndex> sinds = man.getBuffer<Shape2DIndex>(sr->shape);
        ni = (int)sinds.size();
        inds = sinds.data();
    }
    Assert(nv <= sMaxSolidVertices);
    Assert(ni <= sMaxSolidIndices);

    float txa, txb, txc, txd, txe, txf;
    makeCanvasTransform(currentViewMatrix, de.finalMatrix, txa, txb, txc, txd, txe, txf);
    js_canvasSetTransformOnly(txa, txb, txc, txd, txe, txf);

    setBlendingAndSmoothing(sr->blending, true);

    js_canvasRenderShape((const float*)verts, nv, (const uint16_t*)inds, ni,
                       sr->color.r * 255.0f, sr->color.g * 255.0f, sr->color.b * 255.0f, sr->color.a);
                       */
}

void
HTMLRendererPrivate::renderTextWithNativeFont(EntityManager &man, DisplayListEntry& de)
{
    const Text2DRenderer* textRenderer = man.getComponentPtrConstUnsafe<Text2DRenderer>(de.e);
    const Text2DStyle* textStyle = man.getComponentPtrConstUnsafe<Text2DStyle>(textRenderer->style);
    const Text2DStyleNativeFont* nativeStyle = man.getComponentPtrConstUnsafe<Text2DStyleNativeFont>(textRenderer->style);

    Text2DPrivateCacheHTML* textHTML = man.getComponentPtrUnsafe<Text2DPrivateCacheHTML>(de.e);
    const Text2DPrivateNative* textPrivate = man.getComponentPtrConstUnsafe<Text2DPrivateNative>(de.e);

    setBlendingAndSmoothing(textRenderer->blending, true);

    Matrix4x4f finalMatrix = UMtoM(de.finalMatrix);

    float scaleX = currentViewMatrix.MaxAbsScale() * finalMatrix.MaxAbsScale();
    float transX = de.inBounds.x - de.inBounds.width * textRenderer->pivot.x + de.inBounds.width / 2;
    float transY = de.inBounds.y - de.inBounds.height * textRenderer->pivot.y + de.inBounds.height / 2;

    //UTINY-1721: Some fonts have weird charater spacing with small font size.
    //We can try to render the font with a bigger size (*scaleX), and apply the inverse scale.
    if (std::abs(scaleX) <= 1)
        scaleX = 1;

    /*Matrix4x4f m, mSize;
    mSize.SetScaleAndPosition(Vector3f(1/scaleX, 1/scaleX, 1.0f),
    Vector3f(transX, transY, 0.0f));
    MultiplyMatrices4x4(&de->finalMatrix, &mSize, &m);*/
    Matrix4x4f m = finalMatrix;
    m.Get(0, 0) = finalMatrix.Get(0, 0) / scaleX;
    m.Get(0, 1) = finalMatrix.Get(0, 1) / scaleX;
    m.Get(0, 3) = finalMatrix.Get(0, 0) * transX + finalMatrix.Get(0, 1) * transY + finalMatrix.Get(0, 3);

    m.Get(1, 0) = finalMatrix.Get(1, 0) / scaleX;
    m.Get(1, 1) = finalMatrix.Get(1, 1) / scaleX;
    m.Get(1, 3) = finalMatrix.Get(1, 0) * transX + finalMatrix.Get(1, 1) * transY + finalMatrix.Get(1, 3);

    m.Get(2, 0) = finalMatrix.Get(2, 0) / scaleX;
    m.Get(2, 1) = finalMatrix.Get(2, 1) / scaleX;
    m.Get(2, 3) = finalMatrix.Get(2, 0) * transX + finalMatrix.Get(2, 1) * transY + finalMatrix.Get(2, 3);

    m.Get(3, 0) = finalMatrix.Get(3, 0) / scaleX;
    m.Get(3, 1) = finalMatrix.Get(3, 1) / scaleX;
    m.Get(3, 3) = finalMatrix.Get(3, 0) * transX + finalMatrix.Get(3, 1) * transY + finalMatrix.Get(3, 3);

    float txa, txb, txc, txd, txe, txf;
    makeCanvasTransform(currentViewMatrix, m, txa, txb, txc, txd, txe, txf);
    js_canvasSetTransformOnly(txa, txb, txc, txd, txe, txf);

    // Render text in canvas at current location
    float size = textPrivate->size * scaleX;

    //String are UTf16 encoded in c# in 16 bits
    uint16_t* textBuffer = (uint16_t*)man.getBufferElementDataPtrUnsafe<TextString>(de.e);
    int textLength = man.getBufferElementDataLength<TextString>(de.e);
    uint16_t* familyBuffer = (uint16_t*)man.getBufferElementDataPtrUnsafe<TextPrivateFontName>(de.e);
    int familyLength = man.getBufferElementDataLength<TextPrivateFontName>(de.e);

    //Since c# strings don't have a null terminated character, and some browsers don't support it.
    //Let's make sure the text/family buffers are length sized and have a null terminated character
    std::vector<uint16_t> text(textLength + 1);
    copyBufferAndAppendNull(text.data(), textBuffer, textLength);

    std::vector<uint16_t> family(familyLength + 1);
    copyBufferAndAppendNull(family.data(), familyBuffer, familyLength);

    js_renderTextTo2DCanvas(text.data(), family.data(), size,
        nativeStyle->weight, nativeStyle->italic, textStyle->color.r * 255,
        textStyle->color.g * 255, textStyle->color.b * 255, textStyle->color.a * 255, 0, 0);
}

void
HTMLRendererPrivate::renderTextWithBitmapFont(EntityManager &man, DisplayListEntry& de)
{
    const Text2DRenderer* textRenderer = man.getComponentPtrConstUnsafe<Text2DRenderer>(de.e);
    const Text2DStyle* textStyle = man.getComponentPtrConstUnsafe<Text2DStyle>(textRenderer->style);
    const Text2DStyleBitmapFont* bitmapFontStyle =
        man.getComponentPtrConstUnsafe<Text2DStyleBitmapFont>(textRenderer->style);
    const Text2DPrivateBitmap* textPrivate = man.getComponentPtrConstUnsafe<Text2DPrivateBitmap>(de.e);

    const GlyphPrivate* glyphBuffer = (GlyphPrivate*)man.getBufferElementDataPtrConstUnsafe<GlyphPrivateBuffer>(de.e);
    int glyphCount = man.getBufferElementDataLength<GlyphPrivateBuffer>(de.e);
    if (glyphCount == 0)
        return;

    const BitmapFont* bitmapFont = man.getComponentPtrConstUnsafe<BitmapFont>(bitmapFontStyle->font);
    const Image2DHTML* imgHTML = man.getComponentPtrConstUnsafe<Image2DHTML>(bitmapFont->textureAtlas);
    const Image2D* img2D = man.getComponentPtrConstUnsafe<Image2D>(bitmapFont->textureAtlas);
    const uint16_t* textBuffer = (uint16_t*)man.getBufferElementDataPtrConstUnsafe<TextString>(de.e);
    uint16_t* textHtml = (uint16_t*)man.getBufferElementDataPtrUnsafe<TextBitmapHTML>(de.e);

    if (memcmp(textHtml, textBuffer, sizeof(uint16_t))==0) {
        //Release tinted sprites if text has changed
        releaseTintedGlyphs(man, de.e);
        textHtml = (uint16_t*)textBuffer;
    }

    setBlendingAndSmoothing(textRenderer->blending, !img2D->disableSmoothing);

    Matrix4x4f finalMatrix = UMtoM(de.finalMatrix);
    float txa, txb, txc, txd, txe, txf;
    makeCanvasTransform(currentViewMatrix, finalMatrix, txa, txb, txc, txd, txe, txf);

    for (int i = 0; i < glyphCount; ++i) {
        GlyphPrivate glyph = *(glyphBuffer + i);

        //Adjust position and dim per glyph according to pivot position and text origin (originY is baseline)
        float transX = de.inBounds.width * textRenderer->pivot.x;
        float transY = de.inBounds.height * textRenderer->pivot.y - std::abs(bitmapFont->descent);
        Rect rec;
        rec.x = (glyph.position.x - glyph.ci.width / 2 - transX) * textPrivate->fontScale.x + de.inBounds.x;
        int diff = glyph.ci.height - glyph.ci.bearingY;
        rec.y = (glyph.position.y - glyph.ci.height / 2 - glyph.ci.height + diff * 2 + transY) * textPrivate->fontScale.y - de.inBounds.y;

        rec.width = glyph.ci.width * textPrivate->fontScale.x;
        rec.height = glyph.ci.height * textPrivate->fontScale.y;

        if (rec.width == 0 || rec.height == 0)
            continue;

        Rect characterRegion = fixupPixelRectangle(glyph.ci.characterRegion, img2D->imagePixelSize);

        if (isWhite(textStyle->color)) {
            // can render directly
            js_canvasRenderNormalSpriteWhite(
                txa, txb, txc, txd, txe, txf, textStyle->color.a,
                imgHTML->imageIndex, characterRegion.x,
                characterRegion.y,
                characterRegion.width,
                characterRegion.height, rec.x, rec.y,
                rec.width, rec.height);
        }
        else {
            TintedGlyphHTML* tintedGlyphs = (TintedGlyphHTML*)man.getBufferElementDataPtrUnsafe<TintedGlyphHTML>(de.e);
            TintedGlyphIndex* currentTintedIndexBuffer = man.getComponentPtrUnsafe<TintedGlyphIndex>(de.e);
            int length = man.getBufferElementDataLength<TintedGlyphHTML>(de.e);
            int tintIndex = updateOrCreateTintedGlyph(tintedGlyphs, length, currentTintedIndexBuffer, glyph.ci.value, imgHTML->imageIndex, characterRegion, Color{ textStyle->color.r, textStyle->color.g,
                textStyle->color.b, textStyle->color.a });

            js_canvasRenderNormalSpriteTinted(
                txa, txb, txc, txd, txe, txf, textStyle->color.a,
                tintIndex, rec.x, rec.y,
                rec.width, rec.height);

        }
    }
}

void
HTMLRendererPrivate::renderText(EntityManager& man, DisplayListEntry& de)
{
    if (man.hasComponent<Text2DStyleNativeFont>(de.e)) {
        renderTextWithNativeFont(man, de);
    } else if (man.hasComponent<Text2DStyleBitmapFont>(de.e)) {
        renderTextWithBitmapFont(man, de);
    }
}

bool
HTMLRendererPrivate::clipSliceRectsTopRight(Rect& sRect, Rect& dRect, const Vector2f& clipAt)
{
    // clip dRect against clipRect, top/right only, adjust sRect after clipping
    if (dRect.x + dRect.width > clipAt.x) {
        float neww = clipAt.x - dRect.x;
        if (!(neww > 0.0f)) {
            dRect.width = 0.0f;
            return false;
        }
        sRect.width *= neww / dRect.width;
        dRect.width = neww;
    }

    if (dRect.y + dRect.height > clipAt.y) {
        float newh = clipAt.y - dRect.y;
        if (!(newh > 0.0f)) {
            dRect.height = 0.0f;
            return false;
        }
        float newSh = sRect.height * newh / dRect.height;
        sRect.y += sRect.height - newSh;
        sRect.height = newSh;
        dRect.height = newh;
    }
    return true;
}

bool
HTMLRendererPrivate::clipSliceRectsBottomLeft(Rect& sRect, Rect& dRect, const Vector2f& clipAt)
{
    // clip dRect against clipRect, bottom/left only, adjust sRect after clipping
    if (dRect.x < clipAt.x) {
        dRect.width -= clipAt.x - dRect.x;
        if (!(dRect.width > 0.0f)) {
            dRect.width = 0.0f;
            return false;
        }
        dRect.x = clipAt.x;
    }

    if (dRect.y < clipAt.y) {
        dRect.height -= clipAt.y - dRect.y;
        if (!(dRect.height > 0.0f)) {
            dRect.height = 0.0f;
            return false;
        }
        dRect.y = clipAt.y;
    }
    return true;
}

void
HTMLRendererPrivate::makeSlice9Parts(const Rect& sb, const Rect& sbInner, const Rect& b, float pToW,
                                     DrawMode tileMode, std::vector<Rect>& outParts)
{
    // sb: outer source bounds (in image coords, pixels)
    // sbInner: inner source bounds (in image coords, pixels)
    // b: dest bounds (in world units)
    // pToW: pixelsToWorldUnits
    // outParts: output is (sourcerect(image pixels), destrect(world coords)) for every part, as passed to drawImage

    // splits in pixels
    float leftW = sbInner.x - sb.x;
    float rightW = sb.width - (leftW + sbInner.width);
    float botH = sbInner.y - sb.y;
    float topH = sb.height - (botH + sbInner.height);

    float leftW2 = leftW * pToW;
    float rightW2 = rightW * pToW;
    float botH2 = botH * pToW;
    float topH2 = topH * pToW;
    // make parts for elements on the four sides

    float stepW = sbInner.width * pToW; // adjust for adaptive or stretch
    float stepH = sbInner.height * pToW;
    Assert(stepW > 0.0f && stepH > 0.0f);
    if (!(stepW > 0.0f && stepH > 0.0f))
        return;

    // step size for adaptive tiling
    float extrax = (b.width - leftW2 - rightW2) / (sbInner.width * pToW);
    float extray = (b.height - topH2 - botH2) / (sbInner.height * pToW);

    switch (tileMode) {
    case DrawMode::AdaptiveTiling:
        if (extrax > 1.0f) {
            float nrepsx = nextFullScale(extrax);
            extrax = (b.width - leftW2 - rightW2) / (sbInner.width * pToW * nrepsx);
        }
        if (extray > 1.0f) {
            float nrepsy = nextFullScale(extray);
            extray = (b.height - topH2 - botH2) / (sbInner.height * pToW * nrepsy);
        }
        stepW *= extrax;
        stepH *= extray;
        break;
    case DrawMode::Stretch:
        stepW *= extrax;
        stepH *= extray;
        break;
    case DrawMode::ContinuousTiling:
        break;
    default:
        Assert(0);
        return;
    }

    // center fill
    Vector2f centerClip(b.x + b.width - rightW2, b.y + b.height - botH2);
    for (float x = leftW2; x < b.width - rightW2; x += stepW) {
        for (float y = topH2; y < b.height - botH2; y += stepH) {
            outParts.push_back({sbInner.x, sb.y + sb.height - topH - sbInner.height, sbInner.width, sbInner.height});
            outParts.push_back({b.x + x, b.y + y, stepW, stepH});
            clipSliceRectsTopRight(outParts[outParts.size() - 2], outParts[outParts.size() - 1], centerClip);
        }
    }

    // horizontal border
    Vector2f rightClip(b.x + b.width - rightW2, b.y + b.height);
    Vector2f botLeftClip(b.x, b.y);
    for (float x = leftW2; x < b.width - rightW2; x += stepW) {
        outParts.push_back({sbInner.x, sb.y, sbInner.width, botH});
        outParts.push_back({b.x + x, b.y + b.height - botH2, stepW, botH2});
        clipSliceRectsTopRight(outParts[outParts.size() - 2], outParts[outParts.size() - 1], rightClip);
        clipSliceRectsBottomLeft(outParts[outParts.size() - 2], outParts[outParts.size() - 1], botLeftClip);

        outParts.push_back({sbInner.x, sb.y + sb.height - topH, sbInner.width, topH});
        outParts.push_back({b.x + x, b.y, stepW, topH2});
        clipSliceRectsTopRight(outParts[outParts.size() - 2], outParts[outParts.size() - 1], rightClip);
    }

    // vertical border
    Vector2f topClip(b.x + b.width, b.y + b.height - botH2);
    for (float y = topH2; y < b.height - botH2; y += stepH) {
        outParts.push_back(
            {sb.x + sb.width - rightW, sb.y + sb.height - topH - sbInner.height, rightW, sbInner.height});
        outParts.push_back({b.x + b.width - rightW2, b.y + y, rightW2, stepH});
        clipSliceRectsTopRight(outParts[outParts.size() - 2], outParts[outParts.size() - 1], topClip);
        clipSliceRectsBottomLeft(outParts[outParts.size() - 2], outParts[outParts.size() - 1], botLeftClip);

        outParts.push_back({sb.x, sb.y + sb.height - topH - sbInner.height, leftW, sbInner.height});
        outParts.push_back({b.x, b.y + y, leftW2, stepH});
        clipSliceRectsTopRight(outParts[outParts.size() - 2], outParts[outParts.size() - 1], topClip);
    }

    // make parts for all four corner elements
    outParts.push_back({sb.x + sb.width - rightW, sb.y, rightW, botH}); // C
    outParts.push_back({b.x + b.width - rightW2, b.y + b.height - botH2, rightW2, botH2});

    outParts.push_back({sb.x + sb.width - rightW, sb.y + sb.height - topH, rightW, topH}); // I
    outParts.push_back({b.x + b.width - rightW2, b.y, rightW2, topH2});

    outParts.push_back({sb.x, sb.y, leftW, botH}); // A
    outParts.push_back({b.x, b.y + b.height - botH2, leftW2, botH2});

    outParts.push_back({sb.x, sb.y + sb.height - topH, leftW, topH}); // G
    outParts.push_back({b.x, b.y, leftW2, topH2});

    Vector2f fullClip(b.x + b.width, b.y + b.height);
    for (int i = 0; i < 4; i++) {
        clipSliceRectsTopRight(outParts[outParts.size() - i * 2 - 2], outParts[outParts.size() - i * 2 - 1], fullClip);
        clipSliceRectsBottomLeft(outParts[outParts.size() - i * 2 - 2], outParts[outParts.size() - i * 2 - 1],
                                 botLeftClip);
    }
}

static bool EpsEqual ( float a, float b, float eps = 0.0001f ) {
    return a-eps <= b && a+eps >= b;
}

void
HTMLRendererPrivate::renderTiledSprite(EntityManager& man, DisplayListEntry& de)
{
    const Sprite2DRendererOptions* til = man.getComponentPtrConstUnsafe<Sprite2DRendererOptions>(de.e);

    if (til->drawMode == DrawMode::Stretch) {
        // this is also called for stretched sprites
        // in that case canvas renders the exact same way as normal sprites
        // bounds are already taken from DisplayListEntry
        renderNormalSpriteBatch(man, &de, 1);
        return;
    }

    // regular tiling, render via clipping path + pattern
    const Sprite2DRenderer* sr = man.getComponentPtrConstUnsafe<Sprite2DRenderer>(de.e);
    const Sprite2D* sprite = man.getComponentPtrConstUnsafe<Sprite2D>(sr->sprite);
    const Image2D* image = man.getComponentPtrConstUnsafe<Image2D>(sprite->image);
    const Image2DHTML* imageHtml = man.getComponentPtrConstUnsafe<Image2DHTML>(sprite->image);
    Sprite2DRendererHTML* spriteHtml = man.getComponentPtrUnsafe<Sprite2DRendererHTML>(de.e);

    const Rect& b = de.inBounds;
    Rect sb = fixupPixelRectangle(sprite->imageRegion, image->imagePixelSize);

    int patternIdx = updateOrCreatePattern(spriteHtml, imageHtml->imageIndex, sb, sr->color);

    float xscale = sprite->pixelsToWorldUnits;
    float yscale = sprite->pixelsToWorldUnits;
    if (til->drawMode == DrawMode::AdaptiveTiling) {
        // fix scale for adaptive
        float extrax = b.width / (sb.width * sprite->pixelsToWorldUnits);
        float extray = b.height / (sb.height * sprite->pixelsToWorldUnits);
        if (extrax > 1.0f) {
            float nrepsx = nextFullScale(extrax);
            extrax = b.width / (sb.width * sprite->pixelsToWorldUnits * nrepsx);
        }
        if (extray > 1.0f) {
            float nrepsy = nextFullScale(extray);
            extray = b.height / (sb.height * sprite->pixelsToWorldUnits * nrepsy);
        }
        xscale *= extrax;
        yscale *= extray;
    }

    float txa, txb, txc, txd, txe, txf;
    makeCanvasTransform(currentViewMatrix, UMtoM(de.finalMatrix), txa, txb, txc, txd, txe, txf);

    // outline rectangle transform first
    js_canvasSetTransformOnly(txa, txb, txc, txd, txe, txf);

    // pattern transform second
    txe += txa * b.x - txc * b.y;
    txf += txb * b.x - txd * b.y;
    txa *= xscale;
    txb *= xscale;
    txc *= yscale;
    txd *= yscale;

    // note: tiled pattern rendering will look slightly different to webgl due to rounding in pattern creation
    // draw
    setBlendingAndSmoothing(sr->blending, !image->disableSmoothing);
    js_canvasRenderPatternSprite(patternIdx, b.x, -b.height - b.y, b.width, b.height, txa, txb, txc, txd, txe, txf,
                                 sr->color.a);
}

void
HTMLRendererPrivate::setBlendingAndSmoothing(BlendOp blendop, bool smooth)
{
#ifdef DEVELOPMENT
    if (blendop==BlendOp::Multiply && !js_canvasSupportsMultiply()) {
        ut::logWarning("Attempting to use multiply blend mode on a device where canvas rendering does not support it.");
        blendop = BlendOp::Alpha;
    }
#endif
    js_canvasBlendingAndSmoothing((int)blendop,smooth);
}

void
HTMLRendererPrivate::renderSlicedSprite(EntityManager& man, DisplayListEntry& de)
{
    const Sprite2DRenderer* sr = man.getComponentPtrConstUnsafe<Sprite2DRenderer>(de.e);
    const Sprite2DBorder* border = man.getComponentPtrConstUnsafe<Sprite2DBorder>(sr->sprite);
    const Sprite2D* sprite = man.getComponentPtrConstUnsafe<Sprite2D>(sr->sprite);
    const Image2D* image = man.getComponentPtrConstUnsafe<Image2D>(sprite->image);
    const Image2DHTML* imageHtml = man.getComponentPtrConstUnsafe<Image2DHTML>(sprite->image);
    const Sprite2DRendererOptions* til = man.getComponentPtrConstUnsafe<Sprite2DRendererOptions>(de.e);
    Sprite2DRendererHTML* spriteHtml = man.getComponentPtrUnsafe<Sprite2DRendererHTML>(de.e);

    const Rect& b = de.inBounds;
    Rect sb = fixupPixelRectangle(sprite->imageRegion, image->imagePixelSize);

    float txa, txb, txc, txd, txe, txf;
    makeCanvasTransform(currentViewMatrix, UMtoM(de.finalMatrix), txa, txb, txc, txd, txe, txf);
    // outline rectangle transform first
    js_canvasSetTransformOnly(txa, txb, txc, txd, txe, txf);
    setBlendingAndSmoothing(sr->blending, !image->disableSmoothing);

    std::vector<Rect> parts;
    Rect sbInner;
    sbInner.x = sprite->imageRegion.x + sprite->imageRegion.width * border->bottomLeft.x;
    sbInner.y = sprite->imageRegion.y + sprite->imageRegion.height * border->bottomLeft.y;
    sbInner.width = sprite->imageRegion.width * (border->topRight.x - border->bottomLeft.x);
    sbInner.height = sprite->imageRegion.height * (border->topRight.y - border->bottomLeft.y);
    sbInner = fixupPixelRectangle(sbInner, image->imagePixelSize);

    int tintIndex = -1;
    if (!isWhite(sr->color)) {
        // need tint cache, adjust source rectangles
        tintIndex = updateOrCreateTintedSprite(spriteHtml, imageHtml->imageIndex, sb, sr->color);
        sbInner.x -= sb.x;
        sbInner.y -= sb.y;
        sb.x = 0;
        sb.y = 0;
    }
    // if is white use imageHtml->imageIndex directly

    // create many individual drawImage calls :/
    makeSlice9Parts(sb, sbInner, b, sprite->pixelsToWorldUnits, til->drawMode, parts);

    js_canvasRenderMultipleSliced(tintIndex, imageHtml->imageIndex, (const float*)parts.data(), parts.size() >> 1, sr->color.a);
}

void
HTMLRendererPrivate::makeCanvasTransform(const Matrix4x4f& lhs, const Matrix4x4f& rhs, float& txa, float& txb,
                                         float& txc, float& txd, float& txe, float& txf)
{
    /*
    Manually unrolled and unused removed:
    Matrix4x4f m;
    MultiplyMatrices4x4(&lhs, &de.finalMatrix, &m);
    txa = m.Get(0, 0);
    txb = m.Get(1, 0);
    txc = -m.Get(0, 1);
    txd = -m.Get(1, 1);
    txe = m.Get(0, 3);
    txf = m.Get(1, 3);
    */
    txa = lhs[0] * rhs[0] + lhs[4] * rhs[1] + lhs[8] * rhs[2] + lhs[12] * rhs[3];
    txb = lhs[1] * rhs[0] + lhs[5] * rhs[1] + lhs[9] * rhs[2] + lhs[13] * rhs[3];
    txc = -(lhs[0] * rhs[4] + lhs[4] * rhs[5] + lhs[8] * rhs[6] + lhs[12] * rhs[7]);
    txd = -(lhs[1] * rhs[4] + lhs[5] * rhs[5] + lhs[9] * rhs[6] + lhs[13] * rhs[7]);
    txe = lhs[0] * rhs[12] + lhs[4] * rhs[13] + lhs[8] * rhs[14] + lhs[12] * rhs[15];
    txf = lhs[1] * rhs[12] + lhs[5] * rhs[13] + lhs[9] * rhs[14] + lhs[13] * rhs[15];
}

void
HTMLRendererPrivate::renderNormalSpriteBatch(EntityManager& man, const DisplayListEntry* list, int n )
{
    const Sprite2DRenderer* sr = man.getComponentPtrConstUnsafe<Sprite2DRenderer>(list[0].e);
    const Sprite2D* sprite = man.getComponentPtrConstUnsafe<Sprite2D>(sr->sprite);
    const Image2D* image = man.getComponentPtrConstUnsafe<Image2D>(sprite->image);
    const Image2DHTML* imageHtml = man.getComponentPtrConstUnsafe<Image2DHTML>(sprite->image);

    setBlendingAndSmoothing(sr->blending, !image->disableSmoothing);

    for ( int i=0; i<n; i++ ) {
        const DisplayListEntry &de = list[i];
        if ( i>=1 ) {
            sr = man.getComponentPtrConstUnsafe<Sprite2DRenderer>(de.e);
            sprite = man.getComponentPtrConstUnsafe<Sprite2D>(sr->sprite);
        }

        const Rect &b = de.inBounds;
        Rect sb = fixupPixelRectangle(sprite->imageRegion, image->imagePixelSize);

        float txa, txb, txc, txd, txe, txf;
        makeCanvasTransform(currentViewMatrix, UMtoM(de.finalMatrix), txa, txb, txc, txd, txe, txf);

        if (isWhite(sr->color)) {
            // can render directly
            js_canvasRenderNormalSpriteWhite(
                txa, txb, txc, txd, txe, txf, sr->color.a,
                imageHtml->imageIndex, sb.x, sb.y, sb.width, sb.height, b.x, -b.height - b.y, b.width, b.height);
        } else {
            // go through tint cache
            Sprite2DRendererHTML* spriteHtml = man.getComponentPtrUnsafe<Sprite2DRendererHTML>(de.e);
            int tintIndex = updateOrCreateTintedSprite(spriteHtml, imageHtml->imageIndex, sb, sr->color);
            js_canvasRenderNormalSpriteTinted(
                txa, txb, txc, txd, txe, txf, sr->color.a,
                tintIndex, b.x, -b.height - b.y, b.width, b.height);
        }
    }
}

void
HTMLRendererPrivate::renderSpriteBatch(int n, DisplayListEntry* list, EntityManager& man)
{
    Assert(n > 0 && n <= /*sMaxBatchSize*/4096);
    // grab all the components of first one
    switch (list[0].type) {
    case DisplayListEntryType::HitBoxOnly:
    case DisplayListEntryType::GroupOnly:
        return;
    case DisplayListEntryType::Shape:
        for (int i = 0; i < n; i++)
            renderShape(man, list[i]);
        return;
    case DisplayListEntryType::Text:
        for (int i = 0; i < n; i++)
            renderText(man, list[i]);
        return;
    case DisplayListEntryType::TiledSprite:
        for (int i = 0; i < n; i++)
            renderTiledSprite(man, list[i]);
        return;
    case DisplayListEntryType::SlicedSprite:
        for (int i = 0; i < n; i++)
            renderSlicedSprite(man, list[i]);
        return;
    case DisplayListEntryType::Sprite:
        renderNormalSpriteBatch ( man, list, n);
        return;
    default:
        Assert(0);
        return;
    }
}

void
HTMLRendererPrivate::updateRTT(EntityManager& w)
{
    /*
    // initialize empty images - this could go into it's own system
    EntityCommandBuffer ecb;

    // add the native image component for things that need it for rtt
    w.forEach<Entity>({ComponentSpec::subtractive<Image2DHTML>(), ComponentSpec::create<Image2D>(),
                       ComponentSpec::create<Image2DRenderToTexture>()},
                      [&ecb](Entity& entity) {
                          ecb.addComponent<Image2DHTML>(entity);
                      }, BufferingMode::Unbuffered);
    ecb.commit(w);

    // add the native rtt component that is need for rtt
    w.forEach<Entity, Image2DHTML>({ComponentSpec::subtractive<Image2DRenderToTextureHTML>(),
                                    ComponentSpec::create<Image2D>(), ComponentSpec::create<Image2DRenderToTexture>()},
                                   [&ecb, &w](Entity& entity, Image2DHTML& ig) {
                                       ecb.addComponent<Image2DRenderToTextureHTML>(entity);
                                   }, BufferingMode::Unbuffered);
    ecb.commit(w);

    // remove the native rtt component and image if the api component is gone
    w.forEach<Entity, Image2DHTML, Image2DRenderToTextureHTML>(
        {ComponentSpec::subtractive<Image2DRenderToTexture>()},
        [&ecb, &w](Entity& entity, Image2DHTML& ig, Image2DRenderToTextureHTML& igrtt) {
            if (igrtt.displaying.imageIndex)
                js_canvasFreeRenderTarget(igrtt.displaying.imageIndex);
            if (igrtt.rendering.imageIndex)
                js_canvasFreeRenderTarget(igrtt.rendering.imageIndex);
            if (!ig.externalOwner && ig.imageIndex)
                js_canvasFreeImage(ig.imageIndex);
            if (w.hasComponent<Image2D>(entity))
                w.getComponentPtrUnsafe<Image2D>(entity)->status = ImageStatus::Invalid;
            ecb.removeComponent<Image2DRenderToTextureHTML>(entity);
            ecb.removeComponent<Image2DHTML>(entity);
        }, BufferingMode::Unbuffered);
    ecb.commit(w);
    */
}

static HTMLRendererPrivate sInst;

ZEROPLAYER_EXPORT
bool init_renderercanvas(void *emHandle) {
    EntityManager em(emHandle);    
    if (!sInst.init(em) )
        return false;
    return true;
}

ZEROPLAYER_EXPORT
void deinit_renderercanvas(void *emHandle) {
    EntityManager em(emHandle);
    sInst.deInit(em);
}

ZEROPLAYER_EXPORT
void begincamera_renderercanvas(void *emHandle, Entity ecam, float w, float h) {
    EntityManager em(emHandle);
    Vector2f targetSize(w,h);
    sInst.beginCamera(em, ecam, targetSize);
}

ZEROPLAYER_EXPORT
void endcamera_renderercanvas(void *emHandle, Entity ecam) {
    EntityManager em(emHandle);
    sInst.endCamera(em, ecam);
}

ZEROPLAYER_EXPORT
void beginscene_renderercanvas(void *emHandle, float w, float h) {
    EntityManager em(emHandle);
    Vector2f targetSize(w,h);
    sInst.beginScene(em, targetSize);
}

ZEROPLAYER_EXPORT
void endscene_renderercanvas(void *emHandle) {
    EntityManager em(emHandle);
    sInst.endScene(em);
}

ZEROPLAYER_EXPORT
void beginrtt_renderercanvas(void *emHandle, Entity ecam, const Camera2DRenderToTexture *rtt) {
    EntityManager em(emHandle);
    sInst.beginRenderToTexture(em,ecam,rtt);
}

ZEROPLAYER_EXPORT
void endrtt_renderercanvas(void *emHandle, Entity ecam, const Camera2DRenderToTexture *rtt) {
    EntityManager em(emHandle);
    sInst.endRenderToTexture(em,ecam,rtt);
}

ZEROPLAYER_EXPORT
void drawbatch_renderercanvas(void *emHandle, int n, DisplayListEntry *batch) {
    EntityManager em(emHandle);
    sInst.renderSpriteBatch(n, batch, em);
}

ZEROPLAYER_EXPORT
void text_releaseTintedGlyphs(void *emHandle, Entity entity) {
    EntityManager em(emHandle);
    sInst.releaseTintedGlyphs(em, entity);
}



/*
void
Rendering::RendererCanvas::removed(Scheduler& s, EntityManager& w)
{
    HTMLRendererPrivate::get()->deInit(w);
}

void
Rendering::RendererCanvas::update(Scheduler& s, EntityManager& w)
{
    DisplayInfo di = w.getConfigData<DisplayInfo>();
    if (di.renderMode!=RenderMode::Canvas) {
        HTMLRendererPrivate::get()->deInit(w);
        return;
    }
    if (HTMLRendererPrivate::get()->init(w))
        HTMLRendererPrivate::get()->run(w, di.width, di.height);
}*/

static int GetGlyphDescIndex(TintedGlyphHTML* buffer, int length, uint32_t value)
{
    for (int i = 0; i < length; ++i)
    {
        TintedGlyphHTML g = *(buffer + i);
        if (g.value == value) {
            return i;
        }
    }
    return -1;
}

void HTMLRendererPrivate::releaseTintedGlyphs(EntityManager& man, Entity entity)
{
    TintedGlyphHTML* buffer = man.getComponentPtrUnsafe<TintedGlyphHTML>(entity);
    int length = man.getBufferElementDataLength<TintedGlyphHTML>(entity);
    for (int i = 0; i < length; ++i)
    {
        TintedGlyphHTML g = *(buffer + i);
        if (g.tintedIndex > 0)
            js_canvasReleaseTintedSprite(g.tintedIndex);
    }
}
