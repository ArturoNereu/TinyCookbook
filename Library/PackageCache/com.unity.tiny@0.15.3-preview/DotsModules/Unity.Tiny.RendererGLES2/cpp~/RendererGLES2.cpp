#include <emscripten.h>
#include <emscripten/html5.h>
#include <GLES2/gl2.h>

#include "bind-Unity_Tiny_HTML.h"
#include "bind-Unity_Tiny_Core2D.h"
#include "bind-Unity_Tiny_Image2D.h"
#include "bind-Unity_Tiny_Image2DIOHTML.h"
#include "bind-Unity_Tiny_Text.h"
#include "bind-Unity_Tiny_TextHTML.h"
#include "bind-Unity_Tiny_Shape2D.h"
#include "bind-Unity_Tiny_RendererGLES2.h"

#include "zeroplayer.h"
#include "EntityWrappers.h"
#include "GeminiMath.h"

#include <string>
#include <vector>
#include <stdint.h>

#include "RendererGL.h"
#include "RendererGLES2Lib.h"
#include "CoreHelpers.h"

using namespace ut;
using namespace ut::Core2D;
using namespace Unity::Entities;
using namespace Unity::Tiny::Core2D;
using namespace Unity::Tiny::HTML;
using namespace Unity::Tiny::Text;
using namespace Unity::Tiny::TextHTML;
using namespace Unity::Tiny::Rendering;
using namespace Unity::Tiny::Core;

#if 0 // too slow even for debug :/
#define AssertGL() Assert(glGetError()==GL_NO_ERROR);
#else
#define AssertGL()
#endif

class ES2RendererPrivate : public RendererPrivateGL
{
public:
    bool init(EntityManager& w);
    void deInit(EntityManager& w);

    static const int sNBufferVertexBuffers = 4;                 // number of vertex buffers per size class, must be power of 2 
    static const int sMaxVertexBufferSize = sMaxBatchSize*4*64; // max size of vertex buffers, must be power of 2
    static const int sMinVertexBufferSize = 4*64;               // one sprite
    static const int sMaxSizeClasses = 32;                      // max num size classes, must be able to fit at least all powers of 2 between sMinVertexBufferSize and sMaxVertexBufferSize

    // size class 0 = sMaxVertexBufferSize
    // size class i = sMaxVertexBufferSize >> i
    struct WebGLVertexBufferSizeClass {
        unsigned int glid[sNBufferVertexBuffers];           // gl id of buffer
        int currentIndex;                                   // index into jsIndex array of the last recently used buffer to use 
        int sizeBytes;                                      // size of buffers in class
    } vertexBufferSizeClasses[sMaxSizeClasses];

    // maxSolidVertices * 4 * 2   -> solid        524288 -> use separate buffer with subData, not optimized 
    // maxBatchSize * 4 * 64      -> sprites      131072
    bool useBufferSubData; // run either using bufferSubData (good on webgl2 and desktop) or using full buffer uploads with double buffered size classes (needed for ios, older mobile)
    bool initialized;
    float currentCam[3];
    float currentViewScaleX; // for generating the text texture canvas at the right scale (font size, and canvas size)
    bool inrtt;

    ES2RendererPrivate() : initialized(false), cxHandleEM(0) {}

    void beginScene(EntityManager &man, const Vector2f &targetSize);
    void endScene(EntityManager &man);
    void beginCamera(EntityManager &man, Entity e, const Vector2f& targetSize);
    void endCamera(EntityManager &man, Entity e);
    void renderSpriteBatch(int n, DisplayListEntry* list, EntityManager& world);
    bool beginRenderToTexture(EntityManager& man, Entity e, const Camera2DRenderToTexture *rtt);
    void endRenderToTexture(EntityManager& man, Entity e, const Camera2DRenderToTexture *rtt);

    void renderSpriteTiled(EntityManager& man, DisplayListEntry& de);
    void renderSpriteSliced(EntityManager& man, DisplayListEntry& de);
    void renderShape(EntityManager& man, DisplayListEntry& de);
    void renderSprites(EntityManager& man, int n, DisplayListEntry* list);
    void renderText(EntityManager& man, DisplayListEntry& de);
    void renderTextWithNativeFont(EntityManager& man, DisplayListEntry& de);
    void renderTextWithBitmapFont(EntityManager& man, DisplayListEntry& de);
    const WebGLVertexBufferSizeClass& findSizeClassAndFlipBuffer(int sizeNeeded);
    void updateRTT(EntityManager& w);

    void setupBlending(BlendOp blendmode, bool hasAlpha);
    bool initBuffers();
    void bindSpriteBuffers();
    void bindSpriteBuffersWithSizeClass(const float *vertexBuffer, int n);
    void bindSolidBuffers();
    void bindTexture2D(Image2DGLES2 *tex);
    void bindTexture2D(unsigned int id);
    void enableArrays(unsigned int mask);
    void useProgram(ShaderProgram &p);
    void uploadTextureFromImageIfNeeded(Image2D* image, Image2DHTML* imagehtml, Image2DGLES2 *imagees2);
    void freeTexture(Image2DGLES2* imagees2);
    unsigned int allocVertexBuffer(int sizeInBytes);
    static uint32_t CheckSumString(const uint16_t *str);

    bool checkGL();

    BasicShader basicShader;
    SolidShapeShader solidShapeShader;
    TilingShader tilingShader;
    SlicingShader slicingShader;
    ShaderProgram presentShader;

    unsigned int vertexBufferGL;
    unsigned int indexBufferGL;
    unsigned int vertexBufferSolidGL;
    unsigned int indexBufferSolidGL;

    // state cache
    unsigned int curTexture;
    unsigned int curShader;
    unsigned int curVertexBuffer;
    unsigned int curIndexBuffer;
    unsigned int curAttribArrayEnabled;
    BlendOp curBlendOp;

    // emscripten view of context
    EMSCRIPTEN_WEBGL_CONTEXT_HANDLE cxHandleEM;
};

struct Image2DRenderToTextureWebGL : ISystemStateComponentData
{
    struct
    {
        unsigned int imageIndex;
        int w, h;
    } rendering, displaying;
};

void
ES2RendererPrivate::beginScene(EntityManager &man, const Vector2f& targetSize)
{
    Assert(initialized);
    // init global default state
    curBlendOp = BlendOp::Disabled;
    glDisable(GL_BLEND);
    curTexture = -1;
    curShader = -1;
    curVertexBuffer = -1;
    curIndexBuffer = -1;
    curAttribArrayEnabled = ~0x1f;
    enableArrays(0x1f); // make sure all are set, use 011111 as it is the default sprite one 
    glPixelStorei(0x9241/*GL_UNPACK_PREMULTIPLY_ALPHA_WEBGL*/,1);
    inrtt = false;
    AssertGL();
    updateRTT(man);
}

