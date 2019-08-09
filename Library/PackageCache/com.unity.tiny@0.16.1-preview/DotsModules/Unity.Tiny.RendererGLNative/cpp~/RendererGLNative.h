#pragma once 

#include <stdint.h>
#include <glew.h>

#include "zeroplayer.h"

#include "RendererGL.h"
#include "bind-Unity_Tiny_TextNative.h"

using namespace Unity::Entities;
using namespace ut::Core2D;

namespace ut {
namespace GLFW {

class RenderTarget {
public:
    RenderTarget();
    ~RenderTarget();

    bool hasFBO() const { return fbo_!=0; }
    bool hasTexture() const { return renderTex_!=0; }
    void allocateFBO();
    void allocateTexture(int w, int h);
    void bindAndAttach(unsigned int tex);
    void bind();
    static void bindZero();
    //Image readFromGPU();
    void resize(int w, int h);
    void bindTexture();
    void reset();

private:
    GLuint fbo_, renderTex_;
    int w_, h_;
};

class GLFWLRendererPrivate : public RendererPrivateGL {
public:
    GLFWLRendererPrivate();
    ~GLFWLRendererPrivate();

    void beginScene(EntityManager& man, const Vector2f& targetSize);
    void endScene(EntityManager& man);
    void beginCamera(EntityManager& man, Entity e, const Vector2f& targetSize);
    void endCamera(EntityManager& man, Entity e);
    bool beginRenderToTexture(EntityManager& man, Entity e, const Unity::Tiny::Core2D::Camera2DRenderToTexture *rtt);
    void endRenderToTexture(EntityManager& man, Entity e, const Unity::Tiny::Core2D::Camera2DRenderToTexture *rtt);
    void renderSpriteBatch(int n, Unity::Tiny::Core2D::DisplayListEntry* list, EntityManager& world);
    bool init(EntityManager& w);
    void deInit(EntityManager& w);
    void setPresentBorder(int dx, int dy, int dw, int dh);
    bool isBadIntelDriver();
    uint32_t uploadNewTexture(int w, int h, const uint8_t *pixels, bool disableSmoothing);

    //Image readResultingImage();
private:
    void enableVertexBufferTileMaps();
    void enableVertexBuffer();
    void disableVertexBuffer();
    void renderSprites(EntityManager& man, int n, Unity::Tiny::Core2D::DisplayListEntry* list);

    void renderShape(EntityManager& man, Unity::Tiny::Core2D::DisplayListEntry& de);
    void renderSpriteSliced(EntityManager& man, Unity::Tiny::Core2D::DisplayListEntry& de);
    void renderSpriteTiled(EntityManager& man, Unity::Tiny::Core2D::DisplayListEntry& de);
    void renderTilemap(EntityManager& man, Unity::Tiny::Core2D::DisplayListEntry& de);
    void renderText(EntityManager& man, Unity::Tiny::Core2D::DisplayListEntry& de);
    void renderTextWithNativeFont(EntityManager& man, Unity::Tiny::Core2D::DisplayListEntry& de);
    void renderTextWithBitmapFont(EntityManager& man, Unity::Tiny::Core2D::DisplayListEntry& de);
    void uploadTextTexture(int fontHandle, Unity::Tiny::TextNative::Text2DPrivateCacheNative* textCache, const wchar_t* textBuffer, int textLength, float size, float width, float height);

    void updateRTT(EntityManager& w); // adds components

    void setupBlending(Unity::Tiny::Core2D::BlendOp blending, bool hasAlpha);

    bool initShaders();
    void initBuffers();

private:
    bool inrtt;
    bool initialized;
    uint16_t indexBuffer[sMaxBatchSize * 6];
    float currentCam[3];
    float currentViewScaleX; // for generating the text texture bitmap at the right scale (font size, and bitmap size)

    enum { VB_Quad = 0, VB_Default, VB_Tilemaps, VB_Shape2D, VB_Count };
    GLuint vaos[VB_Count], vbos[VB_Count], ibo;

    RenderTarget renderTarget;
    RenderTarget rttTarget;

    BasicShader basicShader;
    SolidShapeShader solidShapeShader;
    TilingShader tilingShader;

    SlicingShader slicingShader;
    TilemapShader tilemapShader;
    ShaderProgram presentShader;

    int presentX, presentY, presentW, presentH;
    int presentXd, presentYd, presentWd, presentHd;
    bool usePresentShader;
};

} // Core2D
} // ut