void
ES2RendererPrivate::endScene(EntityManager &man)
{
    // no need to reset, it's just us
}

bool
ES2RendererPrivate::beginRenderToTexture(EntityManager& man, Entity e, const Camera2DRenderToTexture *rtt)
{
    /*
    TODO MOVE TO C#
    if (!man.hasComponent<Image2DRenderToTextureWebGL>(rtt->target) ||
        !man.hasComponent<Image2DHTML>(rtt->target)) {
        Assert(0);
        return false;
    }
    Image2DHTML *destHTML = man.getComponentPtrUnsafe<Image2DHTML>(rtt->target);
    Image2DRenderToTextureWebGL *dest = man.getComponentPtrUnsafe<Image2DRenderToTextureWebGL>(rtt->target);
    Image2D *destImg = man.getComponentPtrUnsafe<Image2D>(rtt->target);
    // (re)allocate dest target if needed - this smoothly handles resolution changes and init
    if (dest->rendering.w != rtt->width || dest->rendering.h != rtt->height || !dest->rendering.imageIndex) {
        if (!dest->rendering.imageIndex)
            dest->rendering.imageIndex = js_webgl_makeRenderTarget(rtt->width, rtt->height, destImg->disableSmoothing);
        else
            js_webgl_resizeRenderTarget(dest->rendering.imageIndex, rtt->width, rtt->height);
        dest->rendering.w = rtt->width;
        dest->rendering.h = rtt->height;
    }
    js_webgl_setAsFrameBuffer(dest->rendering.imageIndex);
    inrtt = true;
    return true;
    */
    return false;
}

void
ES2RendererPrivate::endRenderToTexture(EntityManager& man, Entity e, const Camera2DRenderToTexture *rtt)
{
    /*
    TODO MOVE TO C#
    Image2DHTML *destHTML = man.getComponentPtrUnsafe<Image2DHTML>(rtt->target);
    Image2DRenderToTextureWebGL *dest = man.getComponentPtrUnsafe<Image2DRenderToTextureWebGL>(rtt->target);
    Image2D *destImg = man.getComponentPtrUnsafe<Image2D>(rtt->target);
    std::swap(dest->displaying, dest->rendering);
    // update image
    destImg->status = ImageStatus::Loaded; // now ready to use
    destImg->imagePixelSize.x = (float)dest->displaying.w;
    destImg->imagePixelSize.y = (float)dest->displaying.h;
    destImg->hasAlpha = true;
    // update native image to rendered target
    if (!destHTML->externalOwner && destHTML->imageIndex)
        js_webgl_freeTextureAndImage(destHTML->imageIndex);
    destHTML->externalOwner = true;
    destHTML->imageIndex = dest->displaying.imageIndex;
    // bind back original target
    js_webgl_setDefaulFrameBuffer();
    inrtt = false;
    */
}

void
ES2RendererPrivate::updateRTT(EntityManager& w)
{
    /*
    TODO MOVE TO C#
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
    w.forEach<Entity, Image2DHTML>({ComponentSpec::subtractive<Image2DRenderToTextureWebGL>(),
                                    ComponentSpec::create<Image2D>(), ComponentSpec::create<Image2DRenderToTexture>()},
                                   [&ecb, &w](Entity& entity, Image2DHTML& ig) {
                                       ecb.addComponent<Image2DRenderToTextureWebGL>(entity);
                                   }, BufferingMode::Unbuffered);
    ecb.commit(w);

    // remove the native rtt component and image if the api component is gone
    w.forEach<Entity, Image2DHTML, Image2DRenderToTextureWebGL>(
        {ComponentSpec::subtractive<Image2DRenderToTexture>()},
        [&ecb, &w](Entity& entity, Image2DHTML& ig, Image2DRenderToTextureWebGL& igrtt) {
            if (igrtt.displaying.imageIndex)
                js_webgl_freeTextureAndImage(igrtt.displaying.imageIndex);
            if (igrtt.rendering.imageIndex)
                js_webgl_freeTextureAndImage(igrtt.rendering.imageIndex);
            if (!ig.externalOwner && ig.imageIndex)
                js_webgl_freeTextureAndImage(ig.imageIndex);
            if (w.hasComponent<Image2D>(entity))
                w.getComponentPtrUnsafe<Image2D>(entity)->status = ImageStatus::Invalid;
            ecb.removeComponent<Image2DRenderToTextureWebGL>(entity);
            ecb.removeComponent<Image2DHTML>(entity);
        }, BufferingMode::Unbuffered);
    ecb.commit(w);
    */
}

void
ES2RendererPrivate::beginCamera(EntityManager &man, Entity e, const Vector2f& targetSize)
{
    const Camera2D* cam = man.getComponentPtrConstUnsafe<Camera2D>(e);
    Vector2f actualTargetSize;

    if ((cam->rect.width <= 0.0f || cam->rect.height <= 0.0f) || (cam->rect.width>=1.0f && cam->rect.height>=1.0f)) {
        glDisable(GL_SCISSOR_TEST);
        glViewport(0,0,targetSize.x, targetSize.y);
        AssertGL();
        actualTargetSize = targetSize;
    } else {
        Rectf targetRect(0, 0, targetSize.x, targetSize.y);
        Rectf clipRect = Rectf(cam->rect.x * targetSize.x, cam->rect.y * targetSize.y, cam->rect.width * targetSize.x,
                       cam->rect.height * targetSize.y);
        if (inrtt)
            clipRect.y = targetSize.y - clipRect.height - clipRect.y;
        // needs scissor and viewport, viewport alone does not clip
        clipRect.Clamp(targetRect);
        actualTargetSize = Vector2f(clipRect.width, clipRect.height);
        glEnable(GL_SCISSOR_TEST);
        glScissor(clipRect.x, clipRect.y, clipRect.width, clipRect.height);
        glViewport(clipRect.x, clipRect.y, clipRect.width, clipRect.height);
        AssertGL();
    }
    if (cam->clearFlags == CameraClearFlags::SolidColor) {
        float a = cam->backgroundColor.a;
        glClearColor (cam->backgroundColor.r * a, cam->backgroundColor.g * a, cam->backgroundColor.b * a, a);
        glClear(GL_COLOR_BUFFER_BIT);
    }

    currentCam[0] =
        actualTargetSize.y /
        (cam->halfVerticalSize * actualTargetSize.x); // 2/w -> pixels to normalized, then uniform scale by (h/2)/halfH
    currentCam[1] = 1.0f / cam->halfVerticalSize;
    currentCam[2] = 1.0f;
    if(inrtt)
        currentCam[1] = -currentCam[1];

    currentViewScaleX = actualTargetSize.x * .5f * currentCam[0];
    AssertGL();
}

void
ES2RendererPrivate::endCamera(EntityManager &man, Entity e)
{
}

void
ES2RendererPrivate::renderShape(EntityManager& man, DisplayListEntry& de)
{
    const Shape2DRenderer* sr = man.getComponentPtrConstUnsafe<Shape2DRenderer>(de.e);
    const Shape2DVertex* vertices = (Shape2DVertex*)man.getBufferElementDataPtrConstUnsafe<Shape2DVertex>(sr->shape);
    int verticesCount = man.getBufferElementDataLength<Shape2DVertex>(sr->shape);
    Assert((int)verticesCount <= sMaxSolidVertices);
    int indicesCount = 0;
    const uint16_t* indices = 0;
    if (man.hasComponent<Shape2DIndex>(sr->shape)) {
        indicesCount = man.getBufferElementDataLength<Shape2DIndex>(sr->shape);
        indices = (const uint16_t*)man.getBufferElementDataPtrConstUnsafe<Shape2DIndex>(sr->shape);
    }

    // setup drawing state
    setupBlending(sr->blending, sr->color.a != 1.0f);
    useProgram(solidShapeShader);
    float cpre[4];
    cpre[0] = sr->color.r * sr->color.a;
    cpre[1] = sr->color.g * sr->color.a;
    cpre[2] = sr->color.b * sr->color.a;
    cpre[3] = sr->color.a;
    glUniform4fv(solidShapeShader.u_color, 1, cpre);
    glUniform3fv(solidShapeShader.u_camera, 1, currentCam);

    // Convert matrix 4x4 to matrix 3x3.
    auto fm = de.finalMatrix.data;
    float mat[9] =
    {
        fm[00], fm[01], fm[02],
        fm[04], fm[05], fm[06],
        fm[12], fm[13], 1
    };
    glUniformMatrix3fv(solidShapeShader.u_trMatrix, 1, false, mat);

    bindSolidBuffers();
    glBufferSubData(GL_ARRAY_BUFFER, 0, verticesCount << 3, vertices);
    if (!indices) {
        glDrawArrays(GL_TRIANGLE_FAN, 0, (GLsizei)verticesCount);
    } else {
        glBufferSubData(GL_ELEMENT_ARRAY_BUFFER, 0, indicesCount<<1, indices);
        glDrawElements(GL_TRIANGLES, indicesCount, GL_UNSIGNED_SHORT, 0);
    }
}

void
ES2RendererPrivate::renderSpriteSliced(EntityManager& man, DisplayListEntry& de)
{
    const Sprite2DRendererOptions* til = man.getComponentPtrConstUnsafe<Sprite2DRendererOptions>(de.e);
    const Sprite2DRenderer* sr = man.getComponentPtrConstUnsafe<Sprite2DRenderer>(de.e);
    const Sprite2D* sprite = man.getComponentPtrConstUnsafe<Sprite2D>(sr->sprite);
    const Sprite2DBorder* border = man.getComponentPtrConstUnsafe<Sprite2DBorder>(sr->sprite);
    Image2D* image = man.getComponentPtrUnsafe<Image2D>(sprite->image);
    Image2DHTML* imagehtml = man.getComponentPtrUnsafe<Image2DHTML>(sprite->image);
    Image2DGLES2* imagees2 = man.getComponentPtrUnsafe<Image2DGLES2>(sprite->image);

    addQuadVertices(sr->color, de, sprite, vertexBuffer);

    // compute uniforms
    Slice9Constants c;
    computeSlice9Constants(image, border, sprite, de, til, c);

    // draw!
    setupBlending(sr->blending, image->hasAlpha || sr->color.a != 1.0f);
    uploadTextureFromImageIfNeeded(image, imagehtml, imagees2);
    bindTexture2D(imagees2);

    useProgram(slicingShader);
    glUniform3fv(slicingShader.u_camera, 1, currentCam); 
    glUniform4fv(slicingShader.u_border, 1, (const float*)&c.border );
    glUniform4fv(slicingShader.u_uvmod_bl, 1, (const float*)&c.uvmod_bl);
    glUniform4fv(slicingShader.u_uvmod_tr, 1, (const float*)&c.uvmod_tr);
    glUniform4fv(slicingShader.u_tilerect, 1, (const float*)&c.tilerect);
    glUniform4fv(slicingShader.u_innertexrect, 1, (const float*)&c.innertexrect);

    if (useBufferSubData) {
        bindSpriteBuffers();
        glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(Vertex)*4, vertexBuffer);
    } else {
        bindSpriteBuffersWithSizeClass((const float*)&vertexBuffer, 1);
    }
    glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_SHORT, 0); 
    AssertGL();
}

void
ES2RendererPrivate::renderSpriteTiled(EntityManager& man, DisplayListEntry& de)
{
    // tiling is single draw
    const Sprite2DRendererOptions* til = man.getComponentPtrConstUnsafe<Sprite2DRendererOptions>(de.e);
    const Sprite2DRenderer* sr = man.getComponentPtrConstUnsafe<Sprite2DRenderer>(de.e);
    const Sprite2D* sprite = man.getComponentPtrConstUnsafe<Sprite2D>(sr->sprite);
    Image2D* image = man.getComponentPtrUnsafe<Image2D>(sprite->image);
    Image2DHTML* imagehtml = man.getComponentPtrUnsafe<Image2DHTML>(sprite->image);
    Image2DGLES2* imagees2 = man.getComponentPtrUnsafe<Image2DGLES2>(sprite->image);
    addQuadVertices(sr->color, de, sprite, vertexBuffer);

    // setup tiling uniforms
    Vector4f tilerect;
    computeTileConstants(image, sprite, de, til, tilerect);

    // draw!
    setupBlending(sr->blending, image->hasAlpha || sr->color.a != 1.0f);
    uploadTextureFromImageIfNeeded(image, imagehtml, imagees2);
    bindTexture2D(imagees2);

    useProgram(tilingShader);
    glUniform3fv(tilingShader.u_camera, 1, currentCam);
    glUniform4fv(tilingShader.u_tilerect, 1, (const float*)&tilerect);

    if (useBufferSubData) {
        bindSpriteBuffers();
        glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(Vertex)*4, vertexBuffer);
    } else {
        bindSpriteBuffersWithSizeClass((const float*)&vertexBuffer, 1);
    }
    glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_SHORT, 0); 
    AssertGL();
}

void
ES2RendererPrivate::setupBlending(BlendOp blendmode, bool hasAlpha)
{
    if (!hasAlpha && blendmode == BlendOp::Alpha)
        blendmode =BlendOp::Disabled;
    if (curBlendOp == blendmode)
        return;
    curBlendOp = blendmode;
    switch (blendmode) {
        case BlendOp::Disabled: // no-alpha in color and image
            glDisable(GL_BLEND);
            break;
        case BlendOp::Alpha:
            glEnable(GL_BLEND);
            glBlendFuncSeparate(GL_ONE, GL_ONE_MINUS_SRC_ALPHA, GL_ONE, GL_ONE);
            break;
        case BlendOp::Add:
            glEnable(GL_BLEND);
            glBlendFuncSeparate(GL_ONE, GL_ONE, GL_ONE, GL_ONE);
            break;
        case BlendOp::Multiply:
            glEnable(GL_BLEND);
            glBlendFuncSeparate(GL_DST_COLOR, GL_ONE_MINUS_SRC_ALPHA, GL_ONE, GL_ONE);
            break;
        case BlendOp::MultiplyAlpha:
            glEnable(GL_BLEND);
            glBlendFuncSeparate(GL_ZERO, GL_SRC_ALPHA, GL_ZERO, GL_SRC_ALPHA);
            break;
    }
}

void
ES2RendererPrivate::uploadTextureFromImageIfNeeded(Image2D* image, Image2DHTML* imagehtml, Image2DGLES2 *imagees2)
{
    if (imagees2->glTexId)
        return;
    glGenTextures (1, &imagees2->glTexId);
    glBindTexture (GL_TEXTURE_2D, imagees2->glTexId);
    bool smooth = !image->disableSmoothing;
    ut::log( "Uploading texture %i from HTML image %i.\n", imagees2->glTexId, imagehtml->imageIndex );
    js_texImage2D_from_html_image(imagehtml->imageIndex);
    glTexParameteri (GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, smooth?GL_LINEAR:GL_NEAREST);
    glTexParameteri (GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, smooth?GL_LINEAR:GL_NEAREST);
    glTexParameteri (GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri (GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    curTexture = imagees2->glTexId;
}

void
ES2RendererPrivate::bindTexture2D(Image2DGLES2 *tex)
{
    bindTexture2D(tex->glTexId);
}

void
ES2RendererPrivate::bindTexture2D(unsigned int id)
{
    if (id!=curTexture) {
        glBindTexture(GL_TEXTURE_2D, id);
        curTexture = id;
    }
}


void
ES2RendererPrivate::useProgram(ShaderProgram& p)
{
    if (p.id()!=curShader) {
        glUseProgram(p.id());
        curShader = p.id(); 
    }
}

void
ES2RendererPrivate::renderSprites(EntityManager& man, int n, DisplayListEntry* list)
{
    if (n == 0)
        return;

    // render batch of sprites
    const Sprite2DRenderer* sr = man.getComponentPtrConstUnsafe<Sprite2DRenderer>(list[0].e);
    const Sprite2D* sprite = man.getComponentPtrConstUnsafe<Sprite2D>(sr->sprite);
    Image2D* image = man.getComponentPtrUnsafe<Image2D>(sprite->image);
    Image2DHTML* imagehtml = man.getComponentPtrUnsafe<Image2DHTML>(sprite->image);
    Image2DGLES2* imagees2 = man.getComponentPtrUnsafe<Image2DGLES2>(sprite->image);

    // build vertex buffer
    for (int i = 0; i < n; i++) {
        Entity ei = list[i].e;
        const Sprite2DRenderer* sir = man.getComponentPtrConstUnsafe<Sprite2DRenderer>(ei);
        const Sprite2D* s = man.getComponentPtrConstUnsafe<Sprite2D>(sir->sprite);
        addQuadVertices(sir->color, list[i], s, vertexBuffer + i * 4);
    }

    // draw!
    setupBlending(sr->blending, image->hasAlpha || sr->color.a != 1.0f);
    uploadTextureFromImageIfNeeded(image, imagehtml, imagees2);
    bindTexture2D(imagees2);
    useProgram(basicShader);
    glUniform3fv(basicShader.u_camera, 1, currentCam);
   
    if (useBufferSubData) {
        bindSpriteBuffers();
        glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(Vertex)*n*4, vertexBuffer);
    } else {
        bindSpriteBuffersWithSizeClass((const float*)&vertexBuffer, n);
    }
    glDrawElements(GL_TRIANGLES, n * 6, GL_UNSIGNED_SHORT, 0); // 6 indices per sprite

    AssertGL();
}

const ES2RendererPrivate::WebGLVertexBufferSizeClass&
ES2RendererPrivate::findSizeClassAndFlipBuffer(int sizeNeeded)
{
    Assert(!useBufferSubData);
    Assert(sizeNeeded > 0);

    // find size class and buffer
    int sizeInBuffer = sMaxVertexBufferSize;
    // could do better with an msb count
    int idx = 0;
    while ((sizeInBuffer>>1) >= sizeNeeded) {
        sizeInBuffer>>=1;
        idx++;
    }
    WebGLVertexBufferSizeClass &v = vertexBufferSizeClasses[idx];
    Assert(v.sizeBytes==sizeInBuffer);
    v.currentIndex = (v.currentIndex+1)&(sNBufferVertexBuffers-1);
    return v;
}

bool
static MatchStringWithBuffer(const uint16_t *s1, int s1len, const uint16_t *sz2) {
    for (;;) {
        if (s1len<=0)
            return *sz2==0;
        if (*sz2==0)
            return false;
        if (*s1 != *sz2)
            return false;
        s1len--;
        s1++;
        sz2++;
    }
}

bool
static CopyStringFromBuffer(uint16_t *s1, int s1maxlen, int &s1len, const uint16_t *sz2) {
    s1len = 0;
    for (;;) {
        if (s1len>=s1maxlen)
            return false;
        if (*sz2==0)
            return true;
        s1[s1len] = *sz2;
        sz2++;
        s1len++;
    }
}

void
ES2RendererPrivate::renderTextWithNativeFont(EntityManager& man, DisplayListEntry& de)
{
    const Text2DRenderer* textRenderer = man.getComponentPtrConstUnsafe<Text2DRenderer>(de.e);
    Image2DGLES2* imagees2 = man.getComponentPtrUnsafe<Image2DGLES2>(de.e);
    const Text2DStyle* style = man.getComponentPtrConstUnsafe<Text2DStyle>(textRenderer->style);
    const Text2DStyleNativeFont* nfStyle = man.getComponentPtrConstUnsafe<Text2DStyleNativeFont>(de.e);
    const Text2DPrivateNative* textPrivate = man.getComponentPtrConstUnsafe<Text2DPrivateNative>(de.e);
    Text2DPrivateCacheHTML* textHTMLES2 = man.getComponentPtrUnsafe<Text2DPrivateCacheHTML>(de.e);

    Matrix4x4f finalMatrix;
    std::memcpy(static_cast<void*>(&finalMatrix), static_cast<void*>(&de.finalMatrix), 16 * sizeof(float));

    // The checks to know if we should recreate a new texture via canvas have already been made in the text measurement system,
    // if we need to recreate it, the cache index should be invalidated at this point (-1)
    if (textHTMLES2->cacheIndex < 0)
    {
        static const float maxTextureSize = 2048.f;
        float scale = currentViewScaleX * finalMatrix.MaxAbsScale();
        float width = textPrivate->bounds.width * scale;
        float height = textPrivate->bounds.height * scale;

        if (width > height && width > maxTextureSize)
        {
            scale = scale * (maxTextureSize / width);
            float ratio = height / width;
            width = maxTextureSize;
            height = width * ratio;
        }
        else if (height > width && height > maxTextureSize)
        {
            scale = scale * (maxTextureSize / height);
            float ratio = width / height;
            height = maxTextureSize;
            width = height * ratio;
        }
        float fontSize = textPrivate->size * scale;

        glGenTextures(1, &imagees2->glTexId);// gen new texture

        //String are UTf16 encoded in c# in 16 bits
        uint16_t* textBuffer = (uint16_t*)man.getBufferElementDataPtrUnsafe<TextString>(de.e);
        int textLength = man.getBufferElementDataLength<TextString>(de.e);
        uint16_t* familyBuffer = (uint16_t*)man.getBufferElementDataPtrUnsafe<TextPrivateFontName>(de.e);
        int familyLength = man.getBufferElementDataLength<TextPrivateFontName>(de.e);

        glBindTexture(GL_TEXTURE_2D, imagees2->glTexId);
        curTexture = imagees2->glTexId;
        textHTMLES2->cacheIndex = (int)imagees2->glTexId;
#ifdef DEBUG
        char temp[4096] = { 0 };
        for (int i = 0; textBuffer[i] && i<4095; i++)
            temp[i] = (char)textBuffer[i];

        ut::log("Uploading text texture %i from HTML \"%s\" \n", textHTMLES2->cacheIndex, temp);
#endif

        //Since c# strings don't have a null terminated character, and some browsers doesnt support it.
        //Let's make sure the text/family buffers are length sized and have a null terminated character
        std::vector<uint16_t> text(textLength + 1);
        copyBufferAndAppendNull(text.data(), textBuffer, textLength);

        std::vector<uint16_t> family(familyLength + 1);
        copyBufferAndAppendNull(family.data(), familyBuffer, familyLength);
           
        js_texImage2D_from_html_text(
            text.data(), family.data(), fontSize,
            nfStyle->weight,
            nfStyle->italic,
            width,
            height);

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    }

    // gen vertices
    Rectf rec;
    rec.x = de.inBounds.x - de.inBounds.width * textRenderer->pivot.x;
    rec.y = de.inBounds.y - de.inBounds.height * textRenderer->pivot.y;
    rec.width = de.inBounds.width;
    rec.height = de.inBounds.height;

    addTextQuadVertices(textRenderer, finalMatrix, rec, nullptr, style->color, vertexBuffer); 

    // render
    setupBlending(textRenderer->blending, true); //The texture is generated from canvas and always contains alpha.
    bindTexture2D(imagees2);
    useProgram(basicShader);
    glUniform3fv(basicShader.u_camera, 1, currentCam);
   
    if (useBufferSubData) {
        bindSpriteBuffers();
        glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(Vertex)*4, vertexBuffer);
    } else {
        bindSpriteBuffersWithSizeClass((const float*)&vertexBuffer, 1);
    }
    glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_SHORT, 0); // 6 indices per sprite
}

void
ES2RendererPrivate::renderTextWithBitmapFont(EntityManager& man, DisplayListEntry& de)
{
    int glyphCount = man.getBufferElementDataLength<GlyphPrivateBuffer>(de.e);
    if (glyphCount == 0)
        return;
    const Text2DRenderer* textRenderer = man.getComponentPtrConstUnsafe<Text2DRenderer>(de.e);
    const Text2DStyle* style = man.getComponentPtrConstUnsafe<Text2DStyle>(textRenderer->style);
    const Text2DPrivateBitmap* textPrivate = man.getComponentPtrConstUnsafe<Text2DPrivateBitmap>(de.e);
    const GlyphPrivate* glyphBuffer = (GlyphPrivate*)man.getBufferElementDataPtrConstUnsafe<GlyphPrivateBuffer>(de.e);
    const Text2DStyleBitmapFont* bitmapStyle = man.getComponentPtrConstUnsafe<Text2DStyleBitmapFont>(de.e);
    const BitmapFont* bitmapFont = man.getComponentPtrConstUnsafe<BitmapFont>(bitmapStyle->font);
    Image2DHTML* bitmapFontImageHTML = man.getComponentPtrUnsafe<Image2DHTML>(bitmapFont->textureAtlas);
    Image2D* bitmapFontImage = man.getComponentPtrUnsafe<Image2D>(bitmapFont->textureAtlas);
    Image2DGLES2* bitmapFontImageES2 = man.getComponentPtrUnsafe<Image2DGLES2>(bitmapFont->textureAtlas);
    Assert(bitmapFontImageHTML && bitmapFontImage);
    Assert(bitmapFontImageHTML->imageIndex > 0);

    Matrix4x4f finalMatrix;
    std::memcpy(static_cast<void*>(&finalMatrix), static_cast<void*>(&de.finalMatrix), 16 * sizeof(float));

    for (int i = 0; i < glyphCount; i++) {
        GlyphPrivate glyph = *(glyphBuffer + i);
        //Adjust position and dim per glyph according to pivot position and text origin (originY is baseline)
        float transX = de.inBounds.width * textRenderer->pivot.x;
        float transY = de.inBounds.height * textRenderer->pivot.y - std::abs(bitmapFont->descent);
        Rectf rec;
        rec.x = (glyph.position.x - glyph.ci.width / 2 - transX) * textPrivate->fontScale.x + de.inBounds.x;
        rec.y = (glyph.position.y - glyph.ci.height / 2 - transY) * textPrivate->fontScale.y + de.inBounds.y;
        rec.width = glyph.ci.width * textPrivate->fontScale.x;
        rec.height = glyph.ci.height * textPrivate->fontScale.y;

        addTextQuadVertices(textRenderer, finalMatrix, rec, &glyph, style->color, vertexBuffer + i * 4);
    }

    setupBlending(textRenderer->blending, true);
    uploadTextureFromImageIfNeeded(bitmapFontImage, bitmapFontImageHTML, bitmapFontImageES2);
    bindTexture2D(bitmapFontImageES2);

    useProgram(basicShader);
    glUniform3fv(basicShader.u_camera, 1, currentCam);
   
    if (useBufferSubData) {
        bindSpriteBuffers();
        glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(Vertex)*glyphCount*4, vertexBuffer);
    } else {
        bindSpriteBuffersWithSizeClass((const float*)&vertexBuffer, glyphCount);
    }
    glDrawElements(GL_TRIANGLES, glyphCount * 6, GL_UNSIGNED_SHORT, 0); 
}

void
ES2RendererPrivate::renderText(EntityManager& man, DisplayListEntry& de)
{
    if(man.hasComponent<Text2DStyleNativeFont>(de.e)) {
        renderTextWithNativeFont(man, de);
    } else if (man.hasComponent<Text2DStyleBitmapFont>(de.e)) {
        renderTextWithBitmapFont(man, de);
    }
}

void
ES2RendererPrivate::renderSpriteBatch(int n, DisplayListEntry* list, EntityManager& man)
{
    Assert(n > 0 && n <= sMaxBatchSize);
    // grab all the components
    switch (list[0].type) {
    case DisplayListEntryType::HitBoxOnly:
    case DisplayListEntryType::GroupOnly:
        return;
    case DisplayListEntryType::Shape:
        for (int i = 0; i < n; i++)
            renderShape(man, list[i]);
        return;
    case DisplayListEntryType::TiledSprite:
        for (int i = 0; i < n; i++)
            renderSpriteTiled(man, list[i]);
        return;
    case DisplayListEntryType::SlicedSprite:
        for (int i = 0; i < n; i++)
            renderSpriteSliced(man, list[i]);
        return;
    case DisplayListEntryType::Text:
        for (int i = 0; i < n; i++)
            renderText(man, list[i]);
        return;
    case DisplayListEntryType::Sprite:
        renderSprites(man, n, list);
        break;
    default:
        Assert(0);
        return;
    }
}

void
ES2RendererPrivate::freeTexture(struct Unity::Tiny::Rendering::Image2DGLES2* imagees2)
{
    if (imagees2 && imagees2->glTexId && !imagees2->externalOwner) {
        glDeleteTextures ( 1, &imagees2->glTexId );
        imagees2->glTexId = 0;
    }
}

static GLuint
compileShader(GLenum type, const char* const src)
{
    const char* start;
    if ((start = strstr(src, "//start")) == nullptr)
        start = src;
    GLuint shader = glCreateShader(type);
    GLsizei srcLen = (GLsizei)strlen(start);
    GLchar const* version = "#version 100\n";
    GLsizei verLen = (GLsizei)strlen(version);
    GLchar const* fpPrec = "precision highp float;\n";
    GLsizei fpPrecLen = (GLsizei)strlen(fpPrec);
    GLchar const* files[] = { version, fpPrec,    start };
    GLsizei lengths[]     = { verLen,  fpPrecLen, srcLen };
    glShaderSource(shader, 3, files, lengths);
    glCompileShader(shader);
    GLint status;
    glGetShaderiv(shader, GL_COMPILE_STATUS, &status);
    if (status == GL_FALSE) {
        GLint infoLength = 0;
        glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &infoLength);
        std::vector<char> logVec(infoLength + 1);
        glGetShaderInfoLog(shader, infoLength, nullptr, logVec.data());
        logVec[infoLength] = 0;
        ut::log("Shader compile failed: %s\n", logVec.data());
        glDeleteShader(shader);
        ut::log("Shader contents:\n--------------------\n%s\n====================\n", start);
        return 0;
    }
    AssertGL();
    return shader;
}

static bool
linkProgram(GLuint pgm, GLuint vertShader, GLuint fragShader)
{
    if ((pgm == 0) || (vertShader == 0) || (fragShader == 0))
        return false;
    glAttachShader(pgm, vertShader);
    glAttachShader(pgm, fragShader);
    glLinkProgram(pgm);
    GLint status;
    glGetProgramiv(pgm, GL_LINK_STATUS, &status);
    if (status == GL_FALSE) {
        GLint infoLength = 0;
        glGetProgramiv(pgm, GL_INFO_LOG_LENGTH, &infoLength);
        std::vector<char> logVec(infoLength + 1);
        glGetProgramInfoLog(pgm, infoLength, nullptr, logVec.data());
        logVec[infoLength] = 0;
        ut::log( "Shader link failed: %s\n", logVec.data());
        return false;
    }
    AssertGL();
    return true;
}

ES2RendererPrivate::ShaderProgram
createProgram(const char* const vertShader, const char* const fragShader)
{
    RendererPrivateGL::ShaderProgram r(glCreateProgram());

    GLuint vert = compileShader(GL_VERTEX_SHADER, vertShader);
    if (!vert)
        return RendererPrivateGL::ShaderProgram(0);

    GLuint frag = compileShader(GL_FRAGMENT_SHADER, fragShader);
    if (!frag)
        return RendererPrivateGL::ShaderProgram(0);

    glBindAttribLocation(r.id(), 0, "a_pos");
    glBindAttribLocation(r.id(), 1, "a_color");
    glBindAttribLocation(r.id(), 2, "a_matrow0");
    glBindAttribLocation(r.id(), 3, "a_matrow1");
    glBindAttribLocation(r.id(), 4, "a_texrect");

    // tile map case
    glBindAttribLocation(r.id(), 1, "a_uv");
    glBindAttribLocation(r.id(), 2, "a_tileidx");

    if (!linkProgram(r.id(), vert, frag)) {
        glDeleteProgram(r.id());
        r = RendererPrivateGL::ShaderProgram(0);
    }
    if (vert != 0)
        glDeleteShader(vert);
    if (frag != 0)
        glDeleteShader(frag);

    return r;
}

ES2RendererPrivate::BasicShader::BasicShader(ShaderProgram const& p)
    : ShaderProgram(p)
{
    u_camera = glGetUniformLocation(pgm_, "u_camera");
    GLint mainTexture = glGetUniformLocation(pgm_, "mainTexture");
    if (mainTexture >= 0) {
        glUseProgram(pgm_);
        glUniform1i(mainTexture, 0);
        glUseProgram(0);
    }
}

ES2RendererPrivate::SolidShapeShader::SolidShapeShader(ShaderProgram const& p)
    : BasicShader(p)
{
    u_color = glGetUniformLocation(pgm_, "u_color");
    u_trMatrix = glGetUniformLocation(pgm_, "u_trMatrix");
}

ES2RendererPrivate::TilingShader::TilingShader(ShaderProgram const& p)
    : BasicShader(p)
{
    u_tilerect = glGetUniformLocation(pgm_, "u_tilerect");
}

ES2RendererPrivate::SlicingShader::SlicingShader(ShaderProgram const& p)
    : BasicShader(p)
{
    u_border = glGetUniformLocation(pgm_, "u_border");
    u_uvmod_bl = glGetUniformLocation(pgm_, "u_uvmod_bl");
    u_uvmod_tr = glGetUniformLocation(pgm_, "u_uvmod_tr");
    u_tilerect = glGetUniformLocation(pgm_, "u_tilerect");
    u_innertexrect = glGetUniformLocation(pgm_, "u_innertexrect");
}

void
ES2RendererPrivate::enableArrays(unsigned int mask)
{
    if (curAttribArrayEnabled==mask)
        return;
    for (int i=0; i<8; i++) {
        if(((curAttribArrayEnabled>>i)&1)!=((mask>>i)&1)) {
            if (((mask>>i)&1)==0)
                glDisableVertexAttribArray(i);
            else
                glEnableVertexAttribArray(i);
        }
    }
    curAttribArrayEnabled = mask;
}

void
ES2RendererPrivate::bindSolidBuffers()
{
    if (curVertexBuffer!=vertexBufferSolidGL) {
        glBindBuffer(GL_ARRAY_BUFFER, vertexBufferSolidGL);
        curVertexBuffer=vertexBufferSolidGL;
        enableArrays(0x1);
        glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, 8, (const void*)0);
    }
    if (curIndexBuffer!=indexBufferSolidGL) {
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexBufferSolidGL);
        curIndexBuffer = indexBufferSolidGL;
    }
}

void
ES2RendererPrivate::bindSpriteBuffers()
{
    if (curVertexBuffer!=vertexBufferGL) {
        glBindBuffer(GL_ARRAY_BUFFER, vertexBufferGL);
        curVertexBuffer=vertexBufferGL;
        enableArrays(0x1f); // 00011111
        glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, 64, (const void*)0);
        glVertexAttribPointer(1, 4, GL_FLOAT, GL_FALSE, 64, (const void*)8);
        glVertexAttribPointer(2, 3, GL_FLOAT, GL_FALSE, 64, (const void*)24);
        glVertexAttribPointer(3, 3, GL_FLOAT, GL_FALSE, 64, (const void*)36);
        glVertexAttribPointer(4, 4, GL_FLOAT, GL_FALSE, 64, (const void*)48);
    }
    if (curIndexBuffer!=indexBufferGL) {
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexBufferGL);
        curIndexBuffer=indexBufferGL;
    }
}

void
ES2RendererPrivate::bindSpriteBuffersWithSizeClass(const float *vertexBuffer, int n)
{
    const WebGLVertexBufferSizeClass& v = findSizeClassAndFlipBuffer(n*64*4);
    glBindBuffer(GL_ARRAY_BUFFER, v.glid[v.currentIndex]);
    glBufferData(GL_ARRAY_BUFFER, v.sizeBytes, vertexBuffer, GL_STREAM_DRAW);
    curVertexBuffer = -1; // do not bother checking cache
    // need to re-specify layout unfortunately
    glVertexAttribPointer(0, 2, GL_FLOAT, false, 64, (const void*)0);
    glVertexAttribPointer(1, 4, GL_FLOAT, false, 64, (const void*)8);
    glVertexAttribPointer(2, 3, GL_FLOAT, false, 64, (const void*)24);
    glVertexAttribPointer(3, 3, GL_FLOAT, false, 64, (const void*)36);
    glVertexAttribPointer(4, 4, GL_FLOAT, false, 64, (const void*)48);
    enableArrays(0x1f); // 00011111
    if (curIndexBuffer!=indexBufferGL) {
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexBufferGL);
        curIndexBuffer=indexBufferGL;
    }
}

unsigned int
ES2RendererPrivate::allocVertexBuffer(int sizeInBytes) {
    unsigned int r = 0;
    glGenBuffers(1, &r);
    Assert(r);
    glBindBuffer(GL_ARRAY_BUFFER, r);
    glBufferData(GL_ARRAY_BUFFER, sizeInBytes, 0, GL_STREAM_DRAW);
    AssertGL();
    curVertexBuffer = -1;
    return r;
};

bool
ES2RendererPrivate::initBuffers() {
    // sprites static index buffer
    glGenBuffers(1, &indexBufferGL);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexBufferGL);
    std::vector<uint16_t> buf(6 * sMaxBatchSize);
    for (int i = 0; i < sMaxBatchSize; i++) {
        buf[i * 6] = i * 4;
        buf[i * 6 + 1] = i * 4 + 1;
        buf[i * 6 + 2] = i * 4 + 2;
        buf[i * 6 + 3] = i * 4;
        buf[i * 6 + 4] = i * 4 + 2;
        buf[i * 6 + 5] = i * 4 + 3;
    }
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, buf.size()*2, buf.data(), GL_STATIC_DRAW);
    // solid vertices
    glGenBuffers(1, &vertexBufferSolidGL);
    glBindBuffer(GL_ARRAY_BUFFER, vertexBufferSolidGL);
    glBufferData(GL_ARRAY_BUFFER, sMaxSolidVertices * 4 * 2, 0, GL_STREAM_DRAW); // sizeof(float)*(xy)
    // solid indices
    glGenBuffers(1, &indexBufferSolidGL);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexBufferSolidGL);
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, sMaxSolidIndices * 2, 0, GL_STREAM_DRAW); // sizeof(ushort)
    AssertGL();

    // size class buffers for when buffer sub data is slow
    if (!useBufferSubData) {
        ut::log( "Using size class based buffers.\n");
        static_assert(sMaxVertexBufferSize>>(sMaxSizeClasses-1) <= sMinVertexBufferSize,"Too few size classes.");
        for (int i=0; i<sMaxSizeClasses; i++) {
            int s = sMaxVertexBufferSize>>i;
            if (s<sMinVertexBufferSize)
                break;
            vertexBufferSizeClasses[i].currentIndex = 0;
            vertexBufferSizeClasses[i].sizeBytes = s;
            for ( int j=0; j<sNBufferVertexBuffers; j++ )
                vertexBufferSizeClasses[i].glid[j] = allocVertexBuffer(s);
        }
        // do not use other buffer
        vertexBufferGL = 0;
    } else {
        ut::log( "Using bufferSubData buffer.\n");
        // sprites vertex buffer
        glGenBuffers(1, &vertexBufferGL);
        glBindBuffer(GL_ARRAY_BUFFER, vertexBufferGL);
        glBufferData(GL_ARRAY_BUFFER, 64 * sMaxBatchSize * 4, 0, GL_STREAM_DRAW);
        // do not use other buffers
        memset(vertexBufferSizeClasses, 0, sizeof(vertexBufferSizeClasses));
    }
    AssertGL();
    curVertexBuffer = -1;
    curIndexBuffer = -1;
    return true;
}

bool
ES2RendererPrivate::init(EntityManager& w)
{
    if (initialized) {
        // to test context loss, put this in the console:
        //   var c = document.getElementById("UT_CANVAS").getContext('webgl').getExtension('WEBGL_lose_context');
        //   c.loseContext();
        //   ... wait a bit ...
        //   c.restoreContext();
        if ( emscripten_is_webgl_context_lost(cxHandleEM) ) {
            ut::log( "Lost WebGL context.\n");
            deInit(w);
            return false;
        }
        return true;
    }

    // check that canvas is actually webgl, and context version
    int glVersion = MAIN_THREAD_EM_ASM_INT({
        if (ut._HTML.canvasMode == 'webgl2') return 2;
        if (ut._HTML.canvasMode == 'webgl') return 1;
        return 0;
        });
    if (!glVersion)
        return false;

    // heuristic, might need to tighten this up for old android/mobile that has gl2 but
    // still slow bufferSubData
    useBufferSubData = glVersion==2;

    // make current
    EmscriptenWebGLContextAttributes attrs;
    emscripten_webgl_init_context_attributes(&attrs);
    attrs.depth = false;
    attrs.majorVersion = glVersion;
    cxHandleEM = emscripten_webgl_create_context("#UT_CANVAS", &attrs);
    if (cxHandleEM <= 0) {
        ut::log("webgl_create_context failed.\n");
        return false;
    }
    if (emscripten_webgl_make_context_current(cxHandleEM) != EMSCRIPTEN_RESULT_SUCCESS) {
        ut::log("webgl_make_context_current failed.\n");
        return false;
    }

    // init shaders
    if ((basicShader = createProgram(shaderSrcVertex, shaderSrcFragment)).id() == 0)
        return false;

    if ((tilingShader = createProgram(shaderSrcVertex, shaderSrcFragmentTiling)).id() == 0)
        return false;

    if ((solidShapeShader = createProgram(shaderSrcSolidVertex, shaderSrcSolidFragment)).id() == 0)
        return false;

    if ((slicingShader = createProgram(shaderSrcVertexSlicing, shaderSrcFragmentSlicing)).id() == 0)
        return false;

    AssertGL();
    if (!initBuffers()) {
        ut::log( "Buffers init failed.\n");
        return false;
    }
    AssertGL();

    InitComponentId<Image2D>();
    InitComponentId<Image2DHTML>();
    InitComponentId<Image2DRenderToTexture>();
    //InitComponentId<Image2DRenderToTextureHTML>();
    InitComponentId<Camera2D>();
    InitComponentId<Shape2DRenderer>();
    InitComponentId<Shape2DVertex>();
    InitComponentId<Shape2DIndex>();
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
    InitComponentId<TextPrivateFontName>();
    InitComponentId<Text2DPrivateBitmap>();
    InitComponentId<GlyphPrivateBuffer>();
    InitComponentId<BitmapFont>();
    InitComponentId<Image2DGLES2>();

    ut::log( "ES2 OpenGL init ok.\n");
    initialized = true;
    return true;
}

void
ES2RendererPrivate::deInit(EntityManager& w)
{
    if (!initialized)
        return;

    // remove private components 
    /*
    TODO c#
    EntityCommandBuffer ecb;
    w.forEach<Entity,Image2DRenderToTextureWebGL>([&ecb](Entity &e, Image2DRenderToTextureWebGL &rtt){
        if (rtt.displaying.imageIndex)
            js_webgl_freeTextureAndImage(rtt.displaying.imageIndex);
        if (rtt.rendering.imageIndex)
            js_webgl_freeTextureAndImage(rtt.rendering.imageIndex);
        ecb.removeComponent<Image2DRenderToTextureWebGL>(e);
    }, BufferingMode::Unbuffered);
    ecb.commit(w);
    */

    // delete all resources
    //EM_ASM_({ ut._HTML.freeAllGL(); }, 0);


    initialized = false;
}

// --------------------------------------------------------- BIND TO C# --------------------------------------------------

static ES2RendererPrivate sInst;

ZEROPLAYER_EXPORT
bool init_rendereres2(void *emHandle) {
    EntityManager em(emHandle);
    if (!sInst.init(em) )
        return false;
    return true;
}

ZEROPLAYER_EXPORT
void deinit_rendereres2(void *emHandle) {
    EntityManager em(emHandle);
    sInst.deInit(em);
}

ZEROPLAYER_EXPORT
void begincamera_rendereres2(void *emHandle, Entity ecam, float w, float h) {
    EntityManager em(emHandle);
    Vector2f targetSize(w,h);
    sInst.beginCamera(em, ecam, targetSize);
}

ZEROPLAYER_EXPORT
void endcamera_rendereres2(void *emHandle, Entity ecam) {
    EntityManager em(emHandle);
    sInst.endCamera(em, ecam);
}

ZEROPLAYER_EXPORT
void beginscene_rendereres2(void *emHandle, float w, float h) {
    EntityManager em(emHandle);
    Vector2f targetSize(w,h);
    sInst.beginScene(em, targetSize);
}

ZEROPLAYER_EXPORT
void endscene_rendereres2(void *emHandle) {
    EntityManager em(emHandle);
    sInst.endScene(em);
}

ZEROPLAYER_EXPORT
void beginrtt_rendereres2(void *emHandle, Entity ecam, const Camera2DRenderToTexture *rtt) {
    EntityManager em(emHandle);
    sInst.beginRenderToTexture(em,ecam,rtt);
}

ZEROPLAYER_EXPORT
void endrtt_rendereres2(void *emHandle, Entity ecam, const Camera2DRenderToTexture *rtt) {
    EntityManager em(emHandle);
    sInst.endRenderToTexture(em,ecam,rtt);
}

ZEROPLAYER_EXPORT
void drawbatch_rendereres2(void *emHandle, int n, DisplayListEntry *batch) {
    EntityManager em(emHandle);
    sInst.renderSpriteBatch(n, batch, em);
}

ZEROPLAYER_EXPORT
void freeimage_rendereres2(struct Unity::Tiny::Rendering::Image2DGLES2* imagees2) {
    sInst.freeTexture(imagees2);
}
